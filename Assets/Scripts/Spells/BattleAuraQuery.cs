using System.Collections.Generic;
using UnityEngine;
using Folklorium;

public static class BattleAuraQuery
{
    public static int GetSpellDamageBonusForSide(bool enemySide)
    {
        BoardManager board = BoardManager.Instance != null ? BoardManager.Instance : Object.FindFirstObjectByType<BoardManager>();
        if (board == null)
            return 0;

        int bonus = 0;

        foreach (CardCombat card in board.GetAllCardsOnBoard())
        {
            if (card == null || card.isDead || card.isEnemy != enemySide)
                continue;

            var providers = card.GetComponents<ISpellDamageAuraProvider>();
            foreach (var provider in providers)
                bonus += provider.SpellDamageBonus;
        }

        return bonus;
    }
}