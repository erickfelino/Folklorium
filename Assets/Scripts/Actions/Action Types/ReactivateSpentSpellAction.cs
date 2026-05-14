using System.Collections;
using UnityEngine;
using Folklorium;

public class ReactivateSpentSpellAction : GameAction
{
    private readonly IEffectSource source;

    public ReactivateSpentSpellAction(IEffectSource source)
    {
        this.source = source;
    }

    public override IEnumerator Perform()
    {
        SpellManager manager = SpellManager.GetForSide(source.IsEnemy);
        if (manager == null)
            yield break;

        yield return manager.BeginReactivateSelection(source);
    }
}