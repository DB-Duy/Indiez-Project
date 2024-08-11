using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class ZomHitFx : MonoBehaviour
{
    [SerializeField]
    private ParticleSystem _particleSystem;
    public IObjectPool<ZomHitFx> hitFxPool;

    private void OnValidate()
    {
        _particleSystem = GetComponent<ParticleSystem>();
    }
    private void OnDisable()
    {
        hitFxPool.Release(this);
    }
    public void Play()
    {
        _particleSystem.Play();
    }
}
