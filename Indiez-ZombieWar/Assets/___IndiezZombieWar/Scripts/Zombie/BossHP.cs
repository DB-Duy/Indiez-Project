using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BossHP : ZombieHP
{
    [SerializeField]
    private SkinnedMeshRenderer _skinnedMesh;
    [SerializeField]
    private Collider Collider;
    [SerializeField]
    private Animator _animator;
    private Material _material;
    private int runHash, fallHash, landHash, attackHash, dieHash;
    [SerializeField]
    private NavMeshAgent _agent;
    [SerializeField, HideInInspector]
    private Transform _player;

    public float agentSpeed;
    public float attackRange;
    public int attackDamage;
    public bool isAttacking = false;

    private Tween DealDamageTween;
    private void OnValidate()
    {
        _skinnedMesh = GetComponentInChildren<SkinnedMeshRenderer>();
        Collider = GetComponent<Collider>();
        _animator = GetComponent<Animator>();
    }

    protected override void Awake()
    {
        base.Awake();
        landHash = Animator.StringToHash("Land");
        dieHash = Animator.StringToHash("Die");
        runHash = Animator.StringToHash("Run");
        attackHash = Animator.StringToHash("Attack");
        _player = FindObjectOfType<CharacterAnimationController>().transform;
        _material = _skinnedMesh.material;
    }
    protected override void DissolveZombie()
    {
        DOTween.Sequence()
                .AppendInterval(4f)
                .Append(_material.DOFloat(1f, "_DissolveAmount", 1.5f))
                .OnComplete(() =>
                {
                    DisableBoss();
                    gameObject.SetActive(false);
                });
    }
    public override void TakeDamage(int damage, Vector3 rotateTowardsDamageSource = default)
    {
        //base.TakeDamage(damage, rotateTowardsDamageSource);

        HP -= damage;
        if (HP <= 0)
        {
            Collider.enabled = false;
            _animator.Play(dieHash);
            _agent.isStopped = true;
            DissolveZombie();
        }
    }
    protected override void ResumeChase()
    {

    }
    protected override void OnEnable()
    {

    }
    [SerializeField]
    private ParticleSystem _hitGroundFx;
    public void ActivateBoss()
    {
        _animator.Play(landHash);
        _hitGroundFx.Play();
        InvokeRepeating(nameof(UpdateAgentTarget), 0, 0.25f);
        InvokeRepeating(nameof(CheckAttackRange), 1, 0.25f);
        _agent.enabled = true;
    }
    private void DisableBoss()
    {
        CancelInvoke(nameof(CheckAttackRange));
        CancelInvoke(nameof(UpdateAgentTarget));
        _agent.isStopped = true;
    }
    private void CheckDealDamage()
    {
        if (Vector3.SqrMagnitude(transform.position - _player.transform.position) < attackRange * attackRange)
        {
            _actionManager.PlayerReceiveDamage(transform, attackDamage);
        }
    }
    private void UpdateAgentTarget()
    {
        if (HP <= 0) { return; }
        _agent.SetDestination(_player.transform.position);
    }
    [SerializeField]
    private Transform vfxDiePos;
    public void PlayDieVfx()
    {
        _hitGroundFx.transform.parent = vfxDiePos;
        _hitGroundFx.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        _hitGroundFx.Play();
    }
    private void CheckAttackRange()
    {
        if (isAttacking || HP <= 0) { return; }
        if (_agent.remainingDistance < attackRange)
        {
            _agent.speed = 1f;
            _animator.Play(attackHash);
        }
        else
        {
            _agent.speed = agentSpeed;
            _animator.Play(runHash);
        }
    }
}
