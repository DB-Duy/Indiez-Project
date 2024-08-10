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
    private void Awake()
    {
        _mainCam = Camera.main;
        rotationY = transform.rotation.eulerAngles.y;
    }
    public void OnDrag(BaseEventData data)
    {
        PointerEventData eventData = (PointerEventData)data;
        RotateOnDrag(eventData.delta);
    }
    private void RotateOnDrag(Vector2 dragDelta)
    {
        rotationY += dragDelta.x * rotationSpeed * Time.deltaTime;

        Vector3 rot = transform.rotation.eulerAngles;
        rot.y = rotationY;
        transform.rotation = Quaternion.Euler(rot);
    }
}
