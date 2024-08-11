using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrenadeLauncher : Gun
{
    private ColliderHit[] colliderHits = new ColliderHit[10];
    protected override void Start()
    {
        base.Start();
    }
    protected override void ShootGun()
    {
        currentMagazineSize--;
        _gunAnimator.SetTrigger(_shootId);
        _actionManager.PerformShoot();
        _bulletFX.PlayShootFXGrenadeLauncher();
        lastShotTime = Time.time;
    }
}
