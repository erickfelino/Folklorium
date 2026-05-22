using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Folklorium;

public class DeckCardView : MonoBehaviour
{
    [SerializeField] private Image artImage;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text manaText;
    [SerializeField] private TMP_Text roleText;
    [SerializeField] private TMP_Text bodyText;
    [SerializeField] private TMP_Text attackText;
    [SerializeField] private TMP_Text lifeText;
    [SerializeField] private GameObject selectedBadge;
    [SerializeField] private Button button;

    [Header("Selection Sockets")]
    [SerializeField] private GameObject[] selectionSockets;

    private CardData boundCard;
    private Action<CardData> onClick;

    public void Bind(CardData card, Action<CardData> clickCallback, int selectedCount)
    {
        boundCard = card;
        onClick = clickCallback;

        if (artImage != null)
        {
            artImage.sprite = card != null ? card.art : null;
            artImage.enabled = card != null && card.art != null;
        }

        if (backgroundImage != null)
            backgroundImage.color = GetColorForCard(card);

        if (nameText != null) nameText.text = card != null ? card.cardName : "";
        if (manaText != null) manaText.text = card != null ? card.mana.ToString() : "";
        if (roleText != null) roleText.text = card != null ? card.cardRole.ToString() : "";
        if (bodyText != null) bodyText.text = card != null ? card.cardBody.ToString() : "";
        if (attackText != null) attackText.text = card != null ? card.attack.ToString() : "";
        if (lifeText != null) lifeText.text = card != null ? card.life.ToString() : "";

    
        RefreshSelectionSockets(selectedCount);

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => onClick?.Invoke(boundCard));
        }
    }

    public void SetSelected(bool selected)
    {
        if (selectedBadge != null)
            selectedBadge.SetActive(selected);
    }

    private void RefreshSelectionSockets(int selectedCount)
    {
        if (selectionSockets == null)
            return;

        for (int i = 0; i < selectionSockets.Length; i++)
        {
            if (selectionSockets[i] != null)
                selectionSockets[i].SetActive(i < selectedCount);
        }
    }
    private Color GetColorForCard(CardData card)
    {
        if (card == null)
            return Color.gray;

        switch (card.cardColorGroup)
        {
            case CardData.CardType.Red:
                return new Color(0.9f, 0.2f, 0.2f);
            case CardData.CardType.Blue:
                return new Color(0.2f, 0.35f, 0.9f);
            case CardData.CardType.Yellow:
                return new Color(0.9f, 0.8f, 0.2f);
            case CardData.CardType.Green:
                return new Color(0.2f, 0.7f, 0.3f);
            case CardData.CardType.Neutral:
            default:
                return new Color(0.7f, 0.7f, 0.7f);
        }
    }
}

