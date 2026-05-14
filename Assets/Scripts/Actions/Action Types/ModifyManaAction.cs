using System.Collections;
using UnityEngine;
using Folklorium;

public class ModifyManaAction : GameAction
{
    private readonly ManaManager manaManager;
    private readonly int delta;
    private readonly bool isTemporary;
    private readonly int durationTurns;
    private readonly EffectTurnScope scope;

    public ModifyManaAction(
        ManaManager manaManager,
        int delta,
        bool isTemporary = false,
        int durationTurns = 0,
        EffectTurnScope scope = EffectTurnScope.OwnerTurns)
    {
        this.manaManager = manaManager;
        this.delta = delta;
        this.isTemporary = isTemporary;
        this.durationTurns = durationTurns;
        this.scope = scope;
    }

    public override IEnumerator Perform()
    {
        if (manaManager == null)
            yield break;

        if (isTemporary)
        {
            manaManager.AddTemporaryManaModifier(delta, durationTurns, scope);
        }
        else
        {
            manaManager.ModifyMana(delta);
        }

        yield break;
    }
}