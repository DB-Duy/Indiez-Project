using System;
using TMPro;
using UnityEngine;

public class TimerAndFPSCounter : MonoBehaviour
{
    public TMP_Text timerText;
    public TMP_Text fpsText;

    private float elapsedTime = 0f;
    private int minutes, seconds;
    private int frameCount = 0;
    private float deltaTime = 0f;
    private const float updateInterval = 0.5f;
    private float fps = 0f;
    [SerializeField, HideInInspector]
    private ActionManager _actionManager;
    private bool isDead = false;
    private void OnValidate()
    {
        _actionManager = FindObjectOfType<ActionManager>();
    }
    private void Awake()
    {
        _actionManager.OnPlayerDead += StopTimerOnDead;
    }

    private void StopTimerOnDead()
    {
        isDead = true;
    }

    private void Update()
    {

        UpdateTimer();

        UpdateFPS();
    }

    private void UpdateFPS()
    {
        frameCount++;
        deltaTime += Time.unscaledDeltaTime;

        if (deltaTime > updateInterval)
        {
            fps = frameCount / deltaTime;
            frameCount = 0;
            deltaTime -= updateInterval;

            fpsText.SetText("FPS: {0}", (int)fps);
        }
    }

    private void UpdateTimer()
    {
        if (isDead) return;
        elapsedTime += Time.deltaTime;
        minutes = Mathf.FloorToInt(elapsedTime / 60F);
        seconds = Mathf.FloorToInt(elapsedTime % 60F);

        timerText.SetText("{0:00}:{1:00}", minutes, seconds);
    }
}
