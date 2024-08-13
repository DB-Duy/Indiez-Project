using DG.Tweening;
using GPUInstancer.CrowdAnimations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieHP : MonoBehaviour
{
    public int HP, maxHP;

    [SerializeField]
    private GPUICrowdPrefab _crowdPrefab;
    [SerializeField]
    private ZombieInstance _zombieInstance;
    [SerializeField]
    private AnimationClip[] _deathClips, _behitClips;
    private Tween _deathTween, _behitTween;
    [SerializeField]
    private ZombieAttack _zombAttack;
    [SerializeField, HideInInspector]
    protected ActionManager _actionManager;
    private AnimationClip _deathClip, _behitClip;
    public bool isStagger = false;
    protected virtual void Awake()
    {
        _actionManager = FindObjectOfType<ActionManager>();
        _deathTween = DOTween.Sequence()
            .AppendInterval(2.5f)
            .AppendCallback(DissolveZombie)
            .AppendInterval(0.5f)
            .AppendCallback(() => gameObject.SetActive(false))
            .SetRecyclable(true)
            .SetAutoKill(false)
            .Pause();
        _behitTween = DOTween.Sequence()
            .AppendInterval(0.5f)
            .AppendCallback(ResumeChase)
            .SetRecyclable(true)
            .SetAutoKill(false)
            .Pause();
    }
    protected virtual void ResumeChase()
    {
        if (HP <= 0)
        {
            return;
        }
        _zombieInstance.PlayAnimation(_zombieInstance.RunningClip);
        _zombieInstance.agent.isStopped = false;
        isStagger = false;
    }
    protected virtual void DissolveZombie()
    {
        _zombieInstance.ZombieFall(animTime: _deathClip.length);
    }
    private void OnValidate()
    {
        _crowdPrefab = GetComponent<GPUICrowdPrefab>();
        _zombieInstance = GetComponent<ZombieInstance>();
        _zombAttack = GetComponent<ZombieAttack>();
    }
    public virtual void TakeDamage(int damage, Vector3 rotateTowardsDamageSource = new Vector3())
    {
        HP -= damage;
        AudioPool.Instance.PlayZombieSound();
        if (HP <= 0)
        {
            _zombieInstance.SetCollider(false);
            _deathClip = _deathClips[Random.Range(0, _deathClips.Length)];
            _zombieInstance.PlayAnimation(_deathClip, speed: 1.5f);
            _zombieInstance.agent.speed = 0;
            if (rotateTowardsDamageSource != Vector3.zero)
            {
                transform.LookAt(rotateTowardsDamageSource, Vector3.up);
            }
            _deathTween.Restart();
            _actionManager.ZombieKilled(this);
        }
        else
        {
            isStagger = true;
            _behitClip = _behitClips[Random.Range(0, _behitClips.Length)];
            _zombieInstance.PlayAnimation(_behitClip, speed: 2f);
            _zombieInstance.agent.isStopped = true;
            _behitTween.Restart();
        }
    }
    protected virtual void OnEnable()
    {
        HP = maxHP;
        _zombieInstance.SetCollider(true);
    }
}
