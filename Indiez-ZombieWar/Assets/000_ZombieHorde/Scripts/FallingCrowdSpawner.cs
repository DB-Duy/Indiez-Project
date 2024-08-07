using DG.Tweening;

using GPUInstancer.CrowdAnimations;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;

public class FallingCrowdSpawner : ZombieSpawner
{
    public Vector3 FallDirection;
    public float FallDuration;
    protected override void SpawnObject()
    {
        GameObject prototype = null;
        Vector3 randomPosition = GetRandomPointInBounds(_spawnArea.bounds);
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
        var instance = ZombieInstancingManager.Instance.GetAvailableZombie(prototype);
        instance.Spawner = this;
        instance.transform.SetPositionAndRotation(randomPosition, Quaternion.Euler(Random.insideUnitSphere * 360f));
        instance.gameObject.SetActive(true);
        instance.RagdollParent = ZombieInstancingManager.Instance.GetRagdollParent(prototype.name);
        float speed = Random.Range(ZombieMoveSpeed.x * 100, ZombieMoveSpeed.y * 100) * 0.01f;
        //var clip = _runningClips[Random.Range(0, _runningClips.Length)];
        var animator = prototype.GetComponent<Animator>().runtimeAnimatorController;
        var clip = animator.animationClips[Random.Range(0, animator.animationClips.Length)];
        float animSpeed = Random.Range(ZombieAnimSpeed.x * 100, ZombieAnimSpeed.y * 100) * 0.01f;
        instance.PlayAnimation(clip, Random.value, animSpeed);
        instance.transform.DOMove(instance.transform.position + FallDirection, FallDuration).OnComplete(() => instance.gameObject.SetActive(false));
    }
}
