using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Folklorium;

public class DeckEntryView : MonoBehaviour
{
    [SerializeField] private Image artImage;
    [SerializeField] private TMP_Text labelText;
    [SerializeField] private Button removeButton;

    private Action onRemove;

    public void Bind(CardData card, string label, Action removeCallback)
    {
        if (artImage != null) artImage.sprite = card != null ? card.art : null;
        if (labelText != null) labelText.text = label;

        onRemove = removeCallback;

        if (removeButton != null)
        {
            removeButton.onClick.RemoveAllListeners();
            removeButton.onClick.AddListener(() => onRemove?.Invoke());
        }
    }
}