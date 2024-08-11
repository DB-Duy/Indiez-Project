using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public enum PickupType
{
    Bullet = 0,
    Health = 1
}
public class Pickup : MonoBehaviour
{
    public PickupType pickupType = PickupType.Bullet;
    [SerializeField]
    public ParticleSystem particles;
    [SerializeField]
    private TrailRenderer trailRenderer;
    [SerializeField]
    public Collider Collider;
    public float DespawnTime = 40f;
    [HideInInspector]
    public IObjectPool<Pickup> pickupPool;
    private void OnValidate()
    {
        particles = GetComponent<ParticleSystem>();
        trailRenderer = GetComponent<TrailRenderer>();
        Collider = GetComponent<Collider>();
    }
    public void SelfReleaseToPool()
    {
        pickupPool.Release(this);
    }
    public void PickupLanded()
    {
        trailRenderer.emitting = false;
        Invoke(nameof(SelfReleaseToPool), DespawnTime);
        Collider.enabled = true;
    }
}
