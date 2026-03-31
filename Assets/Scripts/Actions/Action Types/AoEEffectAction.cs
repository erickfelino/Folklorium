using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Folklorium;

public class AoEEffectAction : GameAction
{
    private readonly CardCombat source;
    private readonly AoEEffectData data;

    public AoEEffectAction(CardCombat source, AoEEffectData data)
    {
        this.source = source;
        this.data = data;
    }

    public override IEnumerator Perform()
    {
        BoardManager boardManager = BoardManager.Instance != null
            ? BoardManager.Instance
            : Object.FindFirstObjectByType<BoardManager>();

        if (boardManager == null || data == null)
            yield break;

        List<CardCombat> allCards = boardManager.GetAllCardsOnBoard();

        foreach (CardCombat card in allCards)
        {
            if (card == null || card.isDead)
                continue;

            if (data.GetExcludeSelf() && card == source)
                continue;

            if (!MatchesAnySelectedGroup(card, source, data.targetGroups))
                continue;

            switch (data.mode)
            {
                case AoEEffectData.AoEMode.Damage:
                    card.ApplyRawStateChange(0, -Mathf.Abs(data.damageAmount), false);
                    break;

                case AoEEffectData.AoEMode.Heal:
                    card.ApplyRawStateChange(0, Mathf.Abs(data.healAmount), false);
                    break;

                case AoEEffectData.AoEMode.Buff:
                    card.ApplyRawStateChange(data.buffAttack, data.buffHealth, true);
                    break;

                case AoEEffectData.AoEMode.Destroy:
                    card.Die();
                    break;
            }

            yield return new WaitForSeconds(0.05f);
        }
    }

    private bool MatchesAnySelectedGroup(CardCombat card, CardCombat source, List<AoETargetType> groups)
    {
        if (card == null || source == null || groups == null || groups.Count == 0)
            return false;

        CardDisplay display = card.GetComponent<CardDisplay>();
        if (display == null || display.cardData == null)
            return false;

        bool targetIsEnemyFromSource = card.isEnemy != source.isEnemy;
        CardData.CardRole role = display.cardData.cardRole;

        foreach (AoETargetType group in groups)
        {
            if (group == AoETargetType.EnemySoldiers && targetIsEnemyFromSource && role == CardData.CardRole.Soldier) return true;
            if (group == AoETargetType.EnemyHeroes && targetIsEnemyFromSource && role == CardData.CardRole.Hero) return true;
            if (group == AoETargetType.EnemyCommanders && targetIsEnemyFromSource && role == CardData.CardRole.Commander) return true;

            if (group == AoETargetType.AllySoldiers && !targetIsEnemyFromSource && role == CardData.CardRole.Soldier) return true;
            if (group == AoETargetType.AllyHeroes && !targetIsEnemyFromSource && role == CardData.CardRole.Hero) return true;
            if (group == AoETargetType.AllyCommanders && !targetIsEnemyFromSource && role == CardData.CardRole.Commander) return true;
        }

        return false;
    }
}