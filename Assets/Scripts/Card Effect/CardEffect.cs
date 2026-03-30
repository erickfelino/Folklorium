using UnityEngine;
using Folklorium; // 👇 Avisamos para ele olhar o nosso namespace!

// 1. As categorias de alvos possíveis
public enum ValidTargetType
{
    EnemyCard,
    AllEnemySoldiers,
    AllEnemyCards,
    AllyCard,
    AllAllyCards,
    AnyCard,
    EnemyTower,
    AllyTower,
    AnyTower,
    AnyCharacter // Aceita tanto cartas quanto jogadores de qualquer lado
}

public abstract class CardEffect : ScriptableObject
{
    // 👇 APAGAMOS O 'trigger' DAQUI! Agora quem manda no gatilho é o CardData lá no Inspector.

    [Header("Configuração de Alvo")]
    public bool requiresTarget = false;
    public ValidTargetType validTargets; // NOVO: O designer escolhe isso no Inspector!

    [TextArea]
    public string description;
    public abstract System.Type GetDataType();

    // A assinatura nova exige o EffectData!
    public abstract GameAction CreateAction(CardEffectContext context, EffectData rawData);

    // 2. A MÁGICA: O Efeito valida o alvo!
    // 👇 Adicionamos o 'EffectData rawData' no final
    public virtual bool IsValidTarget(CardCombat source, CardCombat targetCard, PlayerHealth targetPlayer, EffectData rawData)
    {
        if (!requiresTarget) return false;
        if (source == null) return false; 

        if (targetCard != null)
        {
            // 👇 A MÁGICA: Perguntamos pro pacote de dados se ele exclui a si mesmo!
            if (rawData != null && rawData.GetExcludeSelf() && targetCard == source) 
                return false;

            bool isEnemyTarget = (targetCard.isEnemy != source.isEnemy); 
            
            if (validTargets == ValidTargetType.EnemyCard && isEnemyTarget) return true;
            if (validTargets == ValidTargetType.AllyCard && !isEnemyTarget) return true;
            if (validTargets == ValidTargetType.AnyCard) return true;
            if (validTargets == ValidTargetType.AnyCharacter) return true;
            if (validTargets == ValidTargetType.AllEnemyCards) return true;
        }

        // ... resto da função do targetPlayer continua igual
        if (targetPlayer != null)
        {
            bool targetIsAIHealth = targetPlayer.CompareTag("EnemyHealth"); 
            bool isEnemyPlayer = source.isEnemy ? !targetIsAIHealth : targetIsAIHealth;
            
            if (validTargets == ValidTargetType.EnemyTower && isEnemyPlayer) return true;
            if (validTargets == ValidTargetType.AllyTower && !isEnemyPlayer) return true;
            if (validTargets == ValidTargetType.AnyCharacter) return true;
        }

        return false;
    }
}