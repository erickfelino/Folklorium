using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class GameOverUI : MonoBehaviour
{
    [Header("Root")]
    [SerializeField] private GameObject rootPanel;

    [Header("Fade Overlay")]
    [SerializeField] private Image dimmerImage;
    [SerializeField] private float dimmerTargetAlpha = 0.65f;
    [SerializeField] private float dimmerFadeDuration = 0.45f;

    [Header("Victory / Defeat Text")]
    [SerializeField] private CanvasGroup resultGroup;
    [SerializeField] private TMP_Text resultText;
    [SerializeField] private float resultFadeDuration = 0.25f;

    [Header("Buttons")]
    [SerializeField] private CanvasGroup buttonsGroup;
    [SerializeField] private float buttonsFadeDuration = 0.25f;

    private Sequence sequence;

    private void Awake()
    {
        HideImmediate();
    }

    private void HideImmediate()
    {
        if (sequence != null && sequence.IsActive())
            sequence.Kill();

        if (rootPanel != null)
            rootPanel.SetActive(false);

        if (dimmerImage != null)
        {
            Color c = dimmerImage.color;
            c.a = 0f;
            dimmerImage.color = c;
        }

        if (resultGroup != null)
        {
            resultGroup.alpha = 0f;
            resultGroup.interactable = false;
            resultGroup.blocksRaycasts = false;
        }

        if (buttonsGroup != null)
        {
            buttonsGroup.alpha = 0f;
            buttonsGroup.interactable = false;
            buttonsGroup.blocksRaycasts = false;
        }
    }

    public void ShowVictory()
    {
        Show("Vitória!");
    }

    public void ShowDefeat()
    {
        Show("Derrota!");
    }

    private void Show(string message)
    {
        if (sequence != null && sequence.IsActive())
            sequence.Kill();

        if (rootPanel != null)
            rootPanel.SetActive(true);

        if (resultText != null)
            resultText.text = message;

        if (dimmerImage != null)
        {
            Color c = dimmerImage.color;
            c.a = 0f;
            dimmerImage.color = c;
        }

        if (resultGroup != null)
        {
            resultGroup.alpha = 0f;
            resultGroup.interactable = false;
            resultGroup.blocksRaycasts = false;
        }

        if (buttonsGroup != null)
        {
            buttonsGroup.alpha = 0f;
            buttonsGroup.interactable = false;
            buttonsGroup.blocksRaycasts = false;
        }

        Time.timeScale = 0f;

        sequence = DOTween.Sequence().SetUpdate(true);

        if (dimmerImage != null)
        {
            sequence.Append(dimmerImage.DOFade(dimmerTargetAlpha, dimmerFadeDuration).SetUpdate(true));
        }

        sequence.AppendInterval(0.15f);

        if (resultGroup != null)
        {
            sequence.Append(resultGroup.DOFade(1f, resultFadeDuration).SetUpdate(true));
        }

        sequence.AppendInterval(0.15f);

        if (buttonsGroup != null)
        {
            sequence.Append(buttonsGroup.DOFade(1f, buttonsFadeDuration).SetUpdate(true));
            sequence.OnComplete(() =>
            {
                buttonsGroup.interactable = true;
                buttonsGroup.blocksRaycasts = true;
            });
        }
    }

    public void PlayAgain()
    {
        Time.timeScale = 1f;
        SceneLoader.LoadGame();
    }

    public void BackToMenu()
    {
        Time.timeScale = 1f;
        SceneLoader.LoadMainMenu();
    }

    public void Quit()
    {
        SceneLoader.QuitGame();
    }
}