using DG.Tweening;
using GPUInstancer.CrowdAnimations;
using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.AI;

public class ZombieInstance : MonoBehaviour
{
    [HideInInspector]
    public Transform RagdollParent;
    [SerializeField, HideInInspector]
    public NavMeshAgent agent;
    //public GPUICrowdPrefab crowdPrefab;
    [SerializeField, HideInInspector]
    public GPUICrowdPrefab crowdPrefab;
    [SerializeField, HideInInspector]
    private Camera _mainCam;
    [SerializeField]
    public ZombieRagdoll RagdollPrefab;
    public float SpawnRate = 1f;
    [HideInInspector]
    public ZombieRagdoll RagdollInstance = null;
    [SerializeField, HideInInspector]
    public SkinnedMeshRenderer _renderer;
    private Material _mat;
    [HideInInspector]
    public AnimationClip PlayingClip;
    [HideInInspector]
    public ZombieSpawner Spawner;
    [HideInInspector]
    public string prototypeName;
    private void OnValidate()
    {
        agent = GetComponent<NavMeshAgent>();
        crowdPrefab = GetComponent<GPUICrowdPrefab>();
        _renderer = GetComponentInChildren<SkinnedMeshRenderer>();
    }
    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        crowdPrefab = GetComponent<GPUICrowdPrefab>();
        _mat = _renderer.material;
    }
    private float startTime = -1;
    public void PlayAnimation(AnimationClip anim, float startTime = 0f, float speed = 1f)
    {
        PlayingClip = anim;
        crowdPrefab.StartAnimation(anim, startTime: startTime, speed: speed * 0.7f);
    }

    public void ZombieFall(Vector3 ragdollForce = new Vector3())
    {
        var ragdoll = ZombieInstancingManager.Instance.GetAvailableRagdoll(prototypeName);
        ragdoll.transform.SetPositionAndRotation(transform.position, transform.rotation);
        ragdoll.transform.parent = RagdollParent;
        ragdoll.gameObject.SetActive(true);
        if (ragdollForce != Vector3.zero)
        {
            ragdoll.ApplyForce(ragdollForce);
        }
        RagdollInstance = ragdoll;
        PlayingClip.SampleAnimation(ragdoll.gameObject, crowdPrefab.GetAnimationTime(PlayingClip));
        ragdoll.transform.SetPositionAndRotation(transform.position, transform.rotation);
        ZombieDead();
    }
    public void ZombieDead()
    {
        gameObject.SetActive(false);
    }
}
