using System.Collections;
using UnityEngine;

public class ModifyManaAction : GameAction
{
    private readonly ManaManager manaManager;
    private readonly int delta;

    public ModifyManaAction(ManaManager manaManager, int delta)
    {
        this.manaManager = manaManager;
        this.delta = delta;
    }

    public override IEnumerator Perform()
    {
        if (manaManager == null)
            yield break;

        manaManager.ModifyMana(delta);
        yield break;
    }
}