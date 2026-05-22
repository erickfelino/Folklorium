using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardDeathQueue : MonoBehaviour
{
    public static CardDeathQueue Instance { get; private set; }

    private readonly Queue<CardCombat> pendingDeaths = new();
    private readonly HashSet<CardCombat> queued = new();
    private Coroutine processingRoutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void Enqueue(CardCombat card)
    {
        if (card == null || queued.Contains(card))
            return;

        pendingDeaths.Enqueue(card);
        queued.Add(card);

        if (processingRoutine == null)
            processingRoutine = StartCoroutine(ProcessQueue());
    }

    private IEnumerator ProcessQueue()
    {
        while (pendingDeaths.Count > 0)
        {
            CardCombat card = pendingDeaths.Dequeue();
            queued.Remove(card);

            if (card == null)
                continue;

            while (ActionSystem.Instance != null && ActionSystem.Instance.IsGameBusy())
                yield return null;

            while (card != null && card.IsPerformingAttack)
                yield return null;

            if (card == null)
                continue;

            yield return card.PlayDeathSequenceVisual();
        }

        processingRoutine = null;
    }
}