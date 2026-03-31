// EffectTargeting.cs
using UnityEngine;
using Folklorium;

[RequireComponent(typeof(CardCombat))]
public class EffectTargeting : MonoBehaviour
{
    private TargetingArrow arrow;
    private CardCombat myCombat;
    private CardData myCardData;
    private Camera mainCamera;

    private bool isWaitingForTarget = false;

    void Start()
    {
        myCombat = GetComponent<CardCombat>();
        myCardData = GetComponent<CardDisplay>().cardData;
        mainCamera = Camera.main;
        arrow = Object.FindFirstObjectByType<TargetingArrow>();
    }

    public void StartTargeting()
    {
        isWaitingForTarget = true;
        if (arrow != null) arrow.ShowArrow(true);
        
        Debug.Log("Escolha um alvo para o efeito da carta!");
    }

    void Update()
    {
        if (!isWaitingForTarget) return;

        if (arrow != null)
        {
            Vector3 endPoint = GetMouseWorldPosition();
            arrow.UpdateArrow(transform.position, endPoint);
        }

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

            if (hit.collider.CompareTag("Card"))
            {
                CardCombat targetCard = hit.collider.GetComponent<CardCombat>();
                if (targetCard != null && targetCard.isEnemy) 
                {
                    context.targetCard = targetCard;
                    ExecuteAndFinish(context);
                }
            }
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
            }
        }
    }

    private void ExecuteAndFinish(CardEffectContext context)
    {
        isWaitingForTarget = false;
        if (arrow != null) arrow.ShowArrow(false);

        // O CardEffectExecutor já foi atualizado para gerar GameActions!
        // Então chamar ele aqui já garante que o fluxo vá para o ActionSystem.
        CardEffectExecutor.ExecuteEffects(myCardData, context, EffectTriggerType.OnPlay);
        Debug.Log("Efeito engatilhado e enviado para o ActionSystem!");
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