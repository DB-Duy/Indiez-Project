using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIGunInfo : MonoBehaviour
{
    [SerializeField, HideInInspector]
    private ActionManager _actionManager;
    private Gun currentGun;
    [SerializeField]
    private Image _gunIcon;
    [SerializeField]
    private TMP_Text _bulletText;
    private StringBuilder _stringBuilder = new StringBuilder(10);
    private Tween changeGunIconTween;
    private void OnValidate()
    {
        _actionManager = FindObjectOfType<ActionManager>();
    }
    private void Awake()
    {
        _actionManager.OnEquipGun += EquipGunUI;
        _actionManager.OnPerformShoot += UpdateGunInfo;
        _actionManager.OnReloadComplete += UpdateGunInfo;
        _actionManager.OnPlayerPickUpAmmo += UpdateGunInfoPickup;

        changeGunIconTween = DOTween.Sequence()
            .Append(_gunIcon.DOFade(0, 0.1f))
            .AppendCallback(() =>
            {
                _gunIcon.sprite = currentGun.gunSprite;
                UpdateGunInfo();
            })
            .Append(_gunIcon.DOFade(1, 0.1f))
            .SetRecyclable(true)
            .SetAutoKill(false)
            .Pause();
    }
    private void OnDestroy()
    {
        _actionManager.OnEquipGun -= EquipGunUI;
        _actionManager.OnPerformShoot -= UpdateGunInfo;
        _actionManager.OnReloadComplete -= UpdateGunInfo;
    }
    private void EquipGunUI(Gun gun)
    {
        currentGun = gun;
        changeGunIconTween.Restart();
    }
    private void UpdateGunInfo()
    {
        _stringBuilder.Clear();
        _stringBuilder.Append(currentGun.currentMagazineSize);
        _stringBuilder.Append('/');
        _stringBuilder.Append(currentGun.ammoAvailable);
        _bulletText.SetText(_stringBuilder);
    }
    private void UpdateGunInfoPickup(Pickup pickup)
    {
        UpdateGunInfo();
    }
}
