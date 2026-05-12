using System.Collections;
using UnityEngine;
using Folklorium;

public class SummonAuraAction : GameAction
{
    private readonly CardCombat source;
    private readonly int attackBonus;
    private readonly int healthBonus;
    private readonly bool ownSideOnly;

    public SummonAuraAction(CardCombat source, int attackBonus, int healthBonus, bool ownSideOnly)
    {
        this.source = source;
        this.attackBonus = attackBonus;
        this.healthBonus = healthBonus;
        this.ownSideOnly = ownSideOnly;
    }

    public override IEnumerator Perform()
    {
        if (source == null || source.isDead)
            yield break;

        BoardManager boardManager = BoardManager.Instance != null ? BoardManager.Instance : Object.FindFirstObjectByType<BoardManager>();
        if (boardManager == null)
        {
            Debug.LogWarning("[RegisterSummonAuraAction] BoardManager não encontrado.");
            yield break;
        }

        if (boardManager.CardPlacedChannel == null)
        {
            Debug.LogWarning("[RegisterSummonAuraAction] BoardCardPlacedEventChannelSO não foi configurado no BoardManager.");
            yield break;
        }

        BoardSummonAuraListener listener = source.GetComponent<BoardSummonAuraListener>();
        if (listener == null)
        {
            listener = source.gameObject.AddComponent<BoardSummonAuraListener>();
        }

        listener.Initialize(boardManager.CardPlacedChannel, attackBonus, healthBonus, ownSideOnly);

        yield return null;
    }
}