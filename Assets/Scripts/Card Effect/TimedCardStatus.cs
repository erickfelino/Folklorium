using System.Collections.Generic;
using UnityEngine;
using Folklorium;

public class TimedCardStatus : MonoBehaviour
{
    private class StatusStack
    {
        public int attackDelta;
        public int lifeDelta;
        public bool applyAsBuff;
        public int attackLockDelta;
        public int remainingTurns;
        public EffectTurnScope scope;
    }

    private CardCombat target;
    private TurnManager turnManager;
    private readonly List<StatusStack> stacks = new List<StatusStack>();

    private void Awake()
    {
        target = GetComponent<CardCombat>();
        turnManager = FindFirstObjectByType<TurnManager>();
    }

    private void OnEnable()
    {
        if (turnManager != null)
            turnManager.OnTurnChanged += HandleTurnChanged;
    }

    private void OnDisable()
    {
        if (turnManager != null)
            turnManager.OnTurnChanged -= HandleTurnChanged;
    }

    public void AddStack(
        int attackDelta,
        int lifeDelta,
        bool applyAsBuff,
        int attackLockDelta,
        int durationTurns,
        EffectTurnScope scope)
    {
        if (target == null || target.isDead)
            return;

        var stack = new StatusStack
        {
            attackDelta = attackDelta,
            lifeDelta = lifeDelta,
            applyAsBuff = applyAsBuff,
            attackLockDelta = attackLockDelta,
            remainingTurns = Mathf.Max(1, durationTurns),
            scope = scope
        };

        stacks.Add(stack);

        if (attackDelta != 0 || lifeDelta != 0)
            target.ApplyRawStateChange(attackDelta, lifeDelta, applyAsBuff);

        if (attackLockDelta > 0)
            target.AddTemporaryAttackLock(attackLockDelta);
    }

    private void HandleTurnChanged(bool isPlayerTurn)
    {
        if (target == null || target.isDead)
        {
            Destroy(this);
            return;
        }

        bool sideThatJustEndedIsEnemy = isPlayerTurn;

        for (int i = stacks.Count - 1; i >= 0; i--)
        {
            StatusStack stack = stacks[i];

            bool countsThisTurn =
                stack.scope == EffectTurnScope.GlobalTurns ||
                target.isEnemy == sideThatJustEndedIsEnemy;

            if (!countsThisTurn)
                continue;

            stack.remainingTurns--;

            if (stack.remainingTurns >= 0)
                continue;

            if (stack.attackDelta != 0 || stack.lifeDelta != 0)
                target.ApplyRawStateChange(-stack.attackDelta, -stack.lifeDelta, stack.applyAsBuff);

            if (stack.attackLockDelta > 0)
                target.RemoveTemporaryAttackLock(stack.attackLockDelta);

            stacks.RemoveAt(i);
        }

        if (stacks.Count == 0)
            Destroy(this);
    }
}