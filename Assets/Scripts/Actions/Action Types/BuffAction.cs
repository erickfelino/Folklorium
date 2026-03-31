using System.Collections;
using UnityEngine;
using DG.Tweening; // Mantendo o DOTween para caso você queira um efeitinho visual

public class BuffAction : GameAction
{
    private CardCombat source;
    private CardCombat targetCard;
    private int buffAttack;
    private int buffHealth;

    // Construtor do ticket
    public BuffAction(CardCombat source, CardCombat targetCard, int attack, int health)
    {
        this.source = source;
        this.targetCard = targetCard;
        this.buffAttack = attack;
        this.buffHealth = health;
    }

    public override IEnumerator Perform()
    {
        // 1. O Buff só funciona em Cartas (Lacaios)
        if (targetCard != null)
        {
            // Ignora se o lacaio morreu antes do buff resolver na fila
            if (targetCard.isDead) yield break; 

            // 👇 APLICANDO A MATEMÁTICA BRUTA 👇
            // Como você já tem a função 'ApplyRawStateChange' no CardCombat, 
            // vamos usá-la! Apenas passamos os valores positivos.
            targetCard.ApplyRawStateChange(buffAttack, buffHealth, true); // TRUE! É um buff!

            // Opcional: Efeito visual do Buff (um pulinho ou um brilho)
            // yield return targetCard.transform.DOPunchScale(Vector3.one * 0.2f, 0.3f, 10, 1).WaitForCompletion();
            
            // Pausa rápida para o jogador processar o que aconteceu (se não tiver animação)
            yield return new WaitForSeconds(0.2f); 
            
            Debug.Log($"[BuffAction] {targetCard.name} recebeu +{buffAttack}/+{buffHealth}!");
        }
        else
        {
            Debug.LogWarning("[BuffAction] Tentativa de buffar um alvo nulo ou não-carta.");
        }
    }
}