using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Pool;

public class ShootingBulletFX : MonoBehaviour
{
    public Transform currentTarget;
    [SerializeField, HideInInspector]
    private ActionManager _actionManager;
    [SerializeField]
    private LineRenderer _lineRendererPrefab;
    [SerializeField]
    private ZomHitFx _zombieBloodFxPrefab, _grenadeExplosionPrefab;
    [SerializeField]
    private GrenadeBullet _grenadeBulletPrefab;
    private IObjectPool<LineRenderer> _lineRendererPool;
    private IObjectPool<ZomHitFx> _zombBloodFxPool;
    private IObjectPool<GrenadeBullet> _grenadeBulletPool;
    private IObjectPool<ZomHitFx> _grenadeExplosionPool;
    [SerializeField]
    public Transform MuzzlePos, _bulletFXParent;
    private RaycastHit[] _raycastHits = new RaycastHit[5];
    private Vector3[] _hitFxPosition = new Vector3[2];
    [SerializeField]
    public ParticleSystem MuzzleFlash;
    [HideInInspector]
    public float GunRange;
    private void OnValidate()
    {
        _actionManager = FindObjectOfType<ActionManager>();
    }
    private void Start()
    {
        _lineRendererPool = new ObjectPool<LineRenderer>(OnCreateNewLineRenderer, GetLineRendererFromPool, OnReleaseLineRenderer, OnDestroyLineRenderer, false, 15, 100);
        _zombBloodFxPool = new ObjectPool<ZomHitFx>(OnCreateNewBloodFx, GetZomHitFxFromPool, OnReleaseZomHitFx, OnDestroyZomHitFx, false, 15, 100);
        _grenadeBulletPool = new ObjectPool<GrenadeBullet>(OnCreateGrenadeBullet, GetGrenadeBulletFromPool, OnReleaseGrenadebulletFromPool, OnDestroyGrenadeBulletFromPool, false, 15, 100);
        _grenadeExplosionPool = new ObjectPool<ZomHitFx>(OnCreateExplosion, GetZomHitFxFromPool, OnReleaseZomHitFx, OnDestroyZomHitFx, false, 15, 100);
    }
    private GrenadeBullet OnCreateGrenadeBullet()
    {
        GrenadeBullet bullet = Instantiate(_grenadeBulletPrefab, _bulletFXParent);
        bullet.BulletFx = this;
        bullet.grenadeBulletPool = _grenadeBulletPool;
        bullet.gameObject.SetActive(false);
        return bullet;
    }
    private ZomHitFx OnCreateExplosion()
    {
        ZomHitFx fx = Instantiate(_grenadeExplosionPrefab, _bulletFXParent);
        fx.hitFxPool = _grenadeExplosionPool;
        fx.gameObject.SetActive(false);
        return fx;
    }
    private ZomHitFx OnCreateNewBloodFx()
    {
        ZomHitFx fx = Instantiate(_zombieBloodFxPrefab, _bulletFXParent);
        fx.hitFxPool = _zombBloodFxPool;
        fx.gameObject.SetActive(false);
        return fx;
    }
    private LineRenderer OnCreateNewLineRenderer()
    {
        LineRenderer fx = Instantiate(_lineRendererPrefab, _bulletFXParent);
        fx.useWorldSpace = true;
        fx.gameObject.SetActive(false);
        return fx;
    }
    private void OnDestroyGrenadeBulletFromPool(GrenadeBullet obj)
    {
        Destroy(obj.gameObject);
    }
    private void GetLineRendererFromPool(LineRenderer fromPool)
    {
        fromPool.material.SetFloat("_Alpha", 1);
    }
    private void GetZomHitFxFromPool(ZomHitFx bloodFx)
    {
    }
    private void GetGrenadeBulletFromPool(GrenadeBullet obj)
    {

    }
    private void OnReleaseGrenadebulletFromPool(GrenadeBullet obj)
    {
        obj.gameObject.SetActive(false);
    }
    private void OnReleaseLineRenderer(LineRenderer fromPool)
    {
        fromPool.gameObject.SetActive(false);
        fromPool.material.SetFloat("_Alpha", 0);
    }
    private void OnReleaseZomHitFx(ZomHitFx bloodFx)
    {

    }
    private void OnDestroyZomHitFx(ZomHitFx bloodFx)
    {
        Destroy(bloodFx.gameObject);
    }
    private void OnDestroyLineRenderer(LineRenderer fromPool)
    {
        Destroy(fromPool.gameObject);
    }
    private void Awake()
    {
        _actionManager.OnTargetAcquired += GetCurrentTarget;
        _actionManager.OnEquipGun += GetGunInfo;
    }
    private void OnDestroy()
    {
        _actionManager.OnTargetAcquired -= GetCurrentTarget;
        _actionManager.OnEquipGun -= GetGunInfo;
    }
    private void GetCurrentTarget(Transform target)
    {
        currentTarget = target;
    }
    private void GetGunInfo(Gun gun)
    {
        MuzzleFlash = gun.MuzzleFlash;
        MuzzlePos = gun.MuzzlePoint;
        GunRange = gun.GunRange;
    }
    public void PlayShootFX()
    {
        MuzzleFlash.Play();
        LineRenderer shootFx = _lineRendererPool.Get();
        _hitFxPosition[0] = MuzzlePos.position;
        _hitFxPosition[1] = currentTarget == null ? (MuzzlePos.position + MuzzlePos.forward * GunRange) : (currentTarget.transform.position + new Vector3(0, 1.5f, 0));
        _hitFxPosition[1] += UnityEngine.Random.insideUnitSphere * 0.1f;
        shootFx.gameObject.SetActive(true);
        shootFx.SetPositions(_hitFxPosition);
        DOTween.To(() => shootFx.material.GetFloat("_Alpha"), (x) => shootFx.material.SetFloat("_Alpha", x), 0, 0.2f).OnComplete(() => _lineRendererPool.Release(shootFx));
    }
    public void PlayShootFXShotgun(Vector3[] targets)
    {
        MuzzleFlash.Play();
        for (int i = 0; i < targets.Length; i++)
        {
            LineRenderer shootFx = _lineRendererPool.Get();
            _hitFxPosition[0] = MuzzlePos.position;
            _hitFxPosition[1] = targets[i];
            _hitFxPosition[1] += UnityEngine.Random.insideUnitSphere * 0.05f;
            shootFx.gameObject.SetActive(true);
            shootFx.SetPositions(_hitFxPosition);
            DOTween.To(() => shootFx.material.GetFloat("_Alpha"), (x) => shootFx.material.SetFloat("_Alpha", x), 0, 0.2f).OnComplete(() => _lineRendererPool.Release(shootFx));
        }
    }
    private Vector3 grenadeDefaultOffset = new Vector3(0, -1.5f, 6);
    private Vector3 TargetOffset = new Vector3(0, 1.5f, 0);
    public void PlayShootFXGrenadeLauncher()
    {
        Vector3 target = currentTarget == null ? (MuzzlePos.position + MuzzlePos.TransformDirection(grenadeDefaultOffset)) : currentTarget.position + TargetOffset;
        MuzzleFlash.Play();
        GrenadeBullet grenadeBullet = _grenadeBulletPool.Get();
        grenadeBullet.gameObject.SetActive(true);
        grenadeBullet.transform.SetPositionAndRotation(MuzzlePos.position, MuzzlePos.rotation);

        grenadeBullet.transform.DOJump(target, 1f, 1, 0.1f).OnComplete(() =>
        {
            if (grenadeBullet.gameObject.activeInHierarchy)
            {
                grenadeBullet.ExplodeBullet();
            }
        });
    }
    public void PlayExplosionAtPoint(Vector3 position)
    {
        ZomHitFx explosion = _grenadeExplosionPool.Get();
        explosion.transform.position = position;
        explosion.gameObject.SetActive(true);
        explosion.Play();
    }
    public void PlayBloodFxCurrentTarget()
    {
        if (currentTarget == null) return;
        ZomHitFx bloodFx = _zombBloodFxPool.Get();
        bloodFx.transform.position = _hitFxPosition[1];
        bloodFx.transform.forward = (_hitFxPosition[1] - _hitFxPosition[0]).normalized;
        bloodFx.gameObject.SetActive(true);
        bloodFx.Play();
    }
    public void PlayBloodFxAtPoint(Vector3 point)
    {
        ZomHitFx bloodFx = _zombBloodFxPool.Get();
        bloodFx.transform.position = point;
        bloodFx.transform.forward = (point - _hitFxPosition[0]).normalized;
        bloodFx.gameObject.SetActive(true);
        bloodFx.Play();
    }
}