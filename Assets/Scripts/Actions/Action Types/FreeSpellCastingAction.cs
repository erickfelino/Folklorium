using System.Collections;
using UnityEngine;
using Folklorium;

public class FreeSpellCastAction : GameAction
{
    private readonly IEffectSource source;

    public FreeSpellCastAction(IEffectSource source)
    {
        this.source = source;
    }

    public override IEnumerator Perform()
    {
        SpellManager manager = SpellManager.GetForSide(source.IsEnemy);
        if (manager != null)
            manager.GrantFreeCast(1);

        yield break;
    }
}