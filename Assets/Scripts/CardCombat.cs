using UnityEngine;
using System;
using System.Collections;
using DG.Tweening;

[RequireComponent(typeof(CardDisplay))]
[RequireComponent(typeof(CardDrag))]

public class CardCombat : MonoBehaviour
{
    private TurnManager turnManager; // Agora é privado, não precisamos arrastar nada!

    [Header("Status de Combate")]
    public int currentAttack;
    public int currentLife;
    public bool canAttackThisTurn = false; // Controla o enjoo de invocação
    public bool isEnemy; // Para saber se quem controla essa carta é a IA
    public bool isDead = false; // Impede que a carta "morra" duas vezes

    private CardDisplay display;
    private CardDrag cardDrag;

    // A MESA PODE OUVIR ESSE EVENTO PARA SABER QUANDO UMA CARTA MORRE!
    public event Action<CardCombat> OnDeath;

    private void TriggerEffects(EffectTriggerType targetTrigger, CardCombat targetCard = null, PlayerHealth targetPlayer = null)
    {
        if (display == null || display.cardData == null || display.cardData.effects == null) return;

        foreach (var effect in display.cardData.effects)
        {
            if (effect != null && effect.trigger == targetTrigger)
            {
                if (effect.requiresTarget && targetCard == null && targetPlayer == null)
                {

                    if (this.isEnemy)
                    {
                        OpponentAI ai = FindFirstObjectByType<OpponentAI>();
                        if (ai != null) ai.StartCoroutine(ai.ResolveAITargetingCoroutine(this, display.cardData, effect));
                    }
                    else
                    {
                        EffectTargetManager.Instance.StartTargeting(this, display.cardData, effect);
                    }
                }
                else
                {
                    CardEffectContext context = new CardEffectContext
                    {
                        source = this, targetCard = targetCard, targetPlayer = targetPlayer, isEnemySource = this.isEnemy
                    };
                    effect.Execute(context); 
                }
            }
        }
    }

    void Start()
    {
        // 1. Procuramos o gerente de turno com segurança
        turnManager = FindFirstObjectByType<TurnManager>();

        // 2. Ligamos a "rádio" do turno
        if (turnManager != null)
        {
            turnManager.OnTurnChanged += WakeUpCard;
        }
        else
        {
            // Se cair aqui, é porque você esqueceu de colocar o TurnManager na cena!
            Debug.LogError("ERRO: O TurnManager não foi encontrado na cena!");
        }

        // 3. Prepara os status da carta (o que já tínhamos antes)
        display = GetComponent<CardDisplay>();
        if (display != null && display.cardData != null)
        {
            currentAttack = display.cardData.attack;
            currentLife = display.cardData.life;
        }

        cardDrag =  GetComponent<CardDrag>();
    }

    void OnDestroy()
    {
        // Quando a carta for destruída (Die), ela cancela a assinatura da rádio
        if (turnManager != null)
        {
            turnManager.OnTurnChanged -= WakeUpCard;
        }
    }

    private void WakeUpCard(bool isPlayerTurn)
    {
        // Se for o turno do jogador e eu NÃO sou inimigo, eu acordo!
        if (isPlayerTurn && !isEnemy && cardDrag.isPlayed)
        {
            canAttackThisTurn = true;
        }
        // Se for o turno do inimigo e eu SOU inimigo, eu acordo!
        else if (!isPlayerTurn && isEnemy && cardDrag.isPlayed)
        {
            canAttackThisTurn = true;
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentLife -= damage;
        display.UpdateLifeText(currentLife);
        TriggerEffects(EffectTriggerType.OnDamaged);

        // Feedback de dano: a carta treme e SÓ DEPOIS checa se morreu
        transform.DOShakePosition(0.3f, 0.2f, 10, 90f).OnComplete(() => 
        {
            if (currentLife <= 0)
            {
                currentLife = 0;
                Die();
            }
        });
    }

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

        if (targetCard != null) targetCard.TakeDamage(myDamage);
        if (this != null) this.TakeDamage(enemyDamage);
        TriggerEffects(EffectTriggerType.OnAttack, targetCard);

        yield return new WaitForSeconds(0.35f);

        if (this != null)
        {
            transform.DOMove(originalPos, 0.75f);
        }
    }
    public void Attack(PlayerHealth targetHealth)
    {
        if (!canAttackThisTurn) 
        {
            Debug.Log("Esta criatura não pode atacar neste turno!");
            return;
        }

        canAttackThisTurn = false; // Exausta a carta
        
        // Chama a nova corrotina específica para o jogador
        StartCoroutine(AttackChoreographyPlayer(targetHealth)); 
    }

    private IEnumerator AttackChoreographyPlayer(PlayerHealth targetHealth)
    {

        Vector3 originalPos = transform.position;

        // 1. Vai até a cara do inimigo (avatar/cristal) e ESPERA a viagem terminar
        yield return transform.DOMove(targetHealth.transform.position,0.15f).WaitForCompletion();

        // 2. Aplica o dano SOMENTE no jogador (o jogador não bate de volta na carta)
        int myDamage = this.currentAttack;

        if (targetHealth != null)
        {
            targetHealth.PlayerTakeDamage(myDamage);
            TriggerEffects(EffectTriggerType.OnAttack, null, targetHealth);
        }

        // 3. Espera um pouco para dar o impacto visual (0.35s igual ao outro)
        yield return new WaitForSeconds(0.35f);

        // 4. Volta para casa (se a carta ainda existir)
        if (this != null)
        {
            transform.DOMove(originalPos, 0.75f);
        }
    }

// Transformamos a morte numa Corrotina elegante!
    private void Die()
    {
        isDead = true;
        StartCoroutine(DeathSequence());
    }

    private IEnumerator DeathSequence()
    {
        Debug.Log($"{display.cardData.cardName} foi destruído!");

        // 1. Desliga o colisor para não sofrer novos ataques enquanto agoniza
        Collider col = GetComponent<Collider>(); 
        if (col != null) col.enabled = false;

        // 2. Aciona o OnDeath (que pode ativar a Seta e ligar o Sinal Vermelho internamente)
        TriggerEffects(EffectTriggerType.OnDeath);

        OnDeath?.Invoke(this); 
        transform.DOKill(); 

        // 3. O FANTASMA: Se o TriggerEffects ligou o Sinal Vermelho, a carta "pausa" a própria morte aqui!
        yield return new WaitUntil(() => !TurnManager.isResolvingEffect);

        // 4. Sinal Verde! O Efeito resolveu. A carta pode desaparecer em paz.
        Destroy(gameObject);
    }
}