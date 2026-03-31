using UnityEngine;
using Folklorium;

public class CardPlayController : MonoBehaviour
{
    public static CardPlayController Instance { get; private set; }

    [SerializeField] private TurnManager turnManager;
    [SerializeField] private BoardManager boardManager;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        ResolveReferences();
    }

    private void ResolveReferences()
    {
        if (turnManager == null)
            turnManager = FindFirstObjectByType<TurnManager>();

        if (boardManager == null)
            boardManager = BoardManager.Instance != null ? BoardManager.Instance : FindFirstObjectByType<BoardManager>();
    }

    public bool CanBeginDrag(CardDrag cardDrag)
    {
        ResolveReferences();

        if (cardDrag == null || cardDrag.IsPlayed)
            return false;

        if (turnManager == null || !turnManager.IsPlayerTurn)
            return false;

        if (cardDrag.ManaManagerRef == null)
            return false;

        return cardDrag.ManaManagerRef.HasEnoughMana(cardDrag.ManaCost);
    }

    public bool TryPlayCard(CardDrag cardDrag, BoardSlot slot)
    {
        ResolveReferences();

        if (cardDrag == null || slot == null)
            return false;

        if (turnManager == null || !turnManager.IsPlayerTurn)
            return false;

        if (cardDrag.ManaManagerRef == null || !cardDrag.ManaManagerRef.HasEnoughMana(cardDrag.ManaCost))
            return false;

        CardCombat combat = cardDrag.GetComponent<CardCombat>();
        if (combat == null || boardManager == null)
            return false;

        if (!boardManager.TryPlaceCard(combat, slot, BoardEntryType.PlayedFromHand))
            return false;

        cardDrag.ManaManagerRef.SpendMana(cardDrag.ManaCost);
        cardDrag.HandManagerRef?.RemoveCardFromHand(cardDrag.gameObject);

        CardBoardView boardView = cardDrag.GetComponent<CardBoardView>();
        if (boardView != null)
        {
            boardView.PlayPlacementAnimation(slot, combat.isEnemy);
            boardManager.NotifyCardPlaced(combat, slot, BoardEntryType.PlayedFromHand);
            combat.TriggerEffects(EffectTriggerType.OnPlay);
        }
        else
        {
            cardDrag.MarkAsPlayed();
            boardManager.NotifyCardPlaced(combat, slot, BoardEntryType.PlayedFromHand);
            combat.TriggerEffects(EffectTriggerType.OnPlay);
        }

        return true;
    }

    public void SetBoardHighlight(CardDrag cardDrag, bool active)
    {
        ResolveReferences();

        if (boardManager == null || cardDrag == null)
            return;

        CardDisplay display = cardDrag.GetComponent<CardDisplay>();
        CardCombat combat = cardDrag.GetComponent<CardCombat>();

        if (display == null || display.cardData == null || combat == null)
            return;

        Color highlightColor = GetHighlightColor(display.cardData.cardRole);

        boardManager.HighlightValidSlots(
            display.cardData,
            combat.isEnemy,
            active,
            highlightColor
        );
    }

    private Color GetHighlightColor(CardData.CardRole role)
    {
        switch (role)
        {
            case CardData.CardRole.Soldier: return Color.green;
            case CardData.CardRole.Hero: return Color.blue;
            case CardData.CardRole.Commander: return Color.red;
            default: return Color.white;
        }
    }

    public void PlayDeniedFeedback(CardDrag cardDrag)
    {
        cardDrag?.ShakeInvalidPlay();
    }
}