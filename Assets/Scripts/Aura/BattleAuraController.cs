using System.Collections.Generic;
using UnityEngine;
using Folklorium;

public class BattleAuraController : MonoBehaviour
{
    [Header("Board Event")]
    [SerializeField] private BoardCardPlacedEventChannelSO boardCardPlacedChannel;

    [Header("Spell Slots")]
    [SerializeField] private SpellSlot[] spellSlots;

    [Header("Side")]
    [SerializeField] private bool isEnemySide;

    [Header("Turn Manager")]
    [SerializeField] private TurnManager turnManager;
    [Header("Aura Visual")]
    [SerializeField] private Renderer auraIndicator;
    [SerializeField] private Color pureRedColor = Color.red;
    [SerializeField] private Color pureBlueColor = Color.blue;
    [SerializeField] private Color redBlueColor = new Color(0.55f, 0.0f, 0.8f, 1f);

    private DeckAuraKind auraKind;
    private bool subscribed;
    private bool firstCreatureBuffConsumed;

    public DeckAuraKind AuraKind => auraKind;

    public int SpellManaModifier => auraKind == DeckAuraKind.PureBlue ? -1 : 0;
    public int ExtraSpellUses => auraKind == DeckAuraKind.RedBlue ? 1 : 0;

    private void Awake()
    {
        if (turnManager == null)
            turnManager = FindFirstObjectByType<TurnManager>();
    }

    private void OnEnable()
    {
        SubscribeBoard();
        SubscribeTurn();
    }

    private void OnDisable()
    {
        UnsubscribeBoard();
        UnsubscribeTurn();
    }

    public void SetupFromDeck(IEnumerable<CardData> deckCards, bool enemySide, SpellSlot[] slots)
    {
        isEnemySide = enemySide;
        spellSlots = slots;
        auraKind = DeckAuraResolver.ResolveFromDeck(deckCards);
        firstCreatureBuffConsumed = false;

        ApplySpellModifiersToSlots();
        UpdateAuraVisual();
    }

    private void SubscribeBoard()
    {
        if (subscribed || boardCardPlacedChannel == null)
            return;

        boardCardPlacedChannel.OnEventRaised += HandleBoardCardPlaced;
        subscribed = true;
    }

    private void UnsubscribeBoard()
    {
        if (!subscribed || boardCardPlacedChannel == null)
            return;

        boardCardPlacedChannel.OnEventRaised -= HandleBoardCardPlaced;
        subscribed = false;
    }

    private void SubscribeTurn()
    {
        if (turnManager == null)
            return;

        turnManager.OnTurnChanged += HandleTurnChanged;
    }

    private void UnsubscribeTurn()
    {
        if (turnManager == null)
            return;

        turnManager.OnTurnChanged -= HandleTurnChanged;
    }

    private void HandleTurnChanged(bool isPlayerTurn)
    {
        if (auraKind != DeckAuraKind.PureRed)
            return;

        bool isThisSideTurn = isEnemySide ? !isPlayerTurn : isPlayerTurn;

        if (isThisSideTurn)
        {
            firstCreatureBuffConsumed = false;
        }
    }

    private void HandleBoardCardPlaced(BoardCardPlacedEventPayload payload)
    {
        if (auraKind != DeckAuraKind.PureRed)
            return;

        if (firstCreatureBuffConsumed)
            return;

        if (payload.card == null)
            return;

        if (payload.card.isEnemy != isEnemySide)
            return;

        CardDisplay display = payload.card.GetComponent<CardDisplay>();
        if (display == null || display.cardData == null)
            return;

        if (display.cardData.cardRole == CardData.CardRole.Spell)
            return;

        firstCreatureBuffConsumed = true;

        if (ActionSystem.Instance != null)
        {
            ActionSystem.Instance.AddAction(new AuraBuffAction(payload.card, 1, 0));
        }
    }

    private void ApplySpellModifiersToSlots()
    {
        if (spellSlots == null)
            return;

        int manaModifier = SpellManaModifier;
        int extraUses = ExtraSpellUses;

        foreach (var slot in spellSlots)
        {
            if (slot != null)
            {
                slot.ApplyAuraModifiers(manaModifier, extraUses);
            }
        }
    }

    private void UpdateAuraVisual()
    {
        if (auraIndicator == null)
            return;

        Color finalColor = Color.white;

        switch (auraKind)
        {
            case DeckAuraKind.PureRed:
                finalColor = pureRedColor;
                break;

            case DeckAuraKind.PureBlue:
                finalColor = pureBlueColor;
                break;

            case DeckAuraKind.RedBlue:
                finalColor = redBlueColor;
                break;
        }

        auraIndicator.material.color = finalColor;
    }
}