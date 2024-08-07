using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieRagdollCollider : MonoBehaviour
{
    [SerializeField, HideInInspector]
    public ZombieRagdoll Ragdoll;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            Ragdoll.OnHitGround();
        }
    }
    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            Ragdoll.OnLeaveGround();
        }
    }
}
