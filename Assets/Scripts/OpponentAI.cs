using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq; // Precisamos disso para a IA poder "ordenar" a mão por custo de mana
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
        
        // DICA: Aqui você pode chamar o TurnManager.Instance.EndTurn() se quiser que a IA passe o turno sozinha!
    }

    // ==========================================
    // FASE 1: DESCER CARTAS (Lógica de Curva de Mana)
    // ==========================================
    private IEnumerator PlayCardsPhase()
    {
        // 1. Pega as cartas da mão e ORDENA da mais cara para a mais barata
        List<GameObject> cardsInHand = new List<GameObject>(aiHand.cardsInHand);
        cardsInHand.Sort((a, b) => b.GetComponent<CardDisplay>().cardData.mana.CompareTo(a.GetComponent<CardDisplay>().cardData.mana));

        // 2. Tenta jogar o que der
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
                        
                        // IMPORTANTE: Marca a carta como sendo do inimigo IMEDIATAMENTE!
                        CardCombat combatScript = cardObj.GetComponent<CardCombat>();
                        if (combatScript != null)
                        {
                            combatScript.isEnemy = true;
                        }
                    }

                    // Espera a carta cair na mesa antes de jogar a próxima
                    yield return new WaitForSeconds(1.2f); 
                }
            }
        }
    }

    // ==========================================
    // FASE 2: COMBATE (Lógica de Troca Inteligente vs Aggro)
    // ==========================================
    private IEnumerator AttackPhase()
    {
        // 1. Mapeia o campo de batalha inteiro
        CardCombat[] allCardsOnBoard = FindObjectsByType<CardCombat>(FindObjectsSortMode.None);
        
        List<CardCombat> aiCards = new List<CardCombat>();
        List<CardCombat> playerCards = new List<CardCombat>();

        foreach (CardCombat card in allCardsOnBoard)
        {
            if (card.isEnemy && card.canAttackThisTurn && card.currentLife > 0)
            {
                aiCards.Add(card); // Soldados da IA prontos pra guerra
            }
            else if (!card.isEnemy && card.GetComponent<CardDrag>().isPlayed && card.currentLife > 0)
            {
                playerCards.Add(card); // Soldados do Jogador
            }
        }

        // Encontra o alvo da vida do Jogador
        GameObject playerHealthObj = GameObject.FindGameObjectWithTag("PlayerHealth");
        PlayerHealth playerHealth = playerHealthObj != null ? playerHealthObj.GetComponent<PlayerHealth>() : null;

        // 2. A IA ataca com cada carta que ela tem
        foreach (CardCombat aiCard in aiCards)
        {
            if (aiCard == null || aiCard.currentLife <= 0) continue; // Pode ter morrido num ataque anterior

            bool foundGoodTrade = false;

            // Tenta achar uma "Troca Favorável"
            foreach (CardCombat playerCard in playerCards)
            {
                if (playerCard == null || playerCard.currentLife <= 0) continue;

                // LÓGICA DE GÊNIO: Se o meu ataque mata ele, E a vida dele NÃO me mata... é uma troca perfeita!
                if (aiCard.currentAttack >= playerCard.currentLife && aiCard.currentLife > playerCard.currentAttack)
                {
                    Debug.Log($"IA: Troca favorável encontrada! {aiCard.name} vai destruir {playerCard.name}");
                    aiCard.Attack(playerCard);
                    foundGoodTrade = true;
                    
                    // Espera a coreografia do DOTween acabar antes de dar a ordem pro próximo lacaio
                    yield return new WaitForSeconds(1.5f); 
                    break; // Sai do loop de busca de alvos e vai para o próximo lacaio da IA
                }
            }

            // Se a IA olhou todas as cartas e não achou nenhuma troca boa... VAI DIRETO NA CARA!
            if (!foundGoodTrade && playerHealth != null)
            {
                Debug.Log($"IA: Sem trocas boas. {aiCard.name} vai atacar direto a Vida do Jogador!");
                aiCard.Attack(playerHealth);
                yield return new WaitForSeconds(1.5f);
            }
        }
    }

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