using DG.Tweening;
using GPUInstancer.CrowdAnimations;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
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
    public AnimationClip PlayingClip, RunningClip;
    [HideInInspector]
    public ZombieSpawner Spawner;
    [HideInInspector]
    public string prototypeName;
    [HideInInspector]
    public float currentAnimSpeed, currentAgentSpeed;
    [SerializeField, HideInInspector]
    private Collider _collider;
    public void SetCollider(bool active)
    {
        _collider.enabled = active;
    }
    private void OnValidate()
    {
        agent = GetComponent<NavMeshAgent>();
        crowdPrefab = GetComponent<GPUICrowdPrefab>();
        _renderer = GetComponentInChildren<SkinnedMeshRenderer>();
        _collider = GetComponent<Collider>();
    }
    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        crowdPrefab = GetComponent<GPUICrowdPrefab>();
        _mat = _renderer.material;
        animTimes[0] = 0;
        animTimes[1] = 0;
    }
    private Vector4 weights = new Vector4(0, 1, 0, 0);
    private float[] animTimes = new float[2];
    private float[] animSpeed = new float[2];
    public void PlayAnimation(AnimationClip anim, float startTime = 0f, float speed = 1f)
    {
        if (PlayingClip != null)
        {
            animTimes[0] = crowdPrefab.GetAnimationTime(PlayingClip);
            animTimes[1] = startTime;
            animSpeed[0] = currentAnimSpeed;
            animSpeed[1] = speed;
            crowdPrefab.StartBlend(weights, PlayingClip, anim, transitionTime: 0.1f, animationTimes: animTimes, animationSpeeds: animSpeed);
        }
        else
        {
            crowdPrefab.StartAnimation(anim, startTime: startTime, speed: speed * 0.7f);
        }
        PlayingClip = anim;
        currentAnimSpeed = speed;
    }

    public void ZombieFall(Vector3 ragdollForce = new Vector3(), float animTime = -1)
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
        PlayingClip.SampleAnimation(ragdoll.gameObject, animTime >= 0 ? animTime : crowdPrefab.GetAnimationTime(PlayingClip));
        ragdoll.transform.SetPositionAndRotation(transform.position, transform.rotation);
        ZombieDead();
    }

    public void ZombieDead()
    {
        gameObject.SetActive(false);
        Spawner.RemoveActiveZombie(this);
    }
}
