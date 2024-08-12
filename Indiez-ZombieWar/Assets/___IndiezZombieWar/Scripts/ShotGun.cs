using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShotGun : Gun
{
    private bool showRayGizmo = false;
    [SerializeField]
    private Transform[] shootDirectionTransforms;
    [SerializeField]
    private Vector3[] shootTargets;
    private Vector3[] shootDirections;
    private bool[] playDecals;
    protected override void Start()
    {
        base.Start();
        shootTargets = new Vector3[shootDirectionTransforms.Length];
        shootDirections = new Vector3[shootDirectionTransforms.Length];
        playDecals = new bool[shootDirections.Length];
    }
    private void GetShootDirections()
    {
        for (int i = 0; i < shootDirections.Length; i++)
        {
            shootDirections[i] = (shootDirectionTransforms[i].position - MuzzlePoint.position).normalized;
        }
    }

    [ContextMenu("ToggleGizmo")]
    private void ToggleGizmo()
    {
        showRayGizmo = !showRayGizmo;
    }
    private void OnDrawGizmos()
    {
        if (!showRayGizmo) { return; }
        for (int i = 0; i < shootDirectionTransforms.Length; i++)
        {
            Gizmos.DrawLine(MuzzlePoint.position, shootDirectionTransforms[i].position);
        }
    }

    protected override void ShootGun()
    {
        currentMagazineSize--;
        PerformRaycastShotgun();
        _gunAnimator.SetTrigger(_shootId);
        _bulletFX.PlayShootFXShotgun(shootTargets, playDecals);
        _actionManager.PerformShoot();
        lastShotTime = Time.time;
    }

    private void PerformRaycastShotgun()
    {
        GetShootDirections();
        for (int i = 0; i < shootDirections.Length; i++)
        {
            if (Physics.Raycast(MuzzlePoint.position, shootDirections[i], out RaycastHit hitInfo, GunRange, HitLayer, QueryTriggerInteraction.Collide))
            {
                shootTargets[i] = hitInfo.point;
                if (hitInfo.transform.CompareTag("Zombie"))
                {
                    playDecals[i] = false;
                    DealDamageShotgun(hitInfo.transform, hitInfo.point);
                }
                else
                {
                    playDecals[i] = true;
                }
            }
            else
            {
                playDecals[i] = false;
                shootTargets[i] = MuzzlePoint.position + shootDirections[i] * GunRange;
            }
        }
    }

    protected void DealDamageShotgun(Transform target, Vector3 point)
    {
        ZombieHP zomb = target.GetComponent<ZombieHP>();
        if (zomb != null)
        {
            target.GetComponent<ZombieHP>().TakeDamage(Damage);
            _bulletFX.PlayBloodFxAtPoint(point);
        }
    }
}
