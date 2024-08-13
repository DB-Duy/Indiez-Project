using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class RotateOnSwipe : MonoBehaviour
{
    public bool EnableMoving = true;
    public float rotationSpeed = 1.0f;
    public RectTransform touchArea;

    private float rotationY = 0f;
    private Camera _mainCam;
    [SerializeField]
    private ActionManager _actionManager;
    [SerializeField]
    private Transform lookAt;
    private bool isDead = false;
    private void OnValidate()
    {
        _actionManager = FindObjectOfType<ActionManager>();
    }
    private void OnDead()
    {
        isDead = true;
    }

    private void Awake()
    {
        _mainCam = Camera.main;
        rotationY = transform.rotation.eulerAngles.y;
        _actionManager.OnPlayerDead += OnDead;
    }
    private void OnDestroy()
    {
        _actionManager.OnPlayerDead -= OnDead;
    }
    public void OnDrag(BaseEventData data)
    {
        PointerEventData eventData = (PointerEventData)data;
        RotateOnDrag(eventData.delta);
    }
    private void RotateOnDrag(Vector2 dragDelta)
    {
        Vector3 rot;
        if (isDead)
        {
            rotationY += dragDelta.x * rotationSpeed * Time.deltaTime;

            rot = lookAt.rotation.eulerAngles;
            rot.y = rotationY;
            lookAt.transform.rotation = Quaternion.Euler(rot);
            return;
        }

        rotationY += dragDelta.x * rotationSpeed * Time.deltaTime;

        rot = transform.rotation.eulerAngles;
        rot.y = rotationY;
        transform.rotation = Quaternion.Euler(rot);
    }
}
