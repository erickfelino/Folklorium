using System.Collections;
using UnityEngine;
using Folklorium;

public class TimedCardStatusAction : GameAction
{
    private readonly CardCombat targetCard;
    private readonly int attackDelta;
    private readonly int lifeDelta;
    private readonly bool applyAsBuff;
    private readonly int attackLockDelta;
    private readonly int durationTurns;
    private readonly EffectTurnScope scope;

    public TimedCardStatusAction(
        CardCombat targetCard,
        int attackDelta,
        int lifeDelta,
        bool applyAsBuff,
        int attackLockDelta,
        int durationTurns,
        EffectTurnScope scope)
    {
        this.targetCard = targetCard;
        this.attackDelta = attackDelta;
        this.lifeDelta = lifeDelta;
        this.applyAsBuff = applyAsBuff;
        this.attackLockDelta = attackLockDelta;
        this.durationTurns = durationTurns;
        this.scope = scope;
    }

    public override IEnumerator Perform()
    {
        if (targetCard == null || targetCard.isDead)
            yield break;

        TimedCardStatus status = targetCard.GetComponent<TimedCardStatus>();
        if (status == null)
            status = targetCard.gameObject.AddComponent<TimedCardStatus>();

        status.Initialize(attackDelta, lifeDelta, applyAsBuff, attackLockDelta, durationTurns, scope);
        yield break;
    }
}