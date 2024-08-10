using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFOV : MonoBehaviour
{
    public float portraitFOV = 60f, landscapeFOV = 38f;
    private bool _isPortrait;
    public event Action OnOrientationChanged;
    [SerializeField]
    private CinemachineVirtualCamera _virtualCamera;
    public bool IsPortrait
    {
        get { return _isPortrait; }
        set
        {
            if (_isPortrait != value)
            {
                _isPortrait = value;
                OnOrientationChanged?.Invoke();
            }
        }
    }
    private void Start()
    {
        UpdateFOV();
    }
    private void OnValidate()
    {
        _virtualCamera = GetComponent<CinemachineVirtualCamera>();
    }
    private void Update()
    {
        IsPortrait = Screen.orientation == ScreenOrientation.Portrait || Screen.orientation == ScreenOrientation.PortraitUpsideDown;
    }
    private void Awake()
    {
        _isPortrait = Screen.orientation == ScreenOrientation.Portrait || Screen.orientation == ScreenOrientation.PortraitUpsideDown;
        OnOrientationChanged += UpdateFOV;
    }
    private void OnDestroy()
    {
        OnOrientationChanged -= UpdateFOV;
    }
    private void UpdateFOV()
    {
        _virtualCamera.m_Lens.FieldOfView = IsPortrait ? portraitFOV : landscapeFOV;
    }
}
