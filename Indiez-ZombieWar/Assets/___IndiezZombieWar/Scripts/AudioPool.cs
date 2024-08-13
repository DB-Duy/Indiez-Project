using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class AudioPool : MonoBehaviour
{
    public static AudioPool Instance;

    [SerializeField]
    private int poolSize = 10; // Number of audio sources to pool
    [SerializeField]
    private AudioSource audioSourcePrefab; // Prefab or template for the audio sources

    private List<AudioSource> audioSources; // List to store pooled audio sources

    public AudioClip[] _zombieClip;
    public AudioClip _shootAR, _shootShotgun, _shootGrenade;
    public AudioClip[] _takeDamage;
    public AudioClip _eventStart, _gunUnlock;
    public AudioClip _reload, _switch;
    public AudioClip _die;
    public AudioClip _pickup;
    public AudioClip[] _step, _bossStep;
    public Dictionary<float, WaitForSeconds> _waitDict = new Dictionary<float, WaitForSeconds>();
    public AudioClip[] _bossSound;
    public AudioClip _nadeExplode;
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
    public void PlayNadeExplode()
    {
        PlayAudioClip(_nadeExplode, 1f, 1 + Random.value * 0.1f - 0.05f);
    }
    public void PlayBossSound()
    {
        AudioClip clip = _bossSound[Random.Range(0, _bossSound.Length)];
        PlayAudioClip(clip, 0.5f + Random.value * 0.5f, 1 + Random.value * 0.1f - 0.05f);
    }
    public void PlayZombieSound()
    {
        AudioClip clip = _zombieClip[Random.Range(0, _zombieClip.Length)];
        PlayAudioClip(clip, 0.5f + Random.value * 0.5f, 1 + Random.value * 0.1f - 0.05f);
    }
    public void PlayStep()
    {
        //AudioClip clip = _step[Random.Range(0, _step.Length)];
        //PlayAudioClip(clip, 0.5f + Random.value * 0.5f, 1 + Random.value * 0.1f - 0.05f);
    }
    public void PlayBossStep()
    {
        AudioClip clip = _bossStep[Random.Range(0, _bossStep.Length)];
        PlayAudioClip(clip, 0.5f + Random.value * 0.5f, 1 + Random.value * 0.1f - 0.05f);
    }
    public void PlayShootAR()
    {
        PlayAudioClip(_shootAR, 1f, 1 + Random.value * 0.1f - 0.05f);
    }
    public void PlayShootShotgun()
    {
        PlayAudioClip(_shootShotgun, 1f, 1 + Random.value * 0.1f - 0.05f);
    }
    public void PlayShootNade()
    {
        PlayAudioClip(_shootGrenade, 1f, 1 + Random.value * 0.1f - 0.05f);
    }
    public void PlayEventStart()
    {
        PlayAudioClip(_eventStart, 0.5f + Random.value * 0.5f, 1 + Random.value * 0.1f - 0.05f);
    }
    public void PlayUnlockGun()
    {
        PlayAudioClip(_gunUnlock);
    }
    public void PlayReload()
    {
        PlayAudioClip(_reload);
    }
    public void PlaySwitch()
    {
        PlayAudioClip(_switch);
    }
    public void PlayDie()
    {
        PlayAudioClip(_die);
    }
    public void PlayPickup()
    {
        PlayAudioClip(_pickup);
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

    public void PlayAudioClip(AudioClip clip, float volume = 1f, float pitch = 1f)
    {
        AudioSource availableSource = GetAvailableAudioSource();
        if (availableSource != null)
        {
            availableSource.transform.localPosition = Vector3.zero;
            availableSource.clip = clip;
            availableSource.volume = volume;
            availableSource.pitch = pitch;
            availableSource.gameObject.SetActive(true);
            availableSource.Play();
            StartCoroutine(DisableAfterPlay(availableSource, clip.length));
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

    private IEnumerator DisableAfterPlay(AudioSource audioSource, float audioLength)
    {
        if (!_waitDict.ContainsKey(audioLength))
        {
            _waitDict.Add(audioLength, new WaitForSeconds(audioLength));
        }
        yield return _waitDict[audioLength];
        audioSource.gameObject.SetActive(false);
    }
}
