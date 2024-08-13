using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionManager : MonoBehaviour
{
    public event Action OnShootButtonPressed;
    public event Action OnShootButtonReleased;
    public event Action OnPerformShoot;
    public event Action OnReloadButtonPressed;
    public event Action OnPerformReload;
    public event Action<Transform> OnTargetAcquired;
    public event Action OnRequestAcquireTarget;
    public event Action OnTargetLost;
    public event Action OnReloadComplete;
    public event Action<Gun> OnEquipGun;
    public event Action<Transform, int> OnPlayerReceiveDamage;
    public event Action OnPlayerDead;
    public event Action<ZombieHP> OnZombieKilled;
    public event Action<Pickup> OnPlayerPickUpAmmo;
    public event Action<Pickup> OnPlayerPickUpHealth;

    private void Awake()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;
    }
    public void ZombieKilled(ZombieHP zomb)
    {
        OnZombieKilled?.Invoke(zomb);
    }
    public void PlayerPickUpAmmo(Pickup pickup)
    {
        OnPlayerPickUpAmmo?.Invoke(pickup);
        AudioPool.Instance.PlayPickup();
    }
    public void PlayerPickUpHealth(Pickup pickup)
    {
        OnPlayerPickUpHealth?.Invoke(pickup);
        AudioPool.Instance.PlayPickup();
    }
    public void EquipGun(Gun gun)
    {
        OnEquipGun?.Invoke(gun);
        AudioPool.Instance.PlaySwitch();
    }
    public void ReloadComplete()
    {
        OnReloadComplete?.Invoke();
    }
    public void ShootButtonPressed()
    {
        OnShootButtonPressed?.Invoke();
    }
    public void ShootButtonReleased()
    {
        OnShootButtonReleased?.Invoke();
    }
    public void PerformReload()
    {
        OnPerformReload?.Invoke();
        AudioPool.Instance.PlayReload();
    }
    public void AcquireTarget(Transform target)
    {
        OnTargetAcquired?.Invoke(target);
    }
    public void PerformShoot()
    {
        OnPerformShoot?.Invoke();
    }
    public void PlayerReceiveDamage(Transform source, int damage)
    {
        OnPlayerReceiveDamage?.Invoke(source, damage);
    }
    public void PlayerDead()
    {
        OnPlayerDead?.Invoke();
        AudioPool.Instance.PlayDie();
    }
}
