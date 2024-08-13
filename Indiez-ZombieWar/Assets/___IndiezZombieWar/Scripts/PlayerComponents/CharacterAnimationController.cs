
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class CharacterAnimationController : MonoBehaviour
{
    [SerializeField]
    private Animator animator;
    [HideInInspector]
    public Transform RotateTarget;
    [SerializeField]
    private Transform _playerAim;
    private Vector3 offset = new Vector3(0, 1.5f, 0);
    private Vector3 offsetRecoil;
    private int _reloadId, _switchGunId, _shootGunId, _deadId;
    [SerializeField, HideInInspector]
    private ActionManager _actionManager;
    [SerializeField]
    private Rig _aimingRig;

    private Tween _recoilTween;
    private bool isRecoil = false;
    private void OnValidate()
    {
        _actionManager = FindObjectOfType<ActionManager>();
        animator = GetComponent<Animator>();
    }
    private void Awake()
    {
        _actionManager.OnTargetAcquired += OnGetTarget;
        _actionManager.OnPerformReload += PlayReloadAnim;
        _actionManager.OnEquipGun += EquipGun;
        _actionManager.OnPerformShoot += PlayShootAnim;
        _actionManager.OnPlayerDead += PlayDeadAnim;

        offsetRecoil = offset;
        _recoilTween = DOTween.Sequence()
            .Append(DOTween.To(() => offsetRecoil, (x) => offsetRecoil = x, offset + new Vector3(-0.5f, 1f, 0), 0.05f).SetRecyclable(true)
            .SetAutoKill(false))
            .Append(DOTween.To(() => offsetRecoil, (x) => offsetRecoil = x, offset, 0.05f).SetRecyclable(true)
            .SetAutoKill(false))
            .SetRecyclable(true)
            .SetAutoKill(false)
            .Pause();

        _reloadId = Animator.StringToHash("Reload");
        _switchGunId = Animator.StringToHash("SwitchGun");
        _shootGunId = Animator.StringToHash("Shoot");
        _deadId = Animator.StringToHash("Dead");
    }
    private bool isDead = false;
    private void PlayDeadAnim()
    {
        isDead = true;
        _aimingRig.weight = 0f;
        animator.SetTrigger(_deadId);
    }
    private void PlayShootAnim()
    {
        animator.SetTrigger(_shootGunId);
        isRecoil = true;
        _recoilTween.Restart();
    }

    private void OnDestroy()
    {
        _actionManager.OnTargetAcquired -= OnGetTarget;
        _actionManager.OnPerformReload -= PlayReloadAnim;
        _actionManager.OnEquipGun -= EquipGun;
        _actionManager.OnPerformShoot -= PlayShootAnim;
        _actionManager.OnPlayerDead -= PlayDeadAnim;
    }
    private void EquipGun(Gun gun)
    {
        animator.SetTrigger(_switchGunId);
    }
    private void Update()
    {
        if (isDead)
        {
            return;
        }
        Vector3 gunOffset = isRecoil ? offsetRecoil : offset;
        if (RotateTarget == null)
        {
            _playerAim.position = transform.position + transform.forward * 4 + gunOffset;
        }
        else
        {
            _playerAim.position = RotateTarget.position + gunOffset;
        }
    }
    private void OnGetTarget(Transform target)
    {
        RotateTarget = target;
    }
    public void PlayReloadAnim()
    {
        animator.SetTrigger(_reloadId);
    }
}
