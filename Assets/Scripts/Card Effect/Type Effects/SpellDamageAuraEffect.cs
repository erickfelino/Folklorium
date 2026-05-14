using System;
using UnityEngine;
using Folklorium;


[CreateAssetMenu(menuName = "Card Effects/Spell Damage Aura")]
public class SpellDamageAuraEffect : CardEffect
{
    public override Type GetDataType()
    {
        return typeof(SpellDamageAuraEffectData);
    }

    public override bool IsValidTarget(IEffectSource source, CardCombat targetCard, PlayerHealth targetPlayer, EffectData rawData)
    {
        return false;
    }

    public override GameAction CreateAction(CardEffectContext context, EffectData rawData)
    {
        if (context.source is not CardCombat sourceCard)
        {
            Debug.LogWarning("[SpellDamageAuraEffect] A source precisa ser uma CardCombat.");
            return null;
        }

        int bonusDamage = 4;

        if (rawData is SpellDamageAuraEffectData auraData)
        {
            bonusDamage = auraData.bonusDamage;
        }

        return new AttachSpellDamageAuraAction(sourceCard, bonusDamage);
    }
}