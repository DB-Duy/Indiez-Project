using DG.Tweening;
using GPUInstancer;
using GPUInstancer.CrowdAnimations;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class ZombieSpawner : MonoBehaviour
{
    [SerializeField]
    protected Transform _target;
    public float _targetRadius = 5;
    [SerializeField]
    protected NavMeshSurface _meshSurface;
    public int SpawnRate;
    [SerializeField]
    protected BoxCollider _spawnArea;
    [SerializeField]
    protected GPUICrowdManager _crowdManager;
    public Vector2 ZombieMoveSpeed, ZombieAnimSpeed;
    public GameObject[] SpawnPrefabs;
    public AnimationClip[] CustomClips;
    protected float spawnInterval;
    protected float timeSinceLastSpawn;
    public bool IsSpawning;
    [SerializeField, HideInInspector]
    private bool showGizmo = false;
    public bool SpawnOnStart;

    protected void OnValidate()
    {
        _spawnArea = GetComponent<BoxCollider>();
        _crowdManager = FindObjectOfType<GPUICrowdManager>();
    }
    [ContextMenu("Toggle Gizmo")]
    private void ToggleGizmo()
    {
        showGizmo = !showGizmo;
    }
    private void OnDrawGizmos()
    {
        if (!showGizmo) { return; }
        Gizmos.DrawSphere(_target.transform.position, _targetRadius);
    }
    protected Vector3 GetRandomNavMeshPositionNearLocation(Vector3 origin, float range)
    {
        for (int i = 0; i < 30; i++)
        {
            Vector3 randomPoint = origin + Random.insideUnitSphere * range;
            if (NavMesh.SamplePosition(randomPoint, out NavMeshHit _navMeshHit, range, NavMesh.AllAreas))
                return _navMeshHit.position;
        }
        return origin;
    }

    protected void Start()
    {
        if (SpawnRate <= 0) { return; }
        spawnInterval = 1f / SpawnRate;
        timeSinceLastSpawn = 0f;

        if (SpawnPrefabs.Length == 0)
        {
            SpawnPrefabs = new GameObject[_crowdManager.prototypeList.Count];
            for (int i = 0; i < _crowdManager.prototypeList.Count; i++)
            {
                SpawnPrefabs[i] = _crowdManager.prototypeList[i].prefabObject;
            }
        }
        IsSpawning = SpawnOnStart;
    }
    protected void Update()
    {
        if (!IsSpawning) { return; }
        timeSinceLastSpawn += Time.deltaTime;

        while (timeSinceLastSpawn >= spawnInterval)
        {
            SpawnObject();
            timeSinceLastSpawn -= spawnInterval;
        }
    }
    public void SpawnAmount(int count)
    {
        for (int i = 0; i < count; i++)
        {
            SpawnObject();
        }
    }
    protected virtual void SpawnObject()
    {
        Vector3 randomPosition = GetRandomPointInBounds(_spawnArea.bounds);
        if (NavMesh.SamplePosition(randomPosition, out NavMeshHit hit, 1.0f, NavMesh.AllAreas))
        {
            if (!hit.hit)
            {
                SpawnObject();
                return;
            }
            ZombieInstance instance;

            GameObject prototype = null;
            if (SpawnPrefabs.Length == 0)
            {
                prototype = _crowdManager.prototypeList[Random.Range(0, _crowdManager.prefabList.Count)].prefabObject;
            }
            else
            {
                prototype = SpawnPrefabs[Random.Range(0, SpawnPrefabs.Length)];
            }
            while (Random.value > prototype.GetComponent<ZombieInstance>().SpawnRate)
            {
                prototype = _crowdManager.prototypeList[Random.Range(0, _crowdManager.prefabList.Count)].prefabObject;
            }

            instance = ZombieInstancingManager.Instance.GetAvailableZombie(prototype);
            instance.Spawner = this;
            instance.transform.SetPositionAndRotation(hit.position, Quaternion.identity);
            instance.gameObject.SetActive(true);
            instance.agent.enabled = true;

            instance.RagdollParent = ZombieInstancingManager.Instance.GetRagdollParent(instance.gameObject.name);
            float moveSpeed = Random.Range(ZombieMoveSpeed.x * 100, ZombieMoveSpeed.y * 100) * 0.01f * 2;
            float animSpeed = Random.Range(ZombieAnimSpeed.x * 100, ZombieAnimSpeed.y * 100) * 0.01f;
            var animator = instance.GetComponent<Animator>().runtimeAnimatorController;
            var clip = CustomClips.Length == 0 ? animator.animationClips[Random.Range(0, animator.animationClips.Length)] : CustomClips[Random.Range(0, CustomClips.Length)];
            instance.PlayAnimation(clip, Random.value, animSpeed);
            instance.RunningClip = clip;
            instance.agent.speed = moveSpeed;
            instance.currentAnimSpeed = animSpeed;
            instance.currentAgentSpeed = moveSpeed;
            if (_target != null)
            {
                instance.agent.SetDestination(GetRandomNavMeshPositionNearLocation(_target.position, _targetRadius));
            }
        }
    }


    protected Vector3 GetRandomPointInBounds(Bounds bounds)
    {
        return new Vector3(
            Random.Range(bounds.min.x, bounds.max.x),
            bounds.center.y,
            Random.Range(bounds.min.z, bounds.max.z)
        );
    }
}
