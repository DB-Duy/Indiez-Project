using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaycastKillZombie : MonoBehaviour
{
    public bool IsShooting = false;
    public float Range = 20f;
    public float FireInterval = 0.5f;
    private float _lastShot = -1;
    public LayerMask _zombieMask;
    public Vector3 ragdollForceDirection;
    public float ragdollForcePower;

    private void Update()
    {
        if (!IsShooting) { return; }
        if (Time.time - _lastShot > FireInterval)
        {
            PerformRaycast();
            _lastShot = Time.time;
        }
    }
    public void PerformRaycast()
    {
        if (Physics.Raycast(new Ray(transform.position, transform.forward), out RaycastHit hitInfo, Range, _zombieMask))
        {
            var zombie = hitInfo.collider.GetComponent<ZombieInstance>();
            if (zombie == null) { return; }

            zombie.ZombieFall((transform.forward + ragdollForceDirection) * ragdollForcePower);
        }
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * Range);
    }
}
