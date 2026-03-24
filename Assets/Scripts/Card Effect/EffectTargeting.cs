// EffectTargeting.cs
using UnityEngine;
using Folklorium;

[RequireComponent(typeof(CardCombat))]
public class EffectTargeting : MonoBehaviour
{
    private TargetingArrow arrow;
    private CardCombat myCombat;
    private Card myCardData;
    private Camera mainCamera;

    private bool isWaitingForTarget = false;

    void Start()
    {
        myCombat = GetComponent<CardCombat>();
        myCardData = GetComponent<CardDisplay>().cardData;
        mainCamera = Camera.main;
        arrow = Object.FindFirstObjectByType<TargetingArrow>();
    }

    // O CardDrag vai chamar isso aqui!
    public void StartTargeting()
    {
        isWaitingForTarget = true;
        if (arrow != null) arrow.ShowArrow(true);
        
        // Opcional: Você pode colocar um som aqui ou fazer a carta brilhar!
        Debug.Log("Escolha um alvo para o efeito da carta!");
    }

    void Update()
    {
        if (!isWaitingForTarget) return;

        // 1. A seta segue o mouse o tempo todo
        if (arrow != null)
        {
            Vector3 endPoint = GetMouseWorldPosition();
            arrow.UpdateArrow(transform.position, endPoint);
        }

        // 2. Quando o jogador CLICAR (botão esquerdo), disparamos o tiro!
        if (Input.GetMouseButtonDown(0))
        {
            TrySelectTarget();
        }
    }

    private void TrySelectTarget()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            CardEffectContext context = new CardEffectContext
            {
                source = myCombat,
                isEnemySource = myCombat.isEnemy
            };

            // Se clicou num LACAIO INIMIGO
            if (hit.collider.CompareTag("Card"))
            {
                CardCombat targetCard = hit.collider.GetComponent<CardCombat>();
                if (targetCard != null && targetCard.isEnemy) // Garante que é inimigo
                {
                    context.targetCard = targetCard;
                    ExecuteAndFinish(context);
                }
            }
            // Se clicou no JOGADOR INIMIGO (Torre/Avatar)
            else if (hit.collider.CompareTag("EnemyHealth"))
            {
                PlayerHealth targetPlayer = hit.collider.GetComponent<PlayerHealth>();
                if (targetPlayer != null)
                {
                    context.targetPlayer = targetPlayer;
                    ExecuteAndFinish(context);
                }
            }
            else
            {
                Debug.Log("Alvo inválido. Clique em um inimigo!");
                // Fica esperando clicar num alvo certo. Não desliga a seta.
            }
        }
    }

    private void ExecuteAndFinish(CardEffectContext context)
    {
        // 1. Desliga a mira
        isWaitingForTarget = false;
        if (arrow != null) arrow.ShowArrow(false);

        // 2. Executa a Mágica!
        CardEffectExecutor.ExecuteEffects(myCardData, context, EffectTriggerType.OnPlay);
        Debug.Log("Efeito engatilhado com sucesso no alvo escolhido!");
    }

    private Vector3 GetMouseWorldPosition()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, transform.position); 
        if (groundPlane.Raycast(ray, out float distance))
        {
            return ray.GetPoint(distance); 
        }
        return transform.position; 
    }
}