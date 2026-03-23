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
    public Transform cardVisuals;

    private CardDisplay display;
    private CardDrag cardDrag;

    // A MESA PODE OUVIR ESSE EVENTO PARA SABER QUANDO UMA CARTA MORRE!
    public event Action<CardCombat> OnDeath;

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
        currentLife -= damage;
        display.UpdateLifeText(currentLife);

        // Feedback de dano: a carta treme e SÓ DEPOIS checa se morreu
        transform.DOShakePosition(0.3f, 0.2f, 10, 90f).OnComplete(() => 
        {
            if (currentLife <= 0)
            {
                Die();
            }
        });
    }

    // ==========================================
    // REALIZANDO O ATAQUE
    // ==========================================
// ==========================================
    // REALIZANDO O ATAQUE
    // ==========================================
    public void Attack(CardCombat targetCard)
    {
        if (!canAttackThisTurn) 
        {
            Debug.Log("Esta criatura não pode atacar neste turno!");
            return;
        }

        canAttackThisTurn = false; // Já cansa ela agora para evitar duplo clique

        // Inicia a coreografia de ataque!
        StartCoroutine(AttackChoreography(targetCard));
    }

    private IEnumerator AttackChoreography(CardCombat targetCard)
    {
        Vector3 originalPos = transform.position;

        // 1. Vai até o inimigo e ESPERA a viagem terminar (WaitForCompletion)
        yield return transform.DOMove(targetCard.transform.position, 0.15f).WaitForCompletion();

        // 2. Aplica o dano (O TakeDamage agora faz elas tremerem por 0.3s)
        int myDamage = this.currentAttack;
        int enemyDamage = targetCard.currentAttack;

        if (targetCard != null) targetCard.TakeDamage(myDamage);
        if (this != null) this.TakeDamage(enemyDamage);

        // 3. A Mágica que você sugeriu: ESPERA o tremor acabar antes de fazer qualquer coisa.
        // Coloquei 0.35s para dar uma margem de segurança além dos 0.3s do Shake.
        yield return new WaitForSeconds(0.35f);

        // 4. Volta para casa (só inicia a volta se você ainda existir e estiver vivo!)
        if (this != null && this.currentLife > 0)
        {
            transform.DOMove(originalPos, 0.75f);
        }
    }

    // ==========================================
    // MORTE
    // ==========================================
    private void Die()
    {
        Debug.Log($"{display.cardData.cardName} foi destruído!");
        
        OnDeath?.Invoke(this); // Grita no rádio que morreu
        
        // Mata qualquer animação do DOTween rodando nela antes de destruir
        transform.DOKill(); 
        Destroy(gameObject);
    }
}