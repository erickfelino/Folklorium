using System.Collections;
using UnityEngine;
using Folklorium;

public class HealAction : GameAction
{
    private CardCombat targetCard;
    private PlayerHealth targetPlayer;
    private int healAmount;

    public HealAction(CardCombat targetCard, PlayerHealth targetPlayer, int healAmount)
    {
        this.targetCard = targetCard;
        this.targetPlayer = targetPlayer;
        this.healAmount = healAmount;
    }

    public override IEnumerator Perform()
    {
        bool healedSomeone = false;

        // 1. Tenta curar uma carta na mesa
        if (targetCard != null && targetCard.currentLife > 0)
        {
            Debug.Log($"[Ação] Curando {targetCard.name} em {healAmount} de vida.");
            
            // 👇 USANDO A API CORRETA! (0 de mudança no ataque, +healAmount na vida)
            targetCard.ApplyRawStateChange(0, healAmount); 
            
            healedSomeone = true;
        }
        else if (targetPlayer != null)
        {
            Debug.Log($"[Ação] Curando Jogador/Torre em {healAmount} de vida.");
            
            // Passamos o valor positivo da cura!
            targetPlayer.ApplyRawStateChange(healAmount); 
            
            healedSomeone = true;
        }

        // Se curou alguém, dá um tempinho (Juice!)
        if (healedSomeone)
        {
            yield return new WaitForSeconds(0.5f);
        }
        else
        {
            Debug.LogWarning("[Ação] Cura falhou: Alvo nulo ou já destruído.");
            yield break;
        }
    }
}