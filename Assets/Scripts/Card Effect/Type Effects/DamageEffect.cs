using UnityEngine;
using Folklorium;

[CreateAssetMenu(menuName = "Card Effects/Damage Effect")]
public class DamageEffect : CardEffect
{
    public override System.Type GetDataType()
    {
        return typeof(DamageEffectData);
    }

    public override GameAction CreateAction(CardEffectContext context, EffectData rawData)
    {
        if (rawData is not DamageEffectData damageData)
        {
            Debug.LogError($"[DamageEffect] Dados inválidos para '{context.source.SourceName}'.");
            return null;
        }

        int damageToDeal = damageData.damage;

        if (context.source is SpellSlot)
        {
            damageToDeal += BattleAuraQuery.GetSpellDamageBonusForSide(context.IsEnemySource);
        }

        GameAction instantAction = new DamageAction(context.targetCard, context.targetPlayer, damageToDeal);

        if (rawData.timing != null && rawData.timing.resolutionTiming == EffectResolutionTiming.Delayed)
        {
            return new DeferredEffectAction(
                context.source,
                rawData.timing.delayTurns,
                rawData.timing.resolutionScope,
                () =>
                {
                    if (ActionSystem.Instance != null)
                        ActionSystem.Instance.AddAction(instantAction);
                }
            );
        }

        return instantAction;
    }
}