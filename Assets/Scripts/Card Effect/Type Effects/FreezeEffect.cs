using UnityEngine;
using Folklorium;

[CreateAssetMenu(menuName = "Card Effects/Freeze Effect")]
public class FreezeEffect : CardEffect
{
    public override System.Type GetDataType()
    {
        return typeof(FreezeEffectData);
    }

    public override GameAction CreateAction(CardEffectContext context, EffectData rawData)
    {
        if (context.targetCard == null)
            return null;

        if (rawData is not FreezeEffectData freezeData)
        {
            Debug.LogError($"[FreezeEffect] Dados inválidos para '{context.source.SourceName}'.");
            return null;
        }

        int turns = freezeData.freezeTurns > 0
            ? freezeData.freezeTurns
            : 1;

        return new FreezeAction(
            context.targetCard,
            turns,
            rawData.timing != null ? rawData.timing.lifetimeScope : EffectTurnScope.OwnerTurns
        );
    }

    public override bool IsValidTarget(IEffectSource source, CardCombat targetCard, PlayerHealth targetPlayer, EffectData rawData)
    {
        if (targetCard == null)
            return false;

        return targetCard.isEnemy != source.IsEnemy;
    }
}