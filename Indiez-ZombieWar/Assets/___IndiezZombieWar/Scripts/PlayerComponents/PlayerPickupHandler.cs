using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPickupHandler : MonoBehaviour
{
    [SerializeField, HideInInspector]
    private ActionManager _actionManager;
    [SerializeField, HideInInspector]
    private PlayerHPManager _playerHPManager;
    [SerializeField, HideInInspector]
    private PlayerInventory _playerInventory;
    [SerializeField]
    private ParticleSystem _hpPickupFx, _bulletPickupFx;
    private void OnValidate()
    {
        _actionManager = FindObjectOfType<ActionManager>();
        _playerHPManager = FindObjectOfType<PlayerHPManager>();
        _playerInventory = FindObjectOfType<PlayerInventory>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PickUp"))
        {
            Pickup pickup = other.GetComponent<Pickup>();
            if (pickup != null)
            {
                if (pickup.pickupType == PickupType.Bullet)
                {
                    _playerInventory.PickUpAmmo(pickup);
                    _bulletPickupFx.Play();
                }
                else
                {
                    _playerHPManager.PlayerPickUpHealth(pickup);
                    _bulletPickupFx.Play();
                }
            }
        }
    }
}
