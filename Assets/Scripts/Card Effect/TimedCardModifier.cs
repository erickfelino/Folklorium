using UnityEngine;
using Folklorium;

public class TimedCardModifier : MonoBehaviour
{
    private CardCombat target;
    private TurnManager turnManager;

    private int attackDelta;
    private int lifeDelta;
    private bool applyAsBuff;
    private int remainingTurns;
    private EffectTurnScope scope;
    private bool initialized;

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

    public void Initialize(int attackDelta, int lifeDelta, bool applyAsBuff, int durationTurns, EffectTurnScope scope)
    {
        if (initialized || target == null || target.isDead)
            return;

        this.attackDelta = attackDelta;
        this.lifeDelta = lifeDelta;
        this.applyAsBuff = applyAsBuff;
        this.remainingTurns = Mathf.Max(1, durationTurns);
        this.scope = scope;
        initialized = true;

        target.ApplyRawStateChange(attackDelta, lifeDelta, applyAsBuff);
    }

    private void HandleTurnChanged(bool isPlayerTurn)
    {
        if (!initialized || target == null || target.isDead)
            return;

        bool currentSideIsEnemy = !isPlayerTurn;
        bool countsThisTurn =
            scope == EffectTurnScope.GlobalTurns ||
            target.isEnemy == currentSideIsEnemy;

        if (!countsThisTurn)
            return;

        remainingTurns--;

        if (remainingTurns > 0)
            return;

        target.ApplyRawStateChange(-attackDelta, -lifeDelta, applyAsBuff);
        Destroy(this);
    }
}