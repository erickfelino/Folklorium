using System.Collections;
using UnityEngine;
using Folklorium;

public class TimedCardModifierAction : GameAction
{
    private readonly CardCombat targetCard;
    private readonly int attackDelta;
    private readonly int lifeDelta;
    private readonly bool applyAsBuff;
    private readonly int durationTurns;
    private readonly EffectTurnScope scope;

    public TimedCardModifierAction(CardCombat targetCard, int attackDelta, int lifeDelta, bool applyAsBuff, int durationTurns, EffectTurnScope scope)
    {
        this.targetCard = targetCard;
        this.attackDelta = attackDelta;
        this.lifeDelta = lifeDelta;
        this.applyAsBuff = applyAsBuff;
        this.durationTurns = durationTurns;
        this.scope = scope;
    }

    public override IEnumerator Perform()
    {
        if (targetCard == null || targetCard.isDead)
            yield break;

        TimedCardModifier modifier = targetCard.GetComponent<TimedCardModifier>();
        if (modifier == null)
            modifier = targetCard.gameObject.AddComponent<TimedCardModifier>();

        modifier.Initialize(attackDelta, lifeDelta, applyAsBuff, durationTurns, scope);
        yield break;
    }
}