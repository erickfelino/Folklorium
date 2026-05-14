using UnityEngine;

[CreateAssetMenu(menuName = "Card Effects/Buff Effect")]
public class BuffEffect : CardEffect
{
    public override System.Type GetDataType()
    {
        return typeof(BuffEffectData);
    }

    public override GameAction CreateAction(CardEffectContext context, EffectData rawData)
    {
        if (rawData is not BuffEffectData buffData)
        {
            Debug.LogError($"[BuffEffect] Dados inválidos para '{context.source.SourceName}'.");
            return null;
        }

        if (context.targetCard == null)
            return null;

        if (rawData.timing != null && rawData.timing.lifetimeMode == EffectLifetimeMode.Temporary)
        {
            return new TimedCardStatusAction(
                context.targetCard,
                buffData.attack,
                buffData.health,
                true,
                0,
                rawData.timing.durationTurns,
                rawData.timing.lifetimeScope
            );
        }

        return new BuffAction(context.targetCard, buffData.attack, buffData.health);
    }
}