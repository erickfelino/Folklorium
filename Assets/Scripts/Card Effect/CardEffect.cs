using UnityEngine;

// 1. As categorias de alvos possíveis
public enum ValidTargetType
{
    EnemyCard,
    AllyCard,
    AnyCard,
    EnemyPlayer,
    AllyPlayer,
    AnyCharacter // Aceita tanto cartas quanto jogadores de qualquer lado
}

public abstract class CardEffect : ScriptableObject
{
    public EffectTriggerType trigger;

    [Header("Configuração de Alvo")]
    public bool requiresTarget = false;
    public ValidTargetType validTargets; // NOVO: O designer escolhe isso no Inspector!

    [TextArea]
    public string description;

    public abstract void Execute(CardEffectContext context);

    // 2. A MÁGICA (Problema 3 resolvido): O Efeito valida o alvo!
    public virtual bool IsValidTarget(CardCombat source, CardCombat targetCard, PlayerHealth targetPlayer)
    {
        if (!requiresTarget) return false;
        
        // Proteção extra: Se a carta fonte sumir/morrer antes de resolver, cancela o alvo.
        if (source == null) return false; 

        // 1. Se o alvo for uma CARTA na mesa
        if (targetCard != null)
        {
            bool isEnemyTarget = (targetCard.isEnemy != source.isEnemy); // Vê se é de times diferentes
            
            if (validTargets == ValidTargetType.EnemyCard && isEnemyTarget) return true;
            if (validTargets == ValidTargetType.AllyCard && !isEnemyTarget) return true;
            if (validTargets == ValidTargetType.AnyCard) return true;
            if (validTargets == ValidTargetType.AnyCharacter) return true;
        }

        // 2. Se o alvo for o JOGADOR (Avatar/Torre)
        if (targetPlayer != null)
        {
            bool targetIsAIHealth = targetPlayer.CompareTag("EnemyHealth"); 
            
            // 👇 A MÁGICA DA PERSPECTIVA AQUI 👇
            // Se quem lançou a magia for a IA (source.isEnemy), o inimigo é o Avatar que NÃO tem a tag da IA.
            // Se quem lançou for você (!source.isEnemy), o inimigo é quem tem a tag da IA.
            bool isEnemyPlayer = source.isEnemy ? !targetIsAIHealth : targetIsAIHealth;
            
            if (validTargets == ValidTargetType.EnemyPlayer && isEnemyPlayer) return true;
            if (validTargets == ValidTargetType.AllyPlayer && !isEnemyPlayer) return true;
            if (validTargets == ValidTargetType.AnyCharacter) return true;
        }

        return false; // Se chegou aqui, o alvo é inválido!
    }
}