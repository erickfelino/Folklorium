using System.Collections;
using UnityEngine;
using Folklorium;

public class AuraBuffAction : GameAction
{
    private readonly CardCombat targetCard;
    private readonly int attackBonus;
    private readonly int healthBonus;

    public AuraBuffAction(CardCombat targetCard, int attackBonus, int healthBonus)
    {
        this.targetCard = targetCard;
        this.attackBonus = attackBonus;
        this.healthBonus = healthBonus;
    }

    public override IEnumerator Perform()
    {
        if (targetCard == null || targetCard.isDead)
            yield break;

        targetCard.ApplyRawStateChange(attackBonus, healthBonus, true);
        yield return null;
    }
}