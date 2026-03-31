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
    public bool isAITargeting = false;
    private BoardManager boardManager;

    private void Awake()
    {
        boardManager = BoardManager.Instance != null ? BoardManager.Instance : FindFirstObjectByType<BoardManager>();
    }
    
    public IEnumerator ProcessTurn()
    {
        Debug.Log("IA: Começando o meu turno...");
        yield return new WaitForSeconds(1f);

        yield return StartCoroutine(WaitUntilDustSettles());

        yield return StartCoroutine(PlayCardsPhase());
        
        yield return StartCoroutine(WaitUntilDustSettles());

        yield return StartCoroutine(AttackPhase());

        yield return StartCoroutine(WaitUntilDustSettles());

        yield return new WaitForSeconds(1f);
    }

    private IEnumerator WaitUntilDustSettles()
    {
        float idleTimer = 0f;
        
        // O jogo precisa ficar 100% parado por 0.5 segundos SEGUIDOS
        while (idleTimer < 0.5f)
        {
            if (ActionSystem.Instance.IsGameBusy())
            {
                idleTimer = 0f; // Alguém fez algo! Zera o cronômetro da IA.
            }
            else
            {
                idleTimer += Time.deltaTime; // Jogo está quieto, acumula tempo.
            }
            
            yield return null; // Espera o próximo frame da Unity
        }
    }

    // ==========================================
    // FASE 1: DESCER CARTAS
    // ==========================================
    private IEnumerator PlayCardsPhase()
    {
        List<GameObject> cardsInHand = new List<GameObject>(aiHand.cardsInHand);
        cardsInHand.Sort((a, b) => b.GetComponent<CardDisplay>().cardData.mana.CompareTo(a.GetComponent<CardDisplay>().cardData.mana));

        foreach (GameObject cardObj in cardsInHand)
        {
            yield return new WaitWhile(() => ActionSystem.Instance.IsGameBusy());

            CardData cardData = cardObj.GetComponent<CardDisplay>().cardData;

            if (cardData.mana <= aiMana.currentMana)
            {
                if (boardManager != null && boardManager.TryGetFreeSlot(cardData, true, out BoardSlot freeSlot))
                {
                    Debug.Log($"IA decidiu invocar: {cardData.cardName}");

                    aiMana.SpendMana(cardData.mana);
                    aiHand.RemoveCardFromHand(cardObj);

                    CardCombat combatScript = cardObj.GetComponent<CardCombat>();
                    if (combatScript != null)
                    {
                        combatScript.isEnemy = true;
                    }

                    if (boardManager.TryPlaceCard(combatScript, freeSlot))
                    {
                        CardDrag dragScript = cardObj.GetComponent<CardDrag>();
                        if (dragScript != null)
                        {
                            dragScript.TransformIntoTokenAndJump(freeSlot, true);
                            boardManager.NotifyCardPlaced(combatScript, freeSlot, BoardEntryType.PlayedFromHand);
                            combatScript.TriggerEffects(EffectTriggerType.OnPlay);
                        }

                        yield return new WaitForSeconds(1.2f);
                    }
                }
            }
        }
    }

    // ==========================================
    // FASE 2: COMBATE COM INTELIGÊNCIA AVANÇADA
    // ==========================================
    private IEnumerator AttackPhase()
    {
        PlayerHealth playerHealth = GameObject.FindGameObjectWithTag("PlayerHealth")?.GetComponent<PlayerHealth>();
        PlayerHealth aiHealth = GameObject.FindGameObjectWithTag("EnemyHealth")?.GetComponent<PlayerHealth>();

        CardCombat[] allCardsOnBoard = FindObjectsByType<CardCombat>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        List<CardCombat> aiCards = allCardsOnBoard.Where(c => c.isEnemy && c.canAttackThisTurn && c.currentLife > 0).ToList();

        foreach (CardCombat aiCard in aiCards)
        {
            if (aiCard == null || aiCard.currentLife <= 0 || aiCard.isDead) continue; 

            // 👇 TRAVA NÍVEL 3: Espera o combate anterior terminar COMPLETAMENTE
            yield return new WaitWhile(() => ActionSystem.Instance.IsGameBusy());

            List<CardCombat> playerCards = FindObjectsByType<CardCombat>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
                .Where(c => !c.isEnemy && c.GetComponent<CardDrag>().isPlayed && c.currentLife > 0 && !c.isDead).ToList();

            bool pHasSoldiers = playerCards.Any(c => c.GetComponent<CardDisplay>().cardData.cardRole == CardData.CardRole.Soldier);
            bool pHasHeroes = playerCards.Any(c => c.GetComponent<CardDisplay>().cardData.cardRole == CardData.CardRole.Hero);
            bool pHasCommanders = playerCards.Any(c => c.GetComponent<CardDisplay>().cardData.cardRole == CardData.CardRole.Commander);

            CardData.CardRole myRole = aiCard.GetComponent<CardDisplay>().cardData.cardRole;

            bool canAttackFace = CombatRules.CanAttackPlayer(myRole, pHasSoldiers, pHasHeroes, pHasCommanders);

            List<CardCombat> validTargets = playerCards.Where(pCard =>
            {
                CardData.CardRole targetRole = pCard.GetComponent<CardDisplay>().cardData.cardRole;
                return CombatRules.CanAttackCard(myRole, targetRole, pHasSoldiers, pHasHeroes, pHasCommanders);
            }).ToList();

            int myTotalFaceDamage = CalculatePotentialFaceDamage(aiCards, pHasSoldiers, pHasHeroes, pHasCommanders);
            int enemyTotalDamage = playerCards.Sum(c => c.currentAttack); 

            bool iHaveLethal = playerHealth != null && myTotalFaceDamage >= playerHealth.currentHealth;
            bool enemyHasLethal = aiHealth != null && enemyTotalDamage >= aiHealth.currentHealth;

            bool attacked = false;

            // MODO 1: LETAL
            if (iHaveLethal && canAttackFace && playerHealth != null)
            {
                Debug.Log($"IA [LETAL]: {aiCard.name} ataca o Jogador para vencer!");
                aiCard.Attack(playerHealth);
                attacked = true;
            }
            // MODO 2: DEFESA
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
                else if (canAttackFace && playerHealth != null)
                {
                    Debug.Log($"IA [AGGRO]: {aiCard.name} bate direto na vida do Jogador.");
                    aiCard.Attack(playerHealth);
                    attacked = true;
                }
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

    private int CalculatePotentialFaceDamage(List<CardCombat> aiCards, bool pSoldiers, bool pHeroes, bool pCommanders)
    {
        int totalDamage = 0;
        foreach(var c in aiCards)
        {
            if (c.currentLife > 0 && c.canAttackThisTurn && !c.isDead) 
            {
                CardData.CardRole role = c.GetComponent<CardDisplay>().cardData.cardRole;
                if (CombatRules.CanAttackPlayer(role, pSoldiers, pHeroes, pCommanders))
                {
                    totalDamage += c.currentAttack;
                }
            }
        }
        return totalDamage;
    }
    
    // ==========================================
    // FASE 3: ESCOLHA DE ALVOS PARA EFEITOS (MÁGICAS)
    // ==========================================
    
    // 👇 RECEBE O rawData AQUI AGORA
    public IEnumerator ResolveAITargetingCoroutine(CardCombat source, CardData cardData, CardEffect effect, EffectData rawData)
    {
        isAITargeting = true; 
        yield return new WaitForSeconds(1.5f);

        PlayerHealth playerHealth = GameObject.FindGameObjectWithTag("PlayerHealth")?.GetComponent<PlayerHealth>();
        
        CardCombat[] allCards = FindObjectsByType<CardCombat>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        List<CardCombat> validCardTargets = new List<CardCombat>();

        foreach (var c in allCards)
        {
            if (c.GetComponent<CardDrag>() != null && c.GetComponent<CardDrag>().isPlayed && c.currentLife > 0 && !c.isDead)
            {
                if (effect.IsValidTarget(source, c, null, rawData))
                {
                    validCardTargets.Add(c);
                }
            }
        }

        bool canTargetPlayer = effect.IsValidTarget(source, null, playerHealth, rawData);

        CardEffectContext context = new CardEffectContext { source = source, playerHand = aiHand, isEnemySource = true };
        bool foundTarget = false;

        List<CardCombat> enemiesToAI = validCardTargets.Where(c => !c.isEnemy).ToList(); 
        List<CardCombat> alliesToAI = validCardTargets.Where(c => c.isEnemy).ToList();  

        if (enemiesToAI.Count > 0)
        {
            CardCombat chosenCard = enemiesToAI.OrderByDescending(c => c.currentAttack).First();
            context.targetCard = chosenCard;
            foundTarget = true;
            Debug.Log($"IA [MÁGICA]: {cardData.cardName} focou no lacaio inimigo {chosenCard.name}.");
        }
        else if (canTargetPlayer)
        {
            context.targetPlayer = playerHealth;
            foundTarget = true;
            Debug.Log($"IA [MÁGICA]: {cardData.cardName} atirou o efeito direto na vida do Jogador.");
        }
        else if (alliesToAI.Count > 0)
        {
            CardCombat chosenCard = alliesToAI.OrderByDescending(c => c.currentAttack).First();
            context.targetCard = chosenCard;
            foundTarget = true;
            Debug.Log($"IA [MÁGICA - BUFF/FORÇADO]: {cardData.cardName} alvejou a própria carta {chosenCard.name}.");
        }

        if (foundTarget)
        {
            // 👇 PASSA O rawData PARA A FÁBRICA DA AÇÃO AQUI
            GameAction action = effect.CreateAction(context, rawData);
            if (action != null)
            {
                ActionSystem.Instance.AddAction(action);
            }
        }
        else
        {
            Debug.Log($"IA [MÁGICA]: {cardData.cardName} entrou em campo, mas não havia alvos válidos para o efeito.");
        }
        isAITargeting = false;
    }
}