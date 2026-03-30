using UnityEngine;
using Folklorium;

[CreateAssetMenu(fileName = "NewHealEffect", menuName = "Card Effects/Destroy")]
public class DestroyEffect : CardEffect
{

    public override System.Type GetDataType()
    {
        return typeof(DestroyEffectData);
    }

    public override bool IsValidTarget(CardCombat source, CardCombat targetCard, PlayerHealth targetPlayer, EffectData rawData)
    {
        // 1. Deixa a classe pai checar se o alvo bate com o que você configurou no Inspector
        // (ex: Se no Inspector está "AllyCard", a classe pai já barra se tentar curar inimigo)
        if (!base.IsValidTarget(source, targetCard, targetPlayer, rawData)) 
        {
            return false; 
        }

        // 2. A SUA DICA DE MESTRE APLICADA! 😎
        // A classe pai deixou passar, mas nós adicionamos uma regra extra: não curar quem já está cheio!
        if (targetCard != null)
        {
            // Checa se a carta já está com a vida cheia (adapte maxLife se a sua variável chamar diferente)
            // if (targetCard.currentLife >= targetCard.maxLife) return false; 
        }

        if (targetPlayer != null)
        {
            // Checa se o jogador já está com a vida cheia
            // if (targetPlayer.currentHealth >= targetPlayer.maxHealth) return false;
        }

        return true; // Se passou pelo pai e pela regra de vida cheia, é um alvo válido!
    }

    public override GameAction CreateAction(CardEffectContext context, EffectData rawData)
    {
        int numberOfTargets = 0;

        // 👇 A MÁGICA DO CASTING: Abre o pacote e pega a cura!
        if (rawData is DestroyEffectData destroyData)
        {
            numberOfTargets = destroyData.quantityTargets;
        }
        else
        {
            Debug.LogWarning("O pacote passado para DestroyEffect não é um DestroyEffectData!");
        }

        // Imprime o "Ticket" de cura!
        return new DestroyAction(context.targetCard, numberOfTargets);
    }
}