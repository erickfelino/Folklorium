using UnityEngine;
using Folklorium;

[CreateAssetMenu(fileName = "NewSummonAuraEffect", menuName = "Card Effects/Summon Aura")]
public class SummonAuraEffect : CardEffect
{
    public override System.Type GetDataType()
    {
        return typeof(SummonAuraEffectData);
    }

    public override bool IsValidTarget(CardCombat source, CardCombat targetCard, PlayerHealth targetPlayer, EffectData rawData)
    {
        return false;
    }

    public override GameAction CreateAction(CardEffectContext context, EffectData rawData)
    {
        int atk = 1;
        int hp = 1;
        bool ownSideOnly = true;

        if (rawData is SummonAuraEffectData auraData)
        {
            atk = auraData.attackBonus;
            hp = auraData.healthBonus;
            ownSideOnly = auraData.affectOnlyOwnSide;
        }

        return new SummonAuraAction(context.source, atk, hp, ownSideOnly);
    }
}