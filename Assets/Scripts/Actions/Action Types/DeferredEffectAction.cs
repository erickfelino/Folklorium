using System;
using System.Collections;
using UnityEngine;
using Folklorium;

public class DeferredEffectAction : GameAction
{
    private readonly IEffectSource source;
    private readonly int delayTurns;
    private readonly EffectTurnScope scope;
    private readonly Action resolve;

    public DeferredEffectAction(IEffectSource source, int delayTurns, EffectTurnScope scope, Action resolve)
    {
        this.source = source;
        this.delayTurns = delayTurns;
        this.scope = scope;
        this.resolve = resolve;
    }

    public override IEnumerator Perform()
    {
        if (BattleEffectScheduler.Instance == null || source == null)
            yield break;

        BattleEffectScheduler.Instance.ScheduleDelayedAction(source, delayTurns, scope, resolve);
        yield break;
    }
}