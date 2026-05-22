using System.Collections;
using UnityEngine;
using DG.Tweening;

public class DamageAction : GameAction
{
    private CardCombat targetCard;
    private PlayerHealth targetPlayer;
    private int finalDamage;

    // Construtor: Preenchemos o ticket com os dados brutos
    public DamageAction(CardCombat targetCard, PlayerHealth targetPlayer, int damage)
    {
        this.targetCard = targetCard;
        this.targetPlayer = targetPlayer;
        this.finalDamage = damage;
    }

    public override IEnumerator Perform()
    {
        if (targetCard != null)
        {
            if (!targetCard.IsTargetable)
                yield break;

            targetCard.ApplyRawStateChange(0, -finalDamage, false);
            yield break;
        }

        if (targetPlayer != null)
        {
            targetPlayer.ApplyRawStateChange(-finalDamage);
            yield break;
        }
    }
}