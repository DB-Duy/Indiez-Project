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
    private SpriteRenderer _ringSprite;
    [SerializeField]
    private Animator _flagAnimator;
    [SerializeField]
    private GameObject _barriers;
    private int _flagDropHash;

    private void OnValidate()
    {
        _stageManager = FindObjectOfType<StageManager>();
    }

    private void OnTriggerEnter(Collider collider)
    {
        if (collider.CompareTag("Player") && !IsTouched && _stageManager.CanActivateEvent())
        {
            IsTouched = true;
            _stageManager.ActivateEvent(this);
        }
    }
    private void Start()
    {
        _flagDropHash = Animator.StringToHash("Raise");
    }

    public void ActivateWeaponCacheEvent()
    {
        _barriers.SetActive(true);
    }
    public void CompleteWeaponCacheEvent()
    {
        _ringSprite.gameObject.SetActive(false);
        _barriers.SetActive(false);
        _flagAnimator.Play(_flagDropHash);
    }
}
