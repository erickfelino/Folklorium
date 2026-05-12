using System.Collections.Generic;
using Folklorium;

public static class DeckAuraResolver
{
    public static DeckAuraKind ResolveFromDeck(IEnumerable<CardData> deckCards)
    {
        bool hasRed = false;
        bool hasBlue = false;

        foreach (var card in deckCards)
        {
            if (card == null) continue;

            if (card.cardColorGroup == CardData.CardType.Red)
                hasRed = true;

            if (card.cardColorGroup == CardData.CardType.Blue)
                hasBlue = true;
        }

        if (hasRed && hasBlue) return DeckAuraKind.RedBlue;
        if (hasRed) return DeckAuraKind.PureRed;
        if (hasBlue) return DeckAuraKind.PureBlue;

        return DeckAuraKind.PureRed;
    }
}