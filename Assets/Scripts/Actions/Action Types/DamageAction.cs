using System.Collections;
using UnityEngine;
using DG.Tweening;

public class DamageAction : GameAction
{
    private CardCombat source;
    private CardCombat targetCard;
    private PlayerHealth targetPlayer;
    private int finalDamage;

    // Construtor: Preenchemos o ticket com os dados brutos
    public DamageAction(CardCombat source, CardCombat targetCard, PlayerHealth targetPlayer, int damage)
    {
        this.source = source;
        this.targetCard = targetCard;
        this.targetPlayer = targetPlayer;
        this.finalDamage = damage;
    }

    public override IEnumerator Perform()
    {
        // 1. Se o alvo for uma CARTA
        if (targetCard != null)
        {
            if (targetCard.isDead) yield break; // Ignora se já está morta

            // 👇 A CARTA APENAS OBEDECE A MATEMÁTICA BRUTA (Vamos criar essa função)
            targetCard.ApplyRawStateChange(0, -finalDamage); 
            
            // Feedback visual e tempo de espera (vamos tirar isso da carta)
            yield return targetCard.transform.DOShakePosition(0.3f, 0.2f, 10, 90f).WaitForCompletion();

            // Morte Lógica (se precisar)
            if (targetCard.currentLife <= 0 && !targetCard.isDead)
            {
                targetCard.Die(); // Chama sua rotina de morte
            }
        }
        // 2. Se o alvo for o JOGADOR
        else if (targetPlayer != null)
        {
            // O pipeline de dano no jogador
            targetPlayer.ApplyRawStateChange(-finalDamage); 
            yield return new WaitForSeconds(0.4f); // Tempo do feedback visual
        }
    }
}