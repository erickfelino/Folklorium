using UnityEngine;

[CreateAssetMenu(menuName = "Card Effects/Heal Effect")]
public class HealEffect : CardEffect
{
    public override void Execute(CardEffectContext context)
    {
        int damage = context.source.GetComponent<CardDisplay>().cardData.damageValue;

        if (context.targetCard != null)
        {
            context.targetCard.TakeDamage(damage);
            return;
        }

        if (context.targetPlayer != null)
        {
            context.targetPlayer.PlayerTakeDamage(damage);
            return;
        }
    }
}