using UnityEngine;

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
        
        // Encontra o Manager da seta na cena.
        // Como é um elemento único de UI, FindFirstObjectByType é seguro e rápido.
        arrow = Object.FindFirstObjectByType<TargetingArrow>();
    }

    void OnMouseDown()
    {
        // Segurança: Só podemos desenhar a seta se a carta for NOSSA e não estiver exausta.
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

        // Ponto A: O centro da nossa carta
        Vector3 startPoint = transform.position;

        // Ponto B: A posição do mouse convertida para o mundo 3D
        Vector3 endPoint = GetMouseWorldPosition();

        // Manda o Ator Visual desenhar!
        arrow.UpdateArrow(startPoint, endPoint);
    }

    void OnMouseUp()
    {
        if (!isDragging) return;
        isDragging = false;

        // Esconde a seta assim que soltamos o clique
        if (arrow != null) arrow.ShowArrow(false);

        // Dispara um "raio laser" do mouse para dentro da tela para ver em quem soltamos
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            // 1. PRIMEIRO TESTE: O raio bateu na vida do inimigo?
            if (hit.collider.CompareTag("EnemyHealth"))
            {
                PlayerHealth enemyHealth = hit.collider.GetComponent<PlayerHealth>();
                
                if (enemyHealth != null)
                {
                    myCombat.Attack(enemyHealth);
                }
            }
            else if (hit.collider.CompareTag("Card"))
            {
                CardCombat targetCard = hit.collider.GetComponent<CardCombat>();
                
                // Se for uma carta E for uma carta inimiga... PORRADA!
                if (targetCard != null && targetCard.isEnemy)
                {
                    myCombat.Attack(targetCard);
                    Debug.Log("Atacou uma tropa inimiga!");
                }
                else
                {
                    Debug.Log("Alvo inválido. Você não pode atacar suas próprias tropas.");
                }
            }
            else
            {
                Debug.Log("Alvo inválido. Você soltou a seta no vazio.");
            }
        }
    }
    
    private Vector3 GetMouseWorldPosition()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        
        // Criamos um "chão invisível" infinito na exata altura da carta (eixo Y)
        Plane groundPlane = new Plane(Vector3.up, transform.position); 
        
        // Calculamos onde o raio do mouse cruza com esse chão invisível
        if (groundPlane.Raycast(ray, out float distance))
        {
            return ray.GetPoint(distance); // Retorna a coordenada 3D exata!
        }
        
        return transform.position; // Fallback de segurança
    }
}