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

    public void EquipGun(Gun gun)
    {
        OnEquipGun?.Invoke(gun);
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
    }
}
