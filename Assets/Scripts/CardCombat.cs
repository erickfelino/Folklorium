using UnityEngine;
using System;
using System.Collections;
using DG.Tweening;
using Folklorium;

[RequireComponent(typeof(CardDisplay))]
[RequireComponent(typeof(CardDrag))]
public class CardCombat : MonoBehaviour, IEffectSource
{
    public bool IsEnemy => isEnemy;
    public Transform EffectTransform => transform;
    public GameObject EffectGameObject => gameObject;

    public string SourceName
    {
        get
        {
            if (display != null && display.cardData != null)
                return display.cardData.cardName;
            return gameObject.name;
        }
    }
    private TurnManager turnManager;
    private bool isPerformingAttack = false;
    private bool pendingDeath = false;

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
        if (!cardDrag.isPlayed)
            return;

        // Carta pode atacar somente no turno do seu dono
        bool shouldWake =
            (isPlayerTurn && !isEnemy) ||
            (!isPlayerTurn && isEnemy);

        canAttackThisTurn = shouldWake;

        RefreshGlowState();
    }

    public void TriggerEffects(Folklorium.EffectTriggerType targetTrigger, CardCombat targetCard = null, PlayerHealth targetPlayer = null)
    {
        if (display == null || display.cardData == null || display.cardData.effects == null) return;

        foreach (Folklorium.EffectEntry entry in display.cardData.effects)
        {
            if (entry != null && entry.effectSO != null && entry.trigger == targetTrigger)
            {
                CardEffect effectSO = entry.effectSO;
                EffectData rawData = entry.parameters;

                CardEffectContext context = new CardEffectContext
                {
                    source = this,
                    targetCard = targetCard,
                    targetPlayer = targetPlayer
                };

                // Efeitos em área resolvem os próprios alvos dentro da Action.
                if (rawData is AoEEffectData)
                {
                    GameAction aoeAction = effectSO.CreateAction(context, rawData);
                    if (aoeAction != null)
                        ActionSystem.Instance.AddAction(aoeAction);

                    continue;
                }

                // Efeitos com alvo aleatório
                if (effectSO.requiresTarget && targetCard == null && targetPlayer == null)
                {
                    if (rawData.RandomizeTarget())
                    {
                        EffectTargetManager.Instance.ResolveRandomTarget(this, effectSO, rawData);
                    }
                    else if (this.isEnemy)
                    {
                        OpponentAI ai = FindFirstObjectByType<OpponentAI>();
                        if (ai != null)
                            ai.StartCoroutine(ai.ResolveAITargetingCoroutine(this, display.cardData, effectSO, rawData));
                    }
                    else
                    {
                        EffectTargetManager.Instance.StartTargeting(this, display.cardData, effectSO, rawData);
                    }
                }
                else
                {
                    GameAction action = effectSO.CreateAction(context, rawData);
                    if (action != null)
                        ActionSystem.Instance.AddAction(action);
                }
            }
        }
    }

    public void ApplyRawStateChange(int attackChange, int lifeChange, bool isBuff = false)
    {
        currentAttack += attackChange;
        if (currentAttack < 0) currentAttack = 0;

        if (isBuff)
        {
            maxLife += lifeChange;
            currentLife += lifeChange;
        }
        else
        {
            currentLife += lifeChange;
            if (currentLife > maxLife)
            {
                currentLife = maxLife;
            }
        }

        if (currentLife <= 0)
        {
            Die();
        }
        display.UpdateStatusText(currentLife, currentAttack);
    }

    public void Attack(CardCombat targetCard)
    {
        if (!canAttackThisTurn)
        {
            Debug.Log("Esta criatura não pode atacar neste turno!");
            return;
        }

        canAttackThisTurn = false;
        RefreshGlowState();
        StartCoroutine(AttackChoreography(targetCard));
    }

    private IEnumerator AttackChoreography(CardCombat targetCard)
    {
        if (targetCard == null)
            yield break;

        isPerformingAttack = true;

        Vector3 originalPos = transform.position;

        yield return transform.DOMove(targetCard.transform.position, 0.15f).WaitForCompletion();

        int myDamage = this.currentAttack;
        int enemyDamage = targetCard.currentAttack;

        ActionSystem.Instance.AddAction(new DamageAction(targetCard, null, myDamage));
        ActionSystem.Instance.AddAction(new DamageAction(this, null, enemyDamage));

        TriggerEffects(Folklorium.EffectTriggerType.OnAttack, targetCard);

        yield return new WaitForSeconds(0.35f);

        if (this != null && gameObject != null && transform != null)
        {
            yield return transform.DOMove(originalPos, 0.5f).SetEase(Ease.OutQuart).WaitForCompletion();
        }

        isPerformingAttack = false;

        if (pendingDeath)
        {
            pendingDeath = false;
            StartCoroutine(DeathSequence());
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
        RefreshGlowState();
        StartCoroutine(AttackChoreographyPlayer(targetHealth));
    }

    private IEnumerator AttackChoreographyPlayer(PlayerHealth targetHealth)
    {
        if (targetHealth == null)
            yield break;

        isPerformingAttack = true;

        Vector3 originalPos = transform.position;

        yield return transform.DOMove(targetHealth.transform.position, 0.15f).WaitForCompletion();

        int myDamage = this.currentAttack;

        ActionSystem.Instance.AddAction(new DamageAction(null, targetHealth, myDamage));
        TriggerEffects(Folklorium.EffectTriggerType.OnAttack, null, targetHealth);

        yield return new WaitForSeconds(0.35f);

        if (this != null && gameObject != null && transform != null)
        {
            yield return transform.DOMove(originalPos, 0.75f).WaitForCompletion();
        }

        isPerformingAttack = false;

        if (pendingDeath)
        {
            pendingDeath = false;
            StartCoroutine(DeathSequence());
        }
    }

    public void RefreshGlowState(bool isTargetingMode = false, bool isValidTarget = false)
    {
        if (isDead) return;

        CardDrag dragObj = GetComponent<CardDrag>();
        if (dragObj == null) return;

        if (isTargetingMode)
        {
            if (isValidTarget)
            {
                dragObj.SetGlow(true, Color.red);
            }
            else
            {
                dragObj.SetGlow(false, Color.white);
            }
            return;
        }

        if (!isEnemy && canAttackThisTurn)
        {
            dragObj.SetGlow(true, Color.green);
        }
        else
        {
            dragObj.SetGlow(false, Color.white);
        }
    }

    public void Die()
    {
        if (isDead) return;

        isDead = true;

        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        // Se estiver no meio de um ataque, não mata agora.
        // Só marca que a morte precisa acontecer depois do retorno.
        if (isPerformingAttack)
        {
            pendingDeath = true;
            return;
        }

        StartCoroutine(DeathSequence());
    }
    private IEnumerator DeathSequence()
    {
        if (display != null && display.cardData != null)
            Debug.Log($"{display.cardData.cardName} foi destruído!");

        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        if (transform != null)
            transform.DOKill();

        BoardManager.Instance?.ReleaseCard(this);

        OnDeath?.Invoke(this);
        TriggerEffects(Folklorium.EffectTriggerType.OnDeath);

        yield return null;

        if (ActionSystem.Instance != null)
            yield return new WaitWhile(() => ActionSystem.Instance.IsGameBusy());

        Destroy(gameObject);
    }
}