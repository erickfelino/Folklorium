using System.Collections;
using UnityEngine;
using Folklorium;

public class FreezeAction : GameAction
{
    private readonly CardCombat targetCard;
    private readonly int freezeTurns;
    private readonly EffectTurnScope scope;

    public FreezeAction(CardCombat targetCard, int freezeTurns, EffectTurnScope scope)
    {
        this.targetCard = targetCard;
        this.freezeTurns = freezeTurns;
        this.scope = scope;
    }

    public override IEnumerator Perform()
    {
        if (targetCard == null || targetCard.isDead)
            yield break;

        TimedCardStatus status = targetCard.GetComponent<TimedCardStatus>();
        if (status == null)
            status = targetCard.gameObject.AddComponent<TimedCardStatus>();

        status.AddStack(
            0,
            0,
            false,
            1,
            freezeTurns,
            scope
        );

        yield break;
    }
}