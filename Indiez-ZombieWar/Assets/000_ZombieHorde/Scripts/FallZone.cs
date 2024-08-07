using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


public enum ZoneType
{
    FallZone = 0,
    BurningZone = 1,
    ExplosionZone = 2,
    ShootZone = 3,
    DestroyZone = 4
}
public class FallZone : MonoBehaviour
{
    public ZoneType ZoneType;
    public float DeadPercentage = 1;

    public float ExplodeForce = 200f;
    public float ExplodeUpForce = 40f;
    private void OnTriggerEnter(Collider other)
    {
        var instance = other.GetComponent<ZombieInstance>();
        ZombieRagdoll ragdoll = null;
        if (instance == null) { return; }
        switch (ZoneType)
        {
            case ZoneType.FallZone:
                if (Random.value < DeadPercentage)
                {
                    instance.ZombieFall();
                }
                break;
            case ZoneType.ExplosionZone:
                instance.ZombieFall();
                ragdoll = instance.RagdollInstance;
                if (ragdoll != null && !ragdoll.CanBake())
                {
                    ragdoll.Explode(transform.position, GetComponent<SphereCollider>().radius, ExplodeForce, ExplodeUpForce);
                }
                break;
            case ZoneType.DestroyZone:
                instance.ZombieDead();
                break;
        }
    }
}
