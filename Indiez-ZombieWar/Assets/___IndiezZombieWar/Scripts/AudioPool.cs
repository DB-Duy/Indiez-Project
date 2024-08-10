using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioPool : MonoBehaviour
{
    public static AudioPool Instance;

    [SerializeField]
    private int poolSize = 10; // Number of audio sources to pool
    [SerializeField]
    private AudioSource audioSourcePrefab; // Prefab or template for the audio sources

    private List<AudioSource> audioSources; // List to store pooled audio sources

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        InitializePool();
    }

    private void InitializePool()
    {
        audioSources = new List<AudioSource>();

        for (int i = 0; i < poolSize; i++)
        {
            AudioSource newAudioSource = Instantiate(audioSourcePrefab, transform);
            newAudioSource.gameObject.SetActive(false);
            audioSources.Add(newAudioSource);
        }
    }

    public void PlayAudioClip(AudioClip clip, Vector3 position, float volume = 1f, float pitch = 1f)
    {
        AudioSource availableSource = GetAvailableAudioSource();
        if (availableSource != null)
        {
            availableSource.transform.position = position;
            availableSource.clip = clip;
            availableSource.volume = volume;
            availableSource.pitch = pitch;
            availableSource.gameObject.SetActive(true);
            availableSource.Play();
            StartCoroutine(DisableAfterPlay(availableSource));
        }
    }

    private AudioSource GetAvailableAudioSource()
    {
        foreach (var audioSource in audioSources)
        {
            if (!audioSource.isPlaying)
            {
                return audioSource;
            }
        }

        // If no available audio source is found, expand the pool (optional)
        AudioSource newAudioSource = Instantiate(audioSourcePrefab, transform);
        audioSources.Add(newAudioSource);
        return newAudioSource;
    }

    private IEnumerator DisableAfterPlay(AudioSource audioSource)
    {
        yield return new WaitForSeconds(audioSource.clip.length);
        audioSource.gameObject.SetActive(false);
    }
}
