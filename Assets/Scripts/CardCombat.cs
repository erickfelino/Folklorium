using UnityEngine;
using System;
using System.Collections;
using DG.Tweening;

[RequireComponent(typeof(CardDisplay))]
[RequireComponent(typeof(CardDrag))]
public class CardCombat : MonoBehaviour
{
    private TurnManager turnManager;

    [Header("Status de Combate")]
    public int currentAttack;
    public int currentLife;
    public int maxLife; 
    public bool canAttackThisTurn = false;
    public bool isEnemy;
    public bool isDead = false;

    private CardDisplay display;
    private CardDrag cardDrag;

    public event Action<CardCombat> OnDeath;

    // ==========================================
    // INICIALIZAÇÃO E EVENTOS DE TURNO
    // ==========================================

    void Start()
    {
        turnManager = FindFirstObjectByType<TurnManager>();

        if (turnManager != null)
        {
            turnManager.OnTurnChanged += WakeUpCard;
        }
        else
        {
            Debug.LogError("ERRO: O TurnManager não foi encontrado na cena!");
        }

        display = GetComponent<CardDisplay>();
        if (display != null && display.cardData != null)
        {
            currentAttack = display.cardData.attack;
            currentLife = display.cardData.life;
            maxLife = display.cardData.life;
        }

        cardDrag = GetComponent<CardDrag>();
    }

    void OnDestroy()
    {
        if (turnManager != null)
        {
            turnManager.OnTurnChanged -= WakeUpCard;
        }
    }

    private void WakeUpCard(bool isPlayerTurn)
    {
        if (isPlayerTurn && !isEnemy && cardDrag.isPlayed)
        {
            canAttackThisTurn = true;
        }
        else if (!isPlayerTurn && isEnemy && cardDrag.isPlayed)
        {
            canAttackThisTurn = true;
        }
    }

    // ==========================================
    // SISTEMA DE TRIGGERS (ATUALIZADO PARA POLIMORFISMO)
    // ==========================================

    public void TriggerEffects(Folklorium.EffectTriggerType targetTrigger, CardCombat targetCard = null, PlayerHealth targetPlayer = null)
    {
        if (display == null || display.cardData == null || display.cardData.effects == null) return;

        // Agora nós iteramos sobre as "Entradas" (EffectEntry) e não mais direto no SO
        foreach (Folklorium.EffectEntry entry in display.cardData.effects)
        {
            if (entry != null && entry.effectSO != null && entry.trigger == targetTrigger)
            {
                CardEffect effectSO = entry.effectSO;
                EffectData rawData = entry.parameters; // <--- Pegamos o pacote de dados do Inspector!

                if (effectSO.requiresTarget && targetCard == null && targetPlayer == null)
                {
                    if (this.isEnemy)
                    {
                        OpponentAI ai = FindFirstObjectByType<OpponentAI>();
                        if (ai != null) ai.StartCoroutine(ai.ResolveAITargetingCoroutine(this, display.cardData, effectSO, rawData));
                    }
                    else
                    {
                        EffectTargetManager.Instance.StartTargeting(this, display.cardData, effectSO, rawData);
                    }
                }
                else
                {
                    CardEffectContext context = new CardEffectContext
                    {
                        source = this, targetCard = targetCard, targetPlayer = targetPlayer, isEnemySource = this.isEnemy
                    };
                    
                    // 👇 A MÁGICA ACONTECE AQUI! Passamos o Contexto E os Dados
                    GameAction action = effectSO.CreateAction(context, rawData);
                    
                    if (action != null)
                    {
                        ActionSystem.Instance.AddAction(action);
                    }
                }
            }
        }
    }

    // ==========================================
    // API DE ESTADO (NÍVEL 3)
    // ==========================================

    public void ApplyRawStateChange(int attackChange, int lifeChange)
    {
        currentAttack += attackChange;
        currentLife += lifeChange;

        if (currentAttack < 0) currentAttack = 0;
        if (currentLife > maxLife) currentLife = maxLife; 
        if (currentLife <= 0) currentLife = 0;

        display.UpdateLifeText(currentLife);
    }

    // ==========================================
    // SISTEMA DE COMBATE FÍSICO
    // ==========================================

    public void Attack(CardCombat targetCard)
    {
        if (!canAttackThisTurn) 
        {
            Debug.Log("Esta criatura não pode atacar neste turno!");
            return;
        }

        canAttackThisTurn = false; 
        StartCoroutine(AttackChoreography(targetCard));
    }

    private IEnumerator AttackChoreography(CardCombat targetCard)
    {
        Vector3 originalPos = transform.position;

        yield return transform.DOMove(targetCard.transform.position, 0.15f).WaitForCompletion();

        int myDamage = this.currentAttack;
        int enemyDamage = targetCard.currentAttack;

        ActionSystem.Instance.AddAction(new DamageAction(this, targetCard, null, myDamage));
        ActionSystem.Instance.AddAction(new DamageAction(targetCard, this, null, enemyDamage));

        TriggerEffects(Folklorium.EffectTriggerType.OnAttack, targetCard);

        yield return new WaitForSeconds(0.35f);

        if (this != null && transform != null)
        {
            yield return transform.DOMove(originalPos, 0.5f).SetEase(Ease.OutQuart).WaitForCompletion();
        }
    }

    public void Attack(PlayerHealth targetHealth)
    {
        if (!canAttackThisTurn) 
        {
            Debug.Log("Esta criatura não pode atacar neste turno!");
            return;
        }

        canAttackThisTurn = false; 
        StartCoroutine(AttackChoreographyPlayer(targetHealth)); 
    }

    private IEnumerator AttackChoreographyPlayer(PlayerHealth targetHealth)
    {
        Vector3 originalPos = transform.position;

        yield return transform.DOMove(targetHealth.transform.position,0.15f).WaitForCompletion();

        int myDamage = this.currentAttack;

        if (targetHealth != null)
        {
            ActionSystem.Instance.AddAction(new DamageAction(this, null, targetHealth, myDamage));
            TriggerEffects(Folklorium.EffectTriggerType.OnAttack, null, targetHealth);
        }

        yield return new WaitForSeconds(0.35f);

        if (this != null)
        {
            transform.DOMove(originalPos, 0.75f);
        }
    }

    // ==========================================
    // SISTEMA DE MORTE
    // ==========================================

    public void Die()
    {
        if (isDead) return;
        isDead = true;
        StartCoroutine(DeathSequence());
    }

    private IEnumerator DeathSequence()
    {
        Debug.Log($"{display.cardData.cardName} foi destruído!");

        isDead = true;
        Collider col = GetComponent<Collider>(); 
        if (col != null) col.enabled = false;
        
        TriggerEffects(Folklorium.EffectTriggerType.OnDeath);
        OnDeath?.Invoke(this); 

        yield return transform.DOComplete(); 

        yield return new WaitWhile(() => ActionSystem.Instance.IsGameBusy());

        Destroy(gameObject);
    }
}