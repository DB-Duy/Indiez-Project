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
    private MultiAimConstraint _aimConstraint;
    [SerializeField]
    private Transform _playerAim;
    private Vector3 offset = new Vector3(0, 1.5f, 0);
    private int _reloadId, _switchGunId, _shootGunId, _deadId;
    [SerializeField, HideInInspector]
    private ActionManager _actionManager;
    [SerializeField]
    private Rig _aimingRig;
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
    }
    private void PlayDeadAnim()
    {
        _aimingRig.weight = 0f;
        animator.SetTrigger(_deadId);
    }
    private void PlayShootAnim()
    {
        animator.SetTrigger(_shootGunId);
    }

    private void Start()
    {
        _reloadId = Animator.StringToHash("Reload");
        _switchGunId = Animator.StringToHash("SwitchGun");
        _shootGunId = Animator.StringToHash("Shoot");
        _deadId = Animator.StringToHash("Dead");
    }
    private void OnDestroy()
    {
        _actionManager.OnTargetAcquired -= OnGetTarget;
        _actionManager.OnPerformReload -= PlayReloadAnim;
        _actionManager.OnEquipGun -= EquipGun;
    }
    private void EquipGun(Gun gun)
    {

    }
    private void Update()
    {
        if (RotateTarget == null)
        {
            _playerAim.position = transform.position + transform.forward * 4 + offset;
        }
        else
        {
            _playerAim.position = RotateTarget.position + offset;
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
