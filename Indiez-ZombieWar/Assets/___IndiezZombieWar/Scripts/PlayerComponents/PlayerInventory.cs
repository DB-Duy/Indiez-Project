using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    [SerializeField]
    public Gun[] AvailableGuns;
    private int currentGunIdx;
    public Gun currentGun;
    [SerializeField, HideInInspector]
    ActionManager _actionManager;

    private void OnValidate()
    {
        _actionManager = FindObjectOfType<ActionManager>();
    }
    private void Awake()
    {
        _actionManager.OnEquipGun += UpdateCurrentGun;
    }
    private void AddAmmoToGuns(Pickup pickup)
    {
        for (int i = 0; i < AvailableGuns.Length; i++)
        {
            AvailableGuns[i].ammoAvailable += AvailableGuns[i].maxMagazineSize;
        }
    }
    private void Start()
    {
        _actionManager.EquipGun(AvailableGuns[0]);
    }
    private void OnDestroy()
    {
        _actionManager.OnEquipGun -= UpdateCurrentGun;
    }
    public void UpdateCurrentGun(Gun gun)
    {
        currentGun = gun;

    }
    public void SetActiveGunAnimEvent()
    {
        for (int i = 0; i < AvailableGuns.Length; i++)
        {
            if (currentGun == AvailableGuns[i])
            {
                AvailableGuns[i].gameObject.SetActive(true);
            }
            else
            {
                AvailableGuns[i].gameObject.SetActive(false);
            }
        }
    }
    public void TryReloadCurrentGun()
    {
        currentGun.TryReload();
    }
    public void ReloadCompleteAnimEvent()
    {
        currentGun.ReloadComplete();
    }
    public void EquipNextGun()
    {
        currentGunIdx++;
        _actionManager.EquipGun(AvailableGuns[currentGunIdx % AvailableGuns.Length]);
    }
    public int GetTotalAmmo()
    {
        int totalAmmo = 0;
        for (int i = 0; i < AvailableGuns.Length; i++)
        {
            totalAmmo += AvailableGuns[i].ammoAvailable;
        }
        return totalAmmo;
    }

    public void PickUpAmmo(Pickup pickup)
    {
        AddAmmoToGuns(pickup);
        _actionManager.PlayerPickUpAmmo(pickup);
    }
}