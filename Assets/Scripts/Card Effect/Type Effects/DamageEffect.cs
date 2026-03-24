using UnityEngine;

[CreateAssetMenu(menuName = "Card Effects/Damage Effect")]
public class DamageEffect : CardEffect
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