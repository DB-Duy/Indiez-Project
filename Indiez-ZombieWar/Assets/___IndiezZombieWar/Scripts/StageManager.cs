using DG.Tweening;
using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UIElements;

public class StageManager : MonoBehaviour
{
    [SerializeField]
    private Transform _poolParent;
    [Header("Spawn Settings")]
    [SerializeField]
    public ZombieSpawner[] _spawnAreas;
    [SerializeField]
    private WeaponCache[] _weaponCaches;
    private int _cachesFound = 0;
    public float BaseZombCountPerSpawn = 1;
    private WaitForSeconds SpawnOffsetWait = new WaitForSeconds(1.5f);
    private WaitForSeconds Wait1Second = new WaitForSeconds(1);
    private Coroutine _spawnZombiesCoroutine;
    public int MaxZombieCountPerSpawner = 30;

    [Header("Pickup Settings")]
    [SerializeField]
    private Pickup _hpPickupPrefab;
    [SerializeField]
    private Pickup _bulletPickupPrefab;
    [HideInInspector]
    private int hpPickupCount = 0, ammoPickupCount = 0;
    [SerializeField, HideInInspector]
    private ActionManager _actionManager;
    public IObjectPool<Pickup> _hpPickupPool;
    public IObjectPool<Pickup> _bulletPickupPool;
    [SerializeField, HideInInspector]
    private PlayerHPManager _playerHPManager;
    [SerializeField, HideInInspector]
    private PlayerInventory _playerInventory;
    public float hpThreshold, minAmmoThreshold, maxAmmoThreshold;

    public event Action OnActivateWeaponCacheEvent;
    public event Action OnCompleteWeaponCacheEvent;

    private void OnValidate()
    {
        _weaponCaches = FindObjectsOfType<WeaponCache>();
        _actionManager = FindObjectOfType<ActionManager>();
        _playerHPManager = FindObjectOfType<PlayerHPManager>();
        _playerInventory = FindObjectOfType<PlayerInventory>();
        _boss = FindObjectOfType<BossHP>(true);
    }
    private void Awake()
    {
        _actionManager.OnPlayerPickUpAmmo += ReleasePickUpAmmo;
        _actionManager.OnPlayerPickUpHealth += ReleasePickupHealth;
        _actionManager.OnZombieKilled += TrySpawnPickupOnZombieKilled;
        _actionManager.OnPlayerDead += StopAllZombiesActivity;
    }
    private void OnDestroy()
    {
        _actionManager.OnPlayerPickUpAmmo -= ReleasePickUpAmmo;
        _actionManager.OnPlayerPickUpHealth -= ReleasePickupHealth;
        _actionManager.OnZombieKilled -= TrySpawnPickupOnZombieKilled;
        _actionManager.OnPlayerDead -= StopAllZombiesActivity;
    }

    private void Start()
    {
        ZombieSpawner.MaxZombieCount = MaxZombieCountPerSpawner;
        _spawnZombiesCoroutine = StartCoroutine(SpawnZombieCoroutine());
        _hpPickupPool = new ObjectPool<Pickup>(OnCreateHPPickup, GetPickup, ReleasePickup, defaultCapacity: 10, maxSize: 20);
        _bulletPickupPool = new ObjectPool<Pickup>(OnCreateBulletPickup, GetPickup, ReleasePickup, defaultCapacity: 10, maxSize: 20);
    }
    int secondsTillMaxDifficulty = 600;
    int secondsElapsed = 0;
    float maxZomCountPerSpawn = 5;
    float zombCountPerSpawn = 0.2f;
    float zombSpeedIncrement = 1f / 600f;
    bool IsSpawningZombies = true;
    private IEnumerator SpawnZombieCoroutine()
    {
        while (IsSpawningZombies)
        {
            yield return Wait1Second;
            zombCountPerSpawn = BaseZombCountPerSpawn + Mathf.Lerp(0, maxZomCountPerSpawn, (float)secondsElapsed / secondsTillMaxDifficulty);
            _spawnAreas[secondsElapsed % _spawnAreas.Length].SpawnAmount(zombCountPerSpawn);
            secondsElapsed++;
            UpdateSpawnerSettings();
        }
    }
    public void StopAllZombiesActivity()
    {
        IsSpawningZombies = false;
        for (int i = 0; i < _spawnAreas.Length; i++)
        {
            for (int j = 0; j < _spawnAreas[i].ActiveZombiesList.Count; j++)
            {
                var zomb = _spawnAreas[i].ActiveZombiesList[j];
                zomb.agent.isStopped = true;
                zomb.GetComponent<ZombieAttack>().OnDisable();
            }
        }
        _boss.DisableBoss();
    }
    private void UpdateSpawnerSettings()
    {
        for (int i = 0; i < _spawnAreas.Length; i++)
        {
            _spawnAreas[i].ZombieAnimSpeed += Vector2.one * zombSpeedIncrement;
            _spawnAreas[i].ZombieMoveSpeed += Vector2.one * zombSpeedIncrement;
        }
    }
    public void TrySpawnPickupOnZombieKilled(ZombieHP zombHp)
    {
        if (_playerHPManager.HP < hpThreshold && hpPickupCount < 10)
        {
            float chance = (_playerHPManager.maxHP - _playerHPManager.HP) / (float)_playerHPManager.maxHP;
            if (UnityEngine.Random.value < chance)
            {
                SpawnHPPickup(zombHp);
            }
        }
        int playerAmmo = _playerInventory.GetTotalAmmo();
        if (playerAmmo < minAmmoThreshold && ammoPickupCount < 10)
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
        if (pickup.pickupType == PickupType.Bullet)
        {
            ammoPickupCount++;
        }
        else
        {
            hpPickupCount++;
        }
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

    [Header("Event settings")]
    public int EventsCompleted = 0;
    public Gun[] _gunsToUnlock;
    public bool IsEventOccuring = false;
    private Coroutine _eventTimerCoroutine;
    public float EventDuration = 50f;
    private float _eventTimeElapsed = 0;
    private WeaponCache _currentEventWeaponCache;
    [SerializeField]
    private BossHP _boss;
    public bool CanActivateEvent()
    {
        return !IsEventOccuring;
    }
    public void ActivateEvent(WeaponCache weaponCache)
    {
        _currentEventWeaponCache = weaponCache;
        IsEventOccuring = true;
        OnActivateWeaponCacheEvent?.Invoke();
        AudioPool.Instance.PlayEventStart();
        if (EventsCompleted == 2)
        {
            StartBossBattle();
            return;
        }

        UIManager.instance.ShowEvent();
        _currentEventWeaponCache.ActivateWeaponCacheEvent();
        BaseZombCountPerSpawn = EventsCompleted * 0.5f;
        _eventTimeElapsed = 0;
        _eventTimerCoroutine = StartCoroutine(EventTimerCoroutine());
    }
    private void StartBossBattle()
    {
        UIManager.instance.ShowBossFight();
        _boss.gameObject.SetActive(true);
        _boss.transform.DOMoveY(13.5f, 1f).OnComplete(() => _boss.ActivateBoss());
    }

    private IEnumerator EventTimerCoroutine()
    {
        while (_eventTimeElapsed < EventDuration)
        {
            yield return Wait1Second;
            _eventTimeElapsed++;
        }
        EventCompleted();
    }
    public void EventCompleted()
    {
        EventsCompleted++;
        _currentEventWeaponCache.CompleteWeaponCacheEvent();
        IsEventOccuring = false;
        _playerInventory.AvailableGuns.Add(_gunsToUnlock[EventsCompleted - 1]);
        UIManager.instance.ShowGunUnlock();
        AudioPool.Instance.PlayUnlockGun();
        _currentEventWeaponCache = null;
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
