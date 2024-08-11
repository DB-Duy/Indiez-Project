using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponCache : MonoBehaviour
{
    public bool IsTouched = false;
    public bool IsActiveEvent = false;
    public bool IsPlayerInside = false;
    [SerializeField]
    private StageManager _stageManager;
    [SerializeField]
    private SpriteRenderer[] _ringSprite;
    [SerializeField]
    private Animator _flagAnimator;
    [SerializeField]
    private GameObject _barriers;

    private void OnValidate()
    {
        _stageManager = FindObjectOfType<StageManager>();
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (!IsTouched && _stageManager.CanActivateEvent())
        {
            IsTouched = true;
            _stageManager.ActivateEvent(this);
            ActivateWeaponCacheEvent();
        }
    }

    private void ActivateWeaponCacheEvent()
    {
        _barriers.SetActive(true);
    }
}
