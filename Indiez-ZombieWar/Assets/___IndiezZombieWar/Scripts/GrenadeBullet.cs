using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class GrenadeBullet : MonoBehaviour
{
    [HideInInspector]
    public ShootingBulletFX BulletFx;
    public IObjectPool<GrenadeBullet> grenadeBulletPool;
    private Collider[] colliderHits = new Collider[20];
    public int Damage = 2;
    public float Radius = 1.5f;
    public LayerMask _zombMask;
    private void OnTriggerEnter(Collider other)
    {
        ExplodeBullet();
    }
    public void ExplodeBullet()
    {
        BulletFx.PlayExplosionAtPoint(transform.position);
        DamageZombies();
        AudioPool.Instance.PlayNadeExplode();
        grenadeBulletPool.Release(this);
    }

    private void DamageZombies()
    {
        int targetsInZone = Physics.OverlapSphereNonAlloc(transform.position, Radius, colliderHits, _zombMask, QueryTriggerInteraction.Collide);
        if (targetsInZone > 0)
        {
            for (int i = 0; i < targetsInZone; i++)
            {
                ZombieHP zombHP = colliderHits[i].GetComponent<ZombieHP>();
                zombHP.TakeDamage(Damage, transform.position);
            }
        }
    }
}
