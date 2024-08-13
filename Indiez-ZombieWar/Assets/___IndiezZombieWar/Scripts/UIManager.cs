using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;
    [SerializeField, HideInInspector]
    private ActionManager _actionManager;
    [SerializeField]
    private Image _textBox, _retryButton;
    [SerializeField]
    private TMP_Text _tutorialText, _introText, _eventText, _gunUnlockText, _bossfightText, _finalText, _deadText;
    private void OnValidate()
    {
        _actionManager = FindObjectOfType<ActionManager>();
    }
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        _actionManager.OnPlayerDead += ShowDeadText;
    }
    public void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    private void OnDestroy()
    {
        _actionManager.OnPlayerDead -= ShowDeadText;
    }
    private bool hasDisableTutorial = false;
    private void Start()
    {
        DisableTutorial();
        ShowIntro();
    }
    public void DisableTutorial()
    {
        _tutorialText.DOFade(0, 0.5f).SetDelay(6f).OnComplete(() => _tutorialText.gameObject.SetActive(false));
    }
    public void ShowIntro()
    {
        _textBox.gameObject.SetActive(true);
        _introText.gameObject.SetActive(true);
        DOTween.Sequence()
            .Append(_textBox.DOFade(0.8f, 0.5f))
            .Join(_introText.DOFade(1f, 0.5f).SetDelay(0.1f))
            .AppendInterval(3)
            .Append(_introText.DOFade(0f, 0.5f))
            .Join(_textBox.DOFade(0f, 0.5f))
            .OnComplete(() =>
            {
                _textBox.gameObject.SetActive(false);
                _introText.gameObject.SetActive(false);
            });
    }
    public void ShowEvent()
    {
        _textBox.gameObject.SetActive(true);
        _eventText.gameObject.SetActive(true);
        DOTween.Sequence()
            .Append(_textBox.DOFade(0.8f, 0.5f))
            .Join(_eventText.DOFade(1f, 0.5f).SetDelay(0.1f))
            .AppendInterval(3)
            .Append(_eventText.DOFade(0f, 0.5f))
            .Join(_textBox.DOFade(0f, 0.5f))
            .OnComplete(() =>
            {
                _textBox.gameObject.SetActive(false);
                _eventText.gameObject.SetActive(false);
            });
    }
    public void ShowGunUnlock()
    {
        _textBox.gameObject.SetActive(true);
        _gunUnlockText.gameObject.SetActive(true);
        DOTween.Sequence()
            .Append(_textBox.DOFade(0.8f, 0.5f))
            .Join(_gunUnlockText.DOFade(1f, 0.5f).SetDelay(0.1f))
            .AppendInterval(3)
            .Append(_gunUnlockText.DOFade(0f, 0.5f))
            .Join(_textBox.DOFade(0f, 0.5f))
            .OnComplete(() =>
            {
                _textBox.gameObject.SetActive(false);
                _gunUnlockText.gameObject.SetActive(false);
            });
    }
    public void ShowBossFight()
    {
        _textBox.gameObject.SetActive(true);
        _bossfightText.gameObject.SetActive(true);
        DOTween.Sequence()
            .Append(_textBox.DOFade(0.8f, 0.5f))
            .Join(_bossfightText.DOFade(1f, 0.5f).SetDelay(0.1f))
            .AppendInterval(3)
            .Append(_bossfightText.DOFade(0f, 0.5f))
            .Join(_textBox.DOFade(0f, 0.5f))
            .OnComplete(() =>
            {
                _textBox.gameObject.SetActive(false);
                _bossfightText.gameObject.SetActive(false);
            });
    }
    public void ShowFinalText()
    {
        _textBox.gameObject.SetActive(true);
        _finalText.gameObject.SetActive(true);
        DOTween.Sequence()
            .Append(_textBox.DOFade(0.8f, 0.5f))
            .Join(_finalText.DOFade(1f, 0.5f).SetDelay(0.1f))
            .AppendInterval(3)
            .Append(_finalText.DOFade(0f, 0.5f))
            .Join(_textBox.DOFade(0f, 0.5f))
            .OnComplete(() =>
            {
                _textBox.gameObject.SetActive(false);
                _finalText.gameObject.SetActive(false);
            });
    }
    public void ShowDeadText()
    {
        _textBox.gameObject.SetActive(true);
        _deadText.gameObject.SetActive(true);
        DOTween.Sequence()
            .Append(_textBox.DOFade(0.8f, 0.5f))
            .Join(_deadText.DOFade(1f, 0.5f).SetDelay(0.1f))
            .AppendCallback(() =>
            {
                _retryButton.gameObject.SetActive(true);
                _retryButton.DOFade(1, 0.5f);
            });
    }
}
