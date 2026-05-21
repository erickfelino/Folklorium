using System.Collections.Generic;
using System.Linq;
using Folklorium;

public static class DeckRules
{
    public const int MaxMainDeckCards = 20;
    public const int MaxCopiesPerCard = 2;
    public const int MaxCommanderCopies = 1;
    public const int MaxSpellSlots = 2;

    public static bool CanAddMainDeckCard(CardData card, List<CardData> mainDeck)
    {
        if (card == null || mainDeck == null)
            return false;

        if (mainDeck.Count >= MaxMainDeckCards)
            return false;

        int sameCardCount = mainDeck.Count(c => c != null && c.cardName == card.cardName);

        if (card.cardRole == CardData.CardRole.Commander)
            return sameCardCount < MaxCommanderCopies;

        return sameCardCount < MaxCopiesPerCard;
    }

    public static bool CanAddSpell(CardData spell, List<CardData> mainDeck, List<CardData> selectedSpells)
    {
        if (spell == null || selectedSpells == null || mainDeck == null)
            return false;

        if (selectedSpells.Count >= MaxSpellSlots)
            return false;

        if (spell.cardRole != CardData.CardRole.Spell)
            return false;

        // Regra de cor: a spell só entra se o deck tiver pelo menos 1 carta daquela cor.
        bool hasMatchingColor = mainDeck.Any(c =>
            c != null &&
            c.cardRole != CardData.CardRole.Spell &&
            c.cardColorGroup == spell.cardColorGroup);

        return hasMatchingColor;
    }

    public static DeckAuraKind ResolveAuraKind(List<CardData> mainDeck)
    {
        bool hasRed = mainDeck.Any(c => c != null && c.cardRole != CardData.CardRole.Spell && c.cardColorGroup == CardData.CardType.Red);
        bool hasBlue = mainDeck.Any(c => c != null && c.cardRole != CardData.CardRole.Spell && c.cardColorGroup == CardData.CardType.Blue);

        if (hasRed && hasBlue) return DeckAuraKind.RedBlue;
        if (hasRed) return DeckAuraKind.PureRed;
        if (hasBlue) return DeckAuraKind.PureBlue;

        return DeckAuraKind.PureRed; // fallback de segurança
    }
}