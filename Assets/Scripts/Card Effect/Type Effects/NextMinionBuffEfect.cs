using UnityEngine;
using Folklorium;

[CreateAssetMenu(menuName = "Card Effects/Next Minion Buff")]
public class NextMinionBuffEffect : CardEffect
{
    public override System.Type GetDataType()
    {
        return typeof(BuffEffectData);
    }

    public override GameAction CreateAction(CardEffectContext context, EffectData rawData)
    {
        if (rawData is not BuffEffectData buffData)
        {
            Debug.LogError("[NextMinionBuffEffect] Dados inválidos.");
            return null;
        }

        return new BoardLatchAction(
            context.source,
            card =>
            {
                CardDisplay display = card.GetComponent<CardDisplay>();
                if (display == null || display.cardData == null) return false;
                if (card.isEnemy != context.IsEnemySource) return false;
                if (display.cardData.cardRole == CardData.CardRole.Spell) return false;
                return true;
            },
            card =>
            {
                if (ActionSystem.Instance != null)
                    ActionSystem.Instance.AddAction(new BuffAction(card, buffData.attack, buffData.health));
            }
        );
    }
}