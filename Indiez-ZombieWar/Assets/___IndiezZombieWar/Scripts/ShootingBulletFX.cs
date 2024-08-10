using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
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
    private ParticleSystem _zombieBloodFxPrefab;
    private IObjectPool<LineRenderer> _lineRendererPool;
    private IObjectPool<ParticleSystem> _zombBloodFxPool;
    [SerializeField]
    private Transform _muzzlePos, _bulletFXParent;
    private RaycastHit[] _raycastHits = new RaycastHit[5];
    private Vector3[] _hitFxPosition = new Vector3[2];
    [SerializeField]
    private ParticleSystem _muzzleFlash;
    private void OnValidate()
    {
        _actionManager = GetComponentInParent<ActionManager>();
    }
    private void Start()
    {
        _lineRendererPool = new ObjectPool<LineRenderer>(OnCreateNewLineRenderer, GetLineRendererFromPool, OnReleaseLineRenderer, OnDestroyLineRenderer, false, 15, 100);
        _zombBloodFxPool = new ObjectPool<ParticleSystem>(OnCreateNewBloodFx, GetBloodFXFromPool, OnReleaseBloodFX, OnDestroyBloodFX, false, 15, 100);
    }
    private ParticleSystem OnCreateNewBloodFx()
    {
        ParticleSystem fx = Instantiate(_zombieBloodFxPrefab, _bulletFXParent);
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
    private void GetLineRendererFromPool(LineRenderer fromPool)
    {
        fromPool.material.SetFloat("_Alpha", 1);
    }
    private void GetBloodFXFromPool(ParticleSystem bloodFx)
    {

    }

    private void OnReleaseLineRenderer(LineRenderer fromPool)
    {
        fromPool.gameObject.SetActive(false);
        fromPool.material.SetFloat("_Alpha", 0);
    }
    private void OnReleaseBloodFX(ParticleSystem bloodFx)
    {
        bloodFx.gameObject.SetActive(false);
    }
    private void OnDestroyBloodFX(ParticleSystem bloodFx)
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
    }
    private void GetCurrentTarget(Transform target)
    {
        currentTarget = target;
    }
    private void OnDestroy()
    {
        _actionManager.OnTargetAcquired -= GetCurrentTarget;
    }
    public void PlayShootFX()
    {
        _muzzleFlash.Play();
        LineRenderer shootFx = _lineRendererPool.Get();
        _hitFxPosition[0] = _muzzlePos.position;
        _hitFxPosition[1] = currentTarget == null ? (_muzzlePos.position + _muzzlePos.forward * 10f) : (currentTarget.transform.position + new Vector3(0, 1.5f, 0));
        _hitFxPosition[1] += UnityEngine.Random.insideUnitSphere * 0.1f;
        PlayBloodFx();
        shootFx.gameObject.SetActive(true);
        shootFx.SetPositions(_hitFxPosition);
        DOTween.To(() => shootFx.material.GetFloat("_Alpha"), (x) => shootFx.material.SetFloat("_Alpha", x), 0, 0.2f).OnComplete(() => _lineRendererPool.Release(shootFx));
    }

    private void PlayBloodFx()
    {
        if (currentTarget == null) return;
        ParticleSystem bloodFx = _zombBloodFxPool.Get();
        bloodFx.transform.position = _hitFxPosition[1];
        bloodFx.transform.forward = (_hitFxPosition[1] - _hitFxPosition[0]).normalized;
        bloodFx.gameObject.SetActive(true);
        bloodFx.Play();
    }
}