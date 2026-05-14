using UnityEngine;
using Folklorium;

[CreateAssetMenu(menuName = "Card Effects/Reactivate Spent Spell")]
public class ReactivateSpentSpellEffect : CardEffect
{
    public override System.Type GetDataType()
    {
        return typeof(ReactivateSpentSpellEffectData);
    }

    public override GameAction CreateAction(CardEffectContext context, EffectData rawData)
    {
        return new ReactivateSpentSpellAction(context.source);
    }
}