using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UIElements;

public class StageManager : MonoBehaviour
{
    [SerializeField]
    public ZombieSpawner[] _spawnAreas;
    [SerializeField]
    private WeaponCache[] _weaponCaches;
    private int _cachesFound = 0;
    public bool IsEventOccuring = false;
    [SerializeField]
    private Transform _poolParent;
    [SerializeField]
    private Pickup _hpPickupPrefab, _bulletPickupPrefab;
    [SerializeField, HideInInspector]
    private ActionManager _actionManager;
    public IObjectPool<Pickup> _hpPickupPool;
    public IObjectPool<Pickup> _bulletPickupPool;
    public int hpPickupCount = 0;
    public int ammoPickupCount = 0;
    [SerializeField, HideInInspector]
    private PlayerHPManager _playerHPManager;
    [SerializeField, HideInInspector]
    private PlayerInventory _playerInventory;
    public float hpThreshold, minAmmoThreshold, maxAmmoThreshold;

    private void OnValidate()
    {
        _weaponCaches = FindObjectsOfType<WeaponCache>();
        _actionManager = FindObjectOfType<ActionManager>();
        _playerHPManager = FindObjectOfType<PlayerHPManager>();
        _playerInventory = FindObjectOfType<PlayerInventory>();
    }
    private void Awake()
    {
        _actionManager.OnPlayerPickUpAmmo += ReleasePickUpAmmo;
        _actionManager.OnPlayerPickUpHealth += ReleasePickupHealth;
        _actionManager.OnZombieKilled += TrySpawnPickupOnZombieKilled;
    }
    private void OnDestroy()
    {
        _actionManager.OnPlayerPickUpAmmo -= ReleasePickUpAmmo;
        _actionManager.OnPlayerPickUpHealth -= ReleasePickupHealth;
        _actionManager.OnZombieKilled -= TrySpawnPickupOnZombieKilled;
    }

    private void Start()
    {
        _spawnAreas[0].SpawnAmount(5);
        _hpPickupPool = new ObjectPool<Pickup>(OnCreateHPPickup, GetPickup, ReleasePickup, defaultCapacity: 10, maxSize: 20);
        _bulletPickupPool = new ObjectPool<Pickup>(OnCreateBulletPickup, GetPickup, ReleasePickup, defaultCapacity: 10, maxSize: 20);
    }
    public void TrySpawnPickupOnZombieKilled(ZombieHP zombHp)
    {
        if (_playerHPManager.HP < hpThreshold)
        {
            float chance = (_playerHPManager.maxHP - _playerHPManager.HP) / (float)_playerHPManager.maxHP;
            if (UnityEngine.Random.value < chance)
            {
                SpawnHPPickup(zombHp);
            }
        }
        int playerAmmo = _playerInventory.GetTotalAmmo();
        if (playerAmmo < minAmmoThreshold)
        {
            float chance = (maxAmmoThreshold - playerAmmo) / maxAmmoThreshold;
            if (UnityEngine.Random.value < chance)
            {
                SpawnAmmoPickup(zombHp);
            }
        }
    }

    private void SpawnAmmoPickup(ZombieHP zombHp)
    {
        Vector2 randompos = UnityEngine.Random.insideUnitCircle * 2;
        Vector3 zombPos = zombHp.transform.position;
        Vector3 target = new Vector3(zombPos.x + randompos.x, zombPos.y + 0.5f, zombPos.z + randompos.y);

        Pickup pickup = _bulletPickupPool.Get();
        pickup.transform.position = zombPos + new Vector3(0, 1, 0);
        pickup.gameObject.SetActive(true);
        pickup.particles.Play();
        pickup.transform.DOJump(target, 2f, 1, 0.8f)
            .OnComplete(() =>
            {
                pickup.PickupLanded();
            });
    }

    private void SpawnHPPickup(ZombieHP zombHp)
    {
        Vector2 randompos = UnityEngine.Random.insideUnitCircle * 2;
        Vector3 zombPos = zombHp.transform.position;
        Vector3 target = new Vector3(zombPos.x + randompos.x, zombPos.y + 0.5f, zombPos.z + randompos.y);

        Pickup pickup = _hpPickupPool.Get();
        pickup.transform.position = zombPos + new Vector3(0, 1, 0);
        pickup.gameObject.SetActive(true);
        pickup.particles.Play();
        pickup.transform.DOJump(target, 2f, 1, 0.8f)
            .OnComplete(() =>
            {
                pickup.PickupLanded();
            });
    }


    private Pickup OnCreateHPPickup()
    {
        Pickup pickup = Instantiate(_hpPickupPrefab, _poolParent);
        pickup.pickupPool = _hpPickupPool;
        pickup.gameObject.SetActive(false);
        return pickup;
    }
    private Pickup OnCreateBulletPickup()
    {
        Pickup pickup = Instantiate(_bulletPickupPrefab, _poolParent);
        pickup.pickupPool = _bulletPickupPool;
        pickup.gameObject.SetActive(false);
        return pickup;
    }
    private void GetPickup(Pickup pickup)
    {
        pickup.Collider.enabled = false;
        hpPickupCount++;
    }
    private void ReleasePickup(Pickup pickup)
    {
        pickup.gameObject.SetActive(false);
        pickup.CancelInvoke(nameof(pickup.SelfReleaseToPool));
        if (pickup.pickupType == PickupType.Bullet)
        {
            ammoPickupCount--;
        }
        else
        {
            hpPickupCount--;
        }
    }
    public bool CanActivateEvent()
    {
        return !IsEventOccuring;
    }
    public void ActivateEvent(WeaponCache weaponCache)
    {
        IsEventOccuring = true;

    }
    private void ReleasePickUpAmmo(Pickup pickup)
    {
        _bulletPickupPool.Release(pickup);
    }
    private void ReleasePickupHealth(Pickup pickup)
    {
        _hpPickupPool.Release(pickup);
    }
}
