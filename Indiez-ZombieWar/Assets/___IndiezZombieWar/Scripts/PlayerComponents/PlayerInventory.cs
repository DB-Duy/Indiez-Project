using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public Gun[] _availableGuns;
    public Gun currentGun;
    [SerializeField, HideInInspector]
    ActionManager _actionManager;

    private void OnValidate()
    {
        _actionManager = FindObjectOfType<ActionManager>();
        _availableGuns = GetComponentsInChildren<Gun>();
    }
    private void Awake()
    {
        _actionManager.OnEquipGun += EquipGun;
    }
    private void Start()
    {

        _actionManager.EquipGun(currentGun);
    }
    private void OnDestroy()
    {
        _actionManager.OnEquipGun -= EquipGun;
    }
    public void EquipGun(Gun gun)
    {
        currentGun = gun;
    }
}
