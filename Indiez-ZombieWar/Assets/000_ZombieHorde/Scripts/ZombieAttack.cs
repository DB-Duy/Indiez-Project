using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieAttack : MonoBehaviour
{
    [SerializeField]
    private ZombieInstance _zombInstance;
    [SerializeField]
    private AnimationClip[] _attackClips;
    public float attackRange;
    public int attackDamage;
    private Tween DealDamageTween;
    [SerializeField, HideInInspector]
    private ActionManager _actionManager;
    public bool isAttacking = false;
    private Transform _player;
    [SerializeField]
    private ZombieHP _zombHP;

    private void OnValidate()
    {
        _zombInstance = GetComponent<ZombieInstance>();
        _zombHP = GetComponent<ZombieHP>();
    }
    private void Awake()
    {
        _actionManager = FindObjectOfType<ActionManager>(true);
        _player = GameObject.FindGameObjectWithTag("Player").transform;
        InitTween();
    }

    private void InitTween()
    {
        DealDamageTween = DOTween.Sequence()
            .AppendCallback(() => isAttacking = true)
            .AppendInterval(0.75f)
            .AppendCallback(CheckDealDamage)
            .AppendInterval(0.3f)
            .OnComplete(() => isAttacking = false)
            .SetAutoKill(false)
            .SetRecyclable(true)
            .Pause();
    }
    private void OnEnable()
    {
        InvokeRepeating(nameof(CheckAttackRange), 1, 0.2f);
        InvokeRepeating(nameof(UpdateAgentTarget), 1, 0.2f);
    }
    private void OnDisable()
    {
        CancelInvoke(nameof(CheckAttackRange));
        CancelInvoke(nameof(UpdateAgentTarget));
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
        if (_zombHP.HP <= 0) { return; }
        _zombInstance.agent.SetDestination(_player.transform.position);
    }
    private void CheckAttackRange()
    {
        if (isAttacking || _zombHP.HP <= 0 || _zombHP.isStagger) { return; }
        if (_zombInstance.agent.remainingDistance < attackRange)
        {
            _zombInstance.agent.speed = 0.1f;
            _zombInstance.PlayAnimation(_attackClips[Random.Range(0, _attackClips.Length)], 0, 2.5f);
            DealDamageTween.Restart();
        }
        else if (_zombInstance.agent.speed != _zombInstance.currentAgentSpeed)
        {
            _zombInstance.agent.speed = _zombInstance.currentAgentSpeed;
            _zombInstance.PlayAnimation(_zombInstance.RunningClip);

        }
    }
}
