using UnityEngine;
using Folklorium;

[CreateAssetMenu(fileName = "NewAoEEffect", menuName = "Card Effects/AoE")]
public class AoEEffect : CardEffect
{
    public override System.Type GetDataType()
    {
        return typeof(AoEEffectData);
    }

    public override bool IsValidTarget(CardCombat source, CardCombat targetCard, PlayerHealth targetPlayer, EffectData rawData)
    {
        return false;
    }

    public override GameAction CreateAction(CardEffectContext context, EffectData rawData)
    {
        if (rawData is not AoEEffectData aoeData)
        {
            Debug.LogWarning("[AoEEffect] rawData não é AoEEffectData.");
            return null;
        }

        return new AoEEffectAction(context.source, aoeData);
    }
}