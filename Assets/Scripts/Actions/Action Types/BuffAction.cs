using System.Collections;
using UnityEngine;
using DG.Tweening; // Mantendo o DOTween para caso você queira um efeitinho visual

public class BuffAction : GameAction
{
    private readonly CardCombat targetCard;
    private readonly int buffAttack;
    private readonly int buffHealth;

    public BuffAction(CardCombat targetCard, int attack, int health)
    {
        this.targetCard = targetCard;
        this.buffAttack = attack;
        this.buffHealth = health;
    }

    public override IEnumerator Perform()
    {
        if (targetCard == null)
        {
            Debug.LogWarning("[BuffAction] Tentativa de buffar um alvo nulo.");
            yield break;
        }

        if (targetCard.isDead)
            yield break;

        targetCard.ApplyRawStateChange(buffAttack, buffHealth, true);
        yield return new WaitForSeconds(0.2f);
    }
}