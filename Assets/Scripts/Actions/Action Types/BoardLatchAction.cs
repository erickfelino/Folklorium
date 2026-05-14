using System;
using System.Collections;
using UnityEngine;
using Folklorium;

public class BoardLatchAction : GameAction
{
    private readonly IEffectSource source;
    private readonly Predicate<CardCombat> filter;
    private readonly Action<CardCombat> resolve;

    public BoardLatchAction(IEffectSource source, Predicate<CardCombat> filter, Action<CardCombat> resolve)
    {
        this.source = source;
        this.filter = filter;
        this.resolve = resolve;
    }

    public override IEnumerator Perform()
    {
        if (BattleEffectScheduler.Instance == null || source == null)
            yield break;

        BattleEffectScheduler.Instance.QueueNextBoardEnter(source, filter, resolve);
        yield break;
    }
}