using UnityEngine;
using Folklorium; // Para acessar a classe Card
using static Folklorium.Card; // Para acessar os CardRoles (Soldier, Hero, Commander)

[RequireComponent(typeof(CardCombat))]
[RequireComponent(typeof(CardDrag))]
public class CardTargeting : MonoBehaviour
{
    private TargetingArrow arrow;
    private CardCombat myCombat;
    private CardDrag cardDrag;
    private Camera mainCamera;

    private bool isDragging = false;

    void Start()
    {
        myCombat = GetComponent<CardCombat>();
        cardDrag = GetComponent<CardDrag>();
        mainCamera = Camera.main;
        
        arrow = Object.FindFirstObjectByType<TargetingArrow>();
    }

    void OnMouseDown()
    {
        if (myCombat.isEnemy || !myCombat.canAttackThisTurn || !cardDrag.isPlayed) 
        {
            Debug.Log("Esta carta não pode atacar agora.");
            return;
        }

        isDragging = true;
        if (arrow != null) arrow.ShowArrow(true);
    }

    void OnMouseDrag()
    {
        if (!isDragging || arrow == null) return;

        Vector3 startPoint = transform.position;
        Vector3 endPoint = GetMouseWorldPosition();
        arrow.UpdateArrow(startPoint, endPoint);
    }

    void OnMouseUp()
    {
        if (!isDragging) return;
        isDragging = false;

        if (arrow != null) arrow.ShowArrow(false);

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            // O RADAR: Lemos a mesa do inimigo uma vez só antes de perguntar ao juiz
            bool enemyHasSoldiers = CheckIfEnemyHasRole(CardRole.Soldier);
            bool enemyHasHeroes = CheckIfEnemyHasRole(CardRole.Hero);
            bool enemyHasCommanders = CheckIfEnemyHasRole(CardRole.Commander);
            
            CardRole myRole = GetComponent<CardDisplay>().cardData.cardRole;

            if (hit.collider.CompareTag("EnemyHealth"))
            {
                PlayerHealth enemyHealth = hit.collider.GetComponent<PlayerHealth>();
                if (enemyHealth != null)
                {
                    // PERGUNTA AO JUIZ GLOBAL!
                    if (CombatRules.CanAttackPlayer(myRole, enemyHasSoldiers, enemyHasHeroes, enemyHasCommanders))
                    {
                        myCombat.Attack(enemyHealth);
                    }
                    else
                    {
                        Debug.Log("ATAQUE BLOQUEADO pelas regras de combate!");
                    }
                }
            }
            else if (hit.collider.CompareTag("Card"))
            {
                CardCombat targetCard = hit.collider.GetComponent<CardCombat>();
                if (targetCard != null && targetCard.isEnemy)
                {
                    CardRole targetRole = targetCard.GetComponent<CardDisplay>().cardData.cardRole;

                    // PERGUNTA AO JUIZ GLOBAL!
                    if (CombatRules.CanAttackCard(myRole, targetRole, enemyHasSoldiers, enemyHasHeroes, enemyHasCommanders))
                    {
                        myCombat.Attack(targetCard);
                    }
                    else
                    {
                        Debug.Log("ATAQUE BLOQUEADO: Você deve atacar a linha de frente apropriada primeiro!");
                    }
                }
            }
        }
    }

    // =========================================================
    // O NOVO RADAR ANTI-FANTASMAS
    // =========================================================
    private bool CheckIfEnemyHasRole(CardRole roleToCheck)
    {
        // 1. FindObjectsInactive.Exclude garante que ignoramos cartas que foram desativadas (ex: durante a animação de morte)
        CardCombat[] allCards = Object.FindObjectsByType<CardCombat>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        
        foreach (CardCombat card in allCards)
        {
            // 2. Pegamos o CardDrag para saber se a carta já caiu na mesa
            CardDrag drag = card.GetComponent<CardDrag>();
            bool isOnBoard = (drag != null && drag.isPlayed);

            // 3. A carta SÓ conta se for Inimiga, estiver Viva E estiver na Mesa!
            if (card.isEnemy && card.currentLife > 0 && isOnBoard)
            {
                CardRole role = card.GetComponent<CardDisplay>().cardData.cardRole;
                if (role == roleToCheck)
                {
                    // Se o bug acontecer de novo, olhe o Console. Ele vai te dizer exatamente O NOME da carta fantasma!
                    Debug.Log($"[Radar] Bloqueio ativo! O inimigo ainda tem um {roleToCheck} protegendo a mesa: {card.gameObject.name} (Vida: {card.currentLife})");
                    return true;
                }
            }
        }
        return false;
    }

    // ==========================================
    // MÁGICA DE CONVERSÃO
    // ==========================================
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