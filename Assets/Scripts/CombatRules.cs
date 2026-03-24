using UnityEngine;
using Folklorium;
using static Folklorium.Card;

public static class CombatRules
{
    // =========================================================
    // REGRA 1: Posso atacar o jogador diretamente?
    // =========================================================
    public static bool CanAttackPlayer(CardRole attackerRole, bool enemyHasSoldiers, bool enemyHasHeroes, bool enemyHasCommanders)
    {
        if (attackerRole == CardRole.Commander)
        {
            // Comandante passa por cima de tudo, exceto outro Comandante
            return !enemyHasCommanders; 
        }
        else if (attackerRole == CardRole.Hero)
        {
            // Herói ignora Soldados, mas é bloqueado por Heróis e Comandantes
            return !enemyHasHeroes && !enemyHasCommanders;
        }
        else if (attackerRole == CardRole.Soldier)
        {
            // Soldado precisa que a mesa inimiga esteja 100% limpa
            return !enemyHasSoldiers && !enemyHasHeroes && !enemyHasCommanders;
        }
        
        return false;
    }

    // =========================================================
    // REGRA 2: Posso atacar esta carta inimiga específica?
    // =========================================================
    public static bool CanAttackCard(CardRole attackerRole, CardRole targetRole, bool enemyHasSoldiers, bool enemyHasHeroes, bool enemyHasCommanders)
    {
        if (attackerRole == CardRole.Soldier)
        {
            if (enemyHasSoldiers) return targetRole == CardRole.Soldier;
            if (enemyHasHeroes) return targetRole == CardRole.Hero;
            if (enemyHasCommanders) return targetRole == CardRole.Commander;
        }
        else if (attackerRole == CardRole.Hero)
        {
            // Heróis SEMPRE podem atacar Soldados ou Heróis
            if (targetRole == CardRole.Soldier || targetRole == CardRole.Hero) return true;
            
            // Heróis só atacam Comandantes se não houver Heróis inimigos
            if (targetRole == CardRole.Commander) return !enemyHasHeroes;
        }
        else if (attackerRole == CardRole.Commander)
        {
            // Comandantes batem em qualquer carta sempre
            return true; 
        }

        return false;
    }
}