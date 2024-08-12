
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.UI;

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
    public int maxMagazineSize = 30;
    public int currentMagazineSize;
    public float fireRate;
    public float GunRange;
    public int Damage;
    [SerializeField]
    public bool IsShooting;
    [SerializeField]
    protected ShootingBulletFX _bulletFX;
    protected float lastShotTime = -1;
    [SerializeField, HideInInspector]
    protected ActionManager _actionManager;
    [SerializeField]
    protected Animator _gunAnimator;
    protected static int _shootId;
    protected Transform _currentTarget;
    [SerializeField]
    public Transform MuzzlePoint;
    [SerializeField]
    public ParticleSystem MuzzleFlash;
    [SerializeField]
    public Sprite gunSprite;
    public bool IsReloading = false;

    private void OnValidate()
    {
        _bulletFX = FindObjectOfType<ShootingBulletFX>();
        _actionManager = FindObjectOfType<ActionManager>();
        _gunAnimator = GetComponent<Animator>();
    }
    private void OnEnable()
    {
        _actionManager.OnShootButtonPressed += StartShoot;
        _actionManager.OnShootButtonReleased += StopShoot;
        _actionManager.OnTargetAcquired += GetTarget;

        _bulletFX.MuzzlePos = MuzzlePoint;
        _bulletFX.MuzzleFlash = MuzzleFlash;
    }
    private void GetTarget(Transform target)
    {
        _currentTarget = target;
    }
    protected virtual void Start()
    {
        _shootId = Animator.StringToHash("Shoot");

        currentMagazineSize = maxMagazineSize;
    }
    private void OnDisable()
    {
        _actionManager.OnShootButtonPressed -= StartShoot;
        _actionManager.OnShootButtonReleased -= StopShoot;
        _actionManager.OnTargetAcquired -= GetTarget;
    }
    public void TryShoot()
    {
        if (IsReloading) { return; }
        if (currentMagazineSize > 0)
        {
            ShootGun();
        }
        else
        {
            TryReload();
        }
    }
    public void TryReload()
    {
        if (ammoAvailable > 0 && currentMagazineSize < maxMagazineSize)
        {
            IsReloading = true;
            _actionManager.PerformReload();
        }
    }
    public void StartShoot()
    {
        IsShooting = true;
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
            TryShoot();
        }
    }
    protected virtual void ShootGun()
    {
        currentMagazineSize--;
        _gunAnimator.SetTrigger(_shootId);
        PerformRaycastAR();
        _actionManager.PerformShoot();
        lastShotTime = Time.time;
    }

    public LayerMask HitLayer;
    private void PerformRaycastAR()
    {
        Vector3 direction = _currentTarget == null ? MuzzlePoint.forward : ((_currentTarget.transform.position + new Vector3(0, 1.5f, 0)) - MuzzlePoint.position).normalized;
        if (Physics.Raycast(MuzzlePoint.position, direction, out RaycastHit hitInfo, GunRange, HitLayer, QueryTriggerInteraction.Collide))
        {
            if (hitInfo.collider.CompareTag("Zombie"))
            {
                _currentTarget.GetComponent<ZombieHP>().TakeDamage(Damage);
                _bulletFX.PlayShootFX(hitInfo.point, false);
                _bulletFX.PlayBloodFxCurrentTarget();
            }
            else
            {
                _bulletFX.PlayShootFX(hitInfo.point, true);
            }
        }
        else
        {
            _bulletFX.PlayShootFX(MuzzlePoint.position + MuzzlePoint.forward * GunRange, false);
        }
    }

    public void ReloadComplete()
    {
        IsReloading = false;
        int bulletsNeeded = maxMagazineSize - currentMagazineSize;

        int bulletsToLoad = Mathf.Min(bulletsNeeded, ammoAvailable);

        currentMagazineSize += bulletsToLoad;

        ammoAvailable -= bulletsToLoad;
        _actionManager.ReloadComplete();
    }
}
