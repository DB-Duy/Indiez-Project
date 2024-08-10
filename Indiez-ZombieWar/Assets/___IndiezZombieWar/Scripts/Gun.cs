using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public enum GunType
{
    Automatic,
    Shotgun,
    GrenadeLauncher
}
public class Gun : MonoBehaviour
{
    public GunType GunType;
    public int ammoAvailable;
    public int magazineSize = 30;
    public int currentMagazineSize;
    public float fireRate;
    [SerializeField]
    public bool IsShooting;
    [SerializeField]
    private ShootingBulletFX _bulletFX;
    private float lastShotTime = -1;
    [SerializeField, HideInInspector]
    private ActionManager _actionManager;
    [SerializeField]
    private Animator _gunAnimator;
    private int _shootId;
    private Transform _currentTarget;
    private void OnValidate()
    {
        _bulletFX = GetComponent<ShootingBulletFX>();
        _actionManager = FindObjectOfType<ActionManager>();
        _gunAnimator = GetComponent<Animator>();
    }
    private void Awake()
    {
        _actionManager.OnShootButtonPressed += TryShoot;
        _actionManager.OnReloadComplete += ReloadComplete;
        _actionManager.OnShootButtonReleased += StopShoot;
        _actionManager.OnTargetAcquired += GetTarget;
    }
    private void GetTarget(Transform target)
    {
        _currentTarget = target;
    }
    private void Start()
    {
        _shootId = Animator.StringToHash("Shoot");
    }
    private void OnDestroy()
    {
        _actionManager.OnShootButtonPressed -= TryShoot;
        _actionManager.OnReloadComplete -= ReloadComplete;
        _actionManager.OnShootButtonReleased -= StopShoot;
        _actionManager.OnTargetAcquired -= GetTarget;
    }
    public void TryShoot()
    {
        if (currentMagazineSize > 0)
        {
            IsShooting = true;
        }
    }
    public void StopShoot()
    {
        IsShooting = false;
    }
    private void Update()
    {
        if (!IsShooting) { return; }
        if (Time.time - lastShotTime > fireRate)
        {
            ShootGun();
            lastShotTime = Time.time;
        }
    }
    private void ShootGun()
    {
        _actionManager.PerformShoot();
        _gunAnimator.SetTrigger(_shootId);
        _bulletFX.PlayShootFX();
        DealDamage();
    }

    private void DealDamage()
    {
        if (_currentTarget == null) { return; }
        _currentTarget.GetComponent<ZombieHP>().TakeDamage(1);
    }

    private void ReloadComplete()
    {

    }
}
