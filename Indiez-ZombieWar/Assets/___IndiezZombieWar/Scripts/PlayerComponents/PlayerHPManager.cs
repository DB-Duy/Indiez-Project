using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHPManager : MonoBehaviour
{
    public int maxHP, HP;
    [SerializeField]
    private Image _bar;
    [SerializeField, HideInInspector]
    private ActionManager _actionManager;
    [SerializeField]
    private SkinnedMeshRenderer _skinnedMeshRenderer;
    private Tween _highLightTween;
    private Material _mat;
    public int HPPickupValue = 10;
    private void OnValidate()
    {
        _actionManager = FindObjectOfType<ActionManager>();
    }

    private void Awake()
    {
        _actionManager.OnPlayerReceiveDamage += PlayerTakeDamage;

        _mat = _skinnedMeshRenderer.material;
        _highLightTween = DOTween.Sequence()
                .Append(_mat.DOFloat(3f, "_HighlightIntensity", 0.1f))
                .Append(_mat.DOFloat(0f, "_HighlightIntensity", 0.1f))
                .SetAutoKill(false)
                .SetRecyclable(true)
                .Pause();
    }
    private void HealPlayer(Pickup pickup)
    {
        if (HP + HPPickupValue > maxHP)
        {
            HP = maxHP;
        }
        else
        {
            HP += HPPickupValue;
        }
        _bar.fillAmount = (float)HP / maxHP;
    }
    private void OnDestroy()
    {
        _actionManager.OnPlayerReceiveDamage -= PlayerTakeDamage;
    }
    private void PlayerTakeDamage(Transform source, int damage)
    {
        if (HP <= 0)
        {
            return;
        }
        _highLightTween.Restart();
        HP -= damage;
        _bar.fillAmount = (float)HP / maxHP;
        if (HP <= 0)
        {
            _actionManager.PlayerDead();
        }
    }

    public void PlayerPickUpHealth(Pickup pickup)
    {
        HealPlayer(pickup);
        _actionManager.PlayerPickUpHealth(pickup);
    }
}
