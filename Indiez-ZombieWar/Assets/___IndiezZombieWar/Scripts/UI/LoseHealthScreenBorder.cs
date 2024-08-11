using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoseHealthScreenBorder : MonoBehaviour
{
    [SerializeField]
    private Image top, bottom, left, right;
    [SerializeField, HideInInspector]
    private ActionManager _actionManager;
    [SerializeField, HideInInspector]
    private Transform _playerTransform;
    [SerializeField, HideInInspector]
    private Camera _camera;
    private Tween showTop, showBottom, showLeft, showRight;
    private float alphaFade = 0.5f, duration = 0.5f;
    private void OnValidate()
    {
        _actionManager = FindObjectOfType<ActionManager>();
        _playerTransform = FindObjectOfType<CharacterAnimationController>().transform;
        _camera = Camera.main;
    }
    private void Awake()
    {
        _actionManager.OnPlayerReceiveDamage += ShowUIBloodEffect;
        showTop = DOTween.Sequence()
            .Append(top.DOFade(alphaFade, duration))
            .Append(top.DOFade(0, duration))
            .SetRecyclable(true)
            .SetAutoKill(false)
            .Pause();
        showBottom = DOTween.Sequence()
            .Append(bottom.DOFade(alphaFade, duration))
            .Append(bottom.DOFade(0, duration))
            .SetRecyclable(true)
            .SetAutoKill(false)
            .Pause();
        showLeft = DOTween.Sequence()
            .Append(left.DOFade(alphaFade, duration))
            .Append(left.DOFade(0, duration))
            .SetRecyclable(true)
            .SetAutoKill(false)
            .Pause();
        showRight = DOTween.Sequence()
            .Append(right.DOFade(alphaFade, duration))
            .Append(right.DOFade(0, duration))
            .SetRecyclable(true)
            .SetAutoKill(false)
            .Pause();
    }
    private void ShowUIBloodEffect(Transform source, int damage)
    {
        Vector3 attackDirection = (source.position - _playerTransform.position).normalized;

        // Get the player's forward direction
        Vector3 playerForward = _playerTransform.forward;

        // Calculate the angle between the player's forward and the attack direction
        float angle = Vector3.SignedAngle(playerForward, attackDirection, Vector3.up);


        // Determine which overlay to activate based on the angle
        if (angle > -45 && angle <= 45)
        {
            showTop.Restart();
        }
        else if (angle > 45 && angle <= 135)
        {
            showRight.Restart();
        }
        else if (angle > 135 || angle <= -135)
        {
            showBottom.Restart();
        }
        else if (angle > -135 && angle <= -45)
        {
            showLeft.Restart();
        }
    }
}
