using UnityEngine;

[CreateAssetMenu(menuName = "Card Effects/Damage Effect")]
public class DamageEffect : CardEffect
{
    public int damageAmount;

    public override void Execute(CardEffectContext context)
    {
        // Se o alvo for uma carta na mesa
        if (context.targetCard != null)
        {
            context.targetCard.TakeDamage(damageAmount);
            Debug.Log($"{context.source.name} causou {damageAmount} de dano na carta {context.targetCard.name}!");
        }

        // Se o alvo for diretamente o jogador/torre
        if (context.targetPlayer != null)
        {
            context.targetPlayer.PlayerTakeDamage(damageAmount);
            Debug.Log($"{context.source.name} causou {damageAmount} de dano no jogador {context.targetPlayer.name}!");
        }
    }
}