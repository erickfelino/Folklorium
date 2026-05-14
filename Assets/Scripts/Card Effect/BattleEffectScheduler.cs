using System;
using System.Collections.Generic;
using UnityEngine;
using Folklorium;

public class BattleEffectScheduler : MonoBehaviour
{
    public static BattleEffectScheduler Instance { get; private set; }

    [SerializeField] private TurnManager turnManager;
    [SerializeField] private BoardManager boardManager;

    private sealed class DelayedEntry
    {
        public IEffectSource source;
        public int remainingTurns;
        public EffectTurnScope scope;
        public Action resolve;
    }

    private sealed class BoardLatchEntry
    {
        public IEffectSource source;
        public Predicate<CardCombat> filter;
        public Action<CardCombat> resolve;
    }

    private readonly List<DelayedEntry> delayedEntries = new();
    private readonly List<BoardLatchEntry> boardLatchEntries = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (turnManager == null)
            turnManager = FindFirstObjectByType<TurnManager>();

        if (boardManager == null)
            boardManager = BoardManager.Instance != null ? BoardManager.Instance : FindFirstObjectByType<BoardManager>();
    }

    private void OnEnable()
    {
        if (turnManager != null)
            turnManager.OnTurnChanged += HandleTurnChanged;

        if (boardManager != null)
            boardManager.OnCardPlaced += HandleCardPlaced;
    }

    private void OnDisable()
    {
        if (turnManager != null)
            turnManager.OnTurnChanged -= HandleTurnChanged;

        if (boardManager != null)
            boardManager.OnCardPlaced -= HandleCardPlaced;
    }

    public void ScheduleDelayedAction(IEffectSource source, int delayTurns, EffectTurnScope scope, Action resolve)
    {
        if (source == null || resolve == null)
            return;

        if (delayTurns <= 0)
        {
            resolve.Invoke();
            return;
        }

        delayedEntries.Add(new DelayedEntry
        {
            source = source,
            remainingTurns = delayTurns,
            scope = scope,
            resolve = resolve
        });
    }

    public void QueueNextBoardEnter(IEffectSource source, Predicate<CardCombat> filter, Action<CardCombat> resolve)
    {
        if (source == null || resolve == null)
            return;

        boardLatchEntries.Add(new BoardLatchEntry
        {
            source = source,
            filter = filter,
            resolve = resolve
        });
    }

    private void HandleTurnChanged(bool isPlayerTurn)
    {
        bool currentSideIsEnemy = !isPlayerTurn;

        for (int i = delayedEntries.Count - 1; i >= 0; i--)
        {
            DelayedEntry entry = delayedEntries[i];
            if (entry.source == null)
            {
                delayedEntries.RemoveAt(i);
                continue;
            }

            bool countsThisTurn =
                entry.scope == EffectTurnScope.GlobalTurns ||
                entry.source.IsEnemy == currentSideIsEnemy;

            if (!countsThisTurn)
                continue;

            entry.remainingTurns--;

            if (entry.remainingTurns <= 0)
            {
                delayedEntries.RemoveAt(i);
                entry.resolve?.Invoke();
            }
        }
    }

    private void HandleCardPlaced(CardCombat card, BoardSlot slot, BoardEntryType entryType)
    {
        if (card == null)
            return;

        for (int i = boardLatchEntries.Count - 1; i >= 0; i--)
        {
            BoardLatchEntry entry = boardLatchEntries[i];

            if (entry.source == null)
            {
                boardLatchEntries.RemoveAt(i);
                continue;
            }

            if (entry.source.IsEnemy != card.isEnemy)
                continue;

            if (entry.filter != null && !entry.filter(card))
                continue;

            boardLatchEntries.RemoveAt(i);
            entry.resolve?.Invoke(card);
            break;
        }
    }
}