using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq; 
using Folklorium;

public class OpponentAI : MonoBehaviour
{
    [Header("Gerenciadores do Oponente")]
    public HandManager aiHand;
    public ManaManager aiMana;
    
    public IEnumerator ProcessTurn()
    {
        Debug.Log("IA: Começando o meu turno...");
        yield return new WaitForSeconds(1f);

        // A IA pensa como um jogador real: primeiro joga as cartas, depois ataca!
        yield return StartCoroutine(PlayCardsPhase());
        
        yield return StartCoroutine(AttackPhase());

        Debug.Log("IA: Fim das jogadas. Passo o turno.");
        yield return new WaitForSeconds(1f);
    }

    // ==========================================
    // FASE 1: DESCER CARTAS (Lógica de Curva de Mana)
    // ==========================================
    private IEnumerator PlayCardsPhase()
    {
        List<GameObject> cardsInHand = new List<GameObject>(aiHand.cardsInHand);
        cardsInHand.Sort((a, b) => b.GetComponent<CardDisplay>().cardData.mana.CompareTo(a.GetComponent<CardDisplay>().cardData.mana));

        foreach (GameObject cardObj in cardsInHand)
        {
            Card cardData = cardObj.GetComponent<CardDisplay>().cardData;
            
            if (cardData.mana <= aiMana.currentMana)
            {
                string targetTag = GetEnemyZoneTag(cardData.cardRole);
                GameObject emptyZone = FindEmptyZone(targetTag);

                if (emptyZone != null)
                {
                    Debug.Log($"IA decidiu invocar: {cardData.cardName}");
                    aiMana.SpendMana(cardData.mana);
                    aiHand.RemoveCardFromHand(cardObj);

                    CardDrag dragScript = cardObj.GetComponent<CardDrag>();
                    if (dragScript != null)
                    {
                        dragScript.TransformIntoTokenAndJump(emptyZone.transform, true);
                        
                        // Marca a carta como sendo do inimigo IMEDIATAMENTE!
                        CardCombat combatScript = cardObj.GetComponent<CardCombat>();
                        if (combatScript != null)
                        {
                            combatScript.isEnemy = true;
                        }
                    }

                    yield return new WaitForSeconds(1.2f); 
                }
            }
        }
    }

    // ==========================================
    // FASE 2: COMBATE COM INTELIGÊNCIA AVANÇADA
    // ==========================================
    private IEnumerator AttackPhase()
    {
        // 1. Acha as vidas na mesa
        PlayerHealth playerHealth = GameObject.FindGameObjectWithTag("PlayerHealth")?.GetComponent<PlayerHealth>();
        PlayerHealth aiHealth = GameObject.FindGameObjectWithTag("EnemyHealth")?.GetComponent<PlayerHealth>();

        // 2. Pega as cartas da IA que estão vivas e prontas para atacar
        CardCombat[] allCardsOnBoard = FindObjectsByType<CardCombat>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        List<CardCombat> aiCards = allCardsOnBoard.Where(c => c.isEnemy && c.canAttackThisTurn && c.currentLife > 0).ToList();

        foreach (CardCombat aiCard in aiCards)
        {
            if (aiCard == null || aiCard.currentLife <= 0) continue; 

            // -----------------------------------------------------
            // O RADAR: Lê a mesa do jogador a cada ataque (pois a mesa muda)
            // -----------------------------------------------------
            List<CardCombat> playerCards = FindObjectsByType<CardCombat>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
                .Where(c => !c.isEnemy && c.GetComponent<CardDrag>().isPlayed && c.currentLife > 0).ToList();

            bool pHasSoldiers = playerCards.Any(c => c.GetComponent<CardDisplay>().cardData.cardRole == Card.CardRole.Soldier);
            bool pHasHeroes = playerCards.Any(c => c.GetComponent<CardDisplay>().cardData.cardRole == Card.CardRole.Hero);
            bool pHasCommanders = playerCards.Any(c => c.GetComponent<CardDisplay>().cardData.cardRole == Card.CardRole.Commander);

            Card.CardRole myRole = aiCard.GetComponent<CardDisplay>().cardData.cardRole;

            // -----------------------------------------------------
            // O JUIZ: Usa o CombatRules.cs para saber quem a IA pode atacar
            // -----------------------------------------------------
            bool canAttackFace = CombatRules.CanAttackPlayer(myRole, pHasSoldiers, pHasHeroes, pHasCommanders);

            List<CardCombat> validTargets = playerCards.Where(pCard =>
            {
                Card.CardRole targetRole = pCard.GetComponent<CardDisplay>().cardData.cardRole;
                return CombatRules.CanAttackCard(myRole, targetRole, pHasSoldiers, pHasHeroes, pHasCommanders);
            }).ToList();

            // -----------------------------------------------------
            // CÁLCULO DE AMEAÇA (A IA sabe se vai ganhar ou perder?)
            // -----------------------------------------------------
            int myTotalFaceDamage = CalculatePotentialFaceDamage(aiCards, pHasSoldiers, pHasHeroes, pHasCommanders);
            int enemyTotalDamage = playerCards.Sum(c => c.currentAttack); 

            bool iHaveLethal = playerHealth != null && myTotalFaceDamage >= playerHealth.currentHealth;
            bool enemyHasLethal = aiHealth != null && enemyTotalDamage >= aiHealth.currentHealth;

            bool attacked = false;

            // =====================================================
            // ÁRVORE DE DECISÃO DA IA
            // =====================================================

            // MODO 1: LETAL (Ganhar o jogo)
            if (iHaveLethal && canAttackFace && playerHealth != null)
            {
                Debug.Log($"IA [LETAL]: {aiCard.name} ataca o Jogador para vencer!");
                aiCard.Attack(playerHealth);
                attacked = true;
            }
            // MODO 2: DEFESA (Sobreviver custe o que custar)
            else if (enemyHasLethal && validTargets.Count > 0)
            {
                CardCombat biggestThreat = validTargets.OrderByDescending(c => c.currentAttack).FirstOrDefault();
                Debug.Log($"IA [DEFESA]: {aiCard.name} ataca {biggestThreat.name} para não morrer!");
                aiCard.Attack(biggestThreat);
                attacked = true;
            }
            // MODO 3: PADRÃO / AGGRO / KAMIKAZE
            else
            {
                // Tenta achar a Troca Perfeita (Mata o inimigo e sobrevive)
                CardCombat bestTrade = validTargets
                    .Where(p => aiCard.currentAttack >= p.currentLife && aiCard.currentLife > p.currentAttack)
                    .OrderByDescending(p => p.currentAttack) 
                    .FirstOrDefault();

                if (bestTrade != null)
                {
                    Debug.Log($"IA [TROCA]: {aiCard.name} vai destruir {bestTrade.name} de forma segura.");
                    aiCard.Attack(bestTrade);
                    attacked = true;
                }
                // Se não tem troca boa, mas o caminho pro jogador está livre, BATE NA CARA!
                else if (canAttackFace && playerHealth != null)
                {
                    Debug.Log($"IA [AGGRO]: {aiCard.name} bate direto na vida do Jogador.");
                    aiCard.Attack(playerHealth);
                    attacked = true;
                }
                // Se o caminho tá bloqueado e não tem troca boa, se sacrifica na carta mais forte que puder bater
                else if (validTargets.Count > 0)
                {
                    CardCombat kamikazeTarget = validTargets.OrderByDescending(p => p.currentAttack).First();
                    Debug.Log($"IA [KAMIKAZE]: Caminho bloqueado. {aiCard.name} ataca {kamikazeTarget.name}.");
                    aiCard.Attack(kamikazeTarget);
                    attacked = true;
                }
            }

            if (attacked) yield return new WaitForSeconds(1.5f);
        }
    }

    // Método auxiliar para calcular se a IA tem dano suficiente para ganhar o jogo neste turno
    private int CalculatePotentialFaceDamage(List<CardCombat> aiCards, bool pSoldiers, bool pHeroes, bool pCommanders)
    {
        int totalDamage = 0;
        foreach(var c in aiCards)
        {
            if (c.currentLife > 0 && c.canAttackThisTurn)
            {
                Card.CardRole role = c.GetComponent<CardDisplay>().cardData.cardRole;
                // Usa o juiz estático aqui também!
                if (CombatRules.CanAttackPlayer(role, pSoldiers, pHeroes, pCommanders))
                {
                    totalDamage += c.currentAttack;
                }
            }
        }
        return totalDamage;
    }

    // ==========================================
    // MÉTODOS DE ZONAS
    // ==========================================
    private string GetEnemyZoneTag(Card.CardRole role)
    {
        switch (role)
        {
            case Card.CardRole.Soldier: return "EnemyDropZoneSoldier";
            case Card.CardRole.Commander: return "EnemyDropZoneCommander";
            case Card.CardRole.Hero: return "EnemyDropZoneHero";
            default: return "Untagged";
        }
    }

    private GameObject FindEmptyZone(string tag)
    {
        GameObject[] zones = GameObject.FindGameObjectsWithTag(tag);
        foreach (GameObject zone in zones)
        {
            if (zone.transform.childCount == 0) return zone;
        }
        return null;
    }
}