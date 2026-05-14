using UnityEngine;
using Folklorium;

[CreateAssetMenu(menuName = "Card Effects/Free Spell Cast")]
public class FreeSpellCastEffect : CardEffect
{
    public override System.Type GetDataType()
    {
        return typeof(FreeSpellCastEffectData);
    }

    public override GameAction CreateAction(CardEffectContext context, EffectData rawData)
    {
        return new FreeSpellCastAction(context.source);
    }
}