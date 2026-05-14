using UnityEngine;
using Folklorium;

[CreateAssetMenu(menuName = "Card Effects/Mana Effect")]
public class ManaEffect : CardEffect
{
    public override System.Type GetDataType()
    {
        return typeof(ManaEffectData);
    }

    public override GameAction CreateAction(CardEffectContext context, EffectData rawData)
    {
        if (rawData is not ManaEffectData manaData)
        {
            Debug.LogError($"[ManaEffect] Dados inválidos para '{context.source.SourceName}'.");
            return null;
        }

        ManaManager manaManager = ManaManager.GetForSide(context.IsEnemySource);
        if (manaManager == null)
        {
            Debug.LogWarning("[ManaEffect] ManaManager do lado correto não encontrado.");
            return null;
        }

        bool isTemporary = rawData.timing != null &&
                           rawData.timing.lifetimeMode == EffectLifetimeMode.Temporary;

        GameAction manaAction = new ModifyManaAction(
            manaManager,
            manaData.manaDelta,
            isTemporary,
            rawData.timing != null ? rawData.timing.durationTurns + rawData.timing.delayTurns: 0,
            rawData.timing != null ? rawData.timing.lifetimeScope : EffectTurnScope.OwnerTurns
        );

        if (rawData.timing != null && rawData.timing.resolutionTiming == EffectResolutionTiming.Delayed)
        {
            return new DeferredEffectAction(
                context.source,
                rawData.timing.delayTurns,
                rawData.timing.resolutionScope,
                () =>
                {
                    if (ActionSystem.Instance != null)
                        ActionSystem.Instance.AddAction(manaAction);
                }
            );
        }

        return manaAction;
    }
}