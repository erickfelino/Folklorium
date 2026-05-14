using UnityEngine;
using Folklorium;

[CreateAssetMenu(fileName = "NewSummonEffect", menuName = "Card Effects/Summon")]
public class SummonEffect : CardEffect
{
    public override System.Type GetDataType()
    {
        return typeof(SummonEffectData);
    }

    public override bool IsValidTarget(IEffectSource source, CardCombat targetCard, PlayerHealth targetPlayer, EffectData rawData)
    {
        return true;
    }

    public override GameAction CreateAction(CardEffectContext context, EffectData rawData)
    {
        int atk = 1;
        int hp = 1;
        int qty = 1;
        int side = 0;

        if (rawData is SummonEffectData summonData)
        {
            atk = summonData.attack;
            hp = summonData.health;
            qty = summonData.Quantity > 0 ? summonData.Quantity : 1;
            side = summonData.boardSide;
        }
        else
        {
            Debug.LogWarning("O pacote de dados passado para SummonEffect não é um SummonEffectData!");
        }

        GameAction summonAction = new SummonAction(atk, hp, qty, side, context.IsEnemySource);

        if (rawData.timing != null &&
            rawData.timing.resolutionTiming == EffectResolutionTiming.Delayed)
        {
            return new DeferredEffectAction(
                context.source,
                rawData.timing.delayTurns,
                rawData.timing.resolutionScope,
                () =>
                {
                    if (ActionSystem.Instance != null)
                        ActionSystem.Instance.AddAction(summonAction);
                }
            );
        }

        return summonAction;
    }
}