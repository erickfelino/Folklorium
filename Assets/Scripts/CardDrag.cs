using UnityEngine;
using DG.Tweening; 
using Folklorium;
using static Folklorium.CardData;
using Unity.VisualScripting;

public class CardDrag : MonoBehaviour
{
    private string dropZoneTag;
    private Color highlightColor;
    private GameObject[] validZones; 
    private Color originalZoneColor;
    private Plane dragPlane; 
    private HandManager handManager;
    private TurnManager turnManager;

    [Header("Mana")]
    private ManaManager manaManager;
    private int myManaCost;

    [Header("Configurações de Tabuleiro")]
    private Vector3 handScale; 
    public GameObject[] objectsToHideOnBoard; 
    public GameObject[] objectsToShowOnBoard;
    public bool isPlayed { get; private set; } = false;
    public bool isDragging = false;
    public static bool isAnyCardDragging = false;

    [Header("Efeitos Visuais")]
    public GameObject dragGlow; 
    public bool isHovering = false;

    void Start()
    {
        CardData cardData = GetComponent<CardDisplay>().cardData;
        turnManager = FindFirstObjectByType<TurnManager>();
        myManaCost = cardData.mana;
        SetupCardRules(cardData);

        validZones = GameObject.FindGameObjectsWithTag(dropZoneTag);

        if (validZones.Length > 0)
        {
            originalZoneColor = validZones[0].GetComponent<Renderer>().material.color;
        }

        handScale = transform.localScale;
    }

    private void SetupCardRules(CardData data)
    {
        if (data == null) return;

        switch (data.cardRole)
        {
            case CardRole.Soldier:
                dropZoneTag = "DropZoneSoldier";
                highlightColor = Color.green;
                break;
            case CardRole.Hero:
                dropZoneTag = "DropZoneHero";
                highlightColor = Color.blue;
                break;
            case CardRole.Commander:
                dropZoneTag = "DropZoneCommander";
                highlightColor = Color.red;
                break;
            default:
                dropZoneTag = "Untagged";
                highlightColor = Color.white;
                break;
        }
    }

    void OnMouseEnter()
    {
        if (!isDragging && !isPlayed && !isAnyCardDragging)
        {
            isHovering = true;
            handManager.TriggerHover(this.gameObject); 
        }
    }

    void OnMouseExit()
    {
        if (!isPlayed)
        {
            isHovering = false;
            if (!isDragging) 
            {
                handManager.CancelHoverOrDrag(this.gameObject); 
            }
        }
    }

    void OnMouseDown()
    {
        if (isPlayed) return;
        if (turnManager != null && !turnManager.isPlayerTurn) return;

        if (manaManager != null && !manaManager.HasEnoughMana(myManaCost))
        {
            Debug.Log("Mana Insuficiente!");
            transform.DOShakeRotation(0.3f, Vector3.forward * 15f); 
            return; 
        }

        isDragging = true;
        isAnyCardDragging = true; 
        isHovering = false; 
        if (dragGlow != null) dragGlow.SetActive(true);

        handManager.TriggerDrag(this.gameObject);

        foreach (GameObject zone in validZones)
        {
            if (zone.transform.childCount == 0)
            {
                zone.GetComponent<Renderer>().material.color = highlightColor;
            }
        }

        dragPlane = new Plane(Vector3.up, transform.position);

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

    }

    void OnMouseDrag()
    {
        if (isPlayed || !isDragging) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (dragPlane.Raycast(ray, out float distance))
        {
            Vector3 targetPosition = ray.GetPoint(distance);
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * 25f);
        }
    }

    void OnMouseUp()
    {
        if (isPlayed || !isDragging) return;

        isDragging = false;
        isAnyCardDragging = false; 
        if (dragGlow != null) dragGlow.SetActive(false); 

        GetComponent<Collider>().enabled = false;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        
        RaycastHit[] hits = Physics.RaycastAll(ray);
        bool placedCard = false;

        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.CompareTag(dropZoneTag) && hit.transform.childCount == 0) 
            {
                if (manaManager != null) manaManager.SpendMana(myManaCost);
                if (handManager != null) handManager.RemoveCardFromHand(gameObject);

                TransformIntoTokenAndJump(hit.transform, false);
                
                placedCard = true; 
                break; 
            }
        }

        if (!placedCard)
        {
            ReturnToHand();
        }

        if (!isPlayed) GetComponent<Collider>().enabled = true;

        foreach (GameObject zone in validZones)
        {
            zone.GetComponent<Renderer>().material.color = originalZoneColor;
        }
    }

    private void ReturnToHand()
    {
        if (handManager != null)
        {
            handManager.CancelHoverOrDrag(this.gameObject);
        }
    }

    public void TransformIntoTokenAndJump(Transform targetZone, bool isEnemy)
    {
        isPlayed = true;
        transform.SetParent(targetZone);

        GetComponent<Collider>().enabled = true;

        ResolveOnPlayEffects();

        foreach(GameObject obj in objectsToHideOnBoard) if(obj) obj.SetActive(false);
        foreach(GameObject obj in objectsToShowOnBoard) if(obj) obj.SetActive(true);

        MeshRenderer mesh = GetComponent<MeshRenderer>();
        if(mesh != null) mesh.enabled = false;

        Vector3 finalPos = new Vector3(targetZone.position.x, targetZone.position.y + 0.1f, targetZone.position.z - 0.15f);

        if (isEnemy)
        {
            transform.localRotation = Quaternion.Euler(-89.98f, 0f, 180f);
        }

        Vector3 targetScale = targetZone.localScale;
        float fatorDeCompensacao = 20f; 
        Vector3 finalScale = new Vector3(
            targetScale.x * fatorDeCompensacao, 
            targetScale.z * fatorDeCompensacao, 
            handScale.z                         
        );

        transform.DOKill(); 
        transform.DOJump(finalPos, jumpPower: 0.7f, numJumps: 1, duration: 1.2f).SetEase(Ease.OutQuad);
    }

    private void TriggerOnPlayEffects()
    {
        CardCombat combat = GetComponent<CardCombat>();
        CardData data = GetComponent<CardDisplay>().cardData;

        CardEffectContext context = new CardEffectContext
        {
            source = combat,
            playerHand = handManager,
            isEnemySource = combat.isEnemy
        };

        CardEffectExecutor.ExecuteEffects(
            data,
            context,
            EffectTriggerType.OnPlay
        );
    }

    private void ResolveOnPlayEffects()
    {
        CardData data = GetComponent<CardDisplay>().cardData;
        CardCombat combat = GetComponent<CardCombat>();

        // 👇 NOVAS VARIÁVEIS PARA GUARDAR O PACOTE (MOLDE + DADOS)
        CardEffect effectThatNeedsTarget = null;
        EffectData dataForTarget = null;

        if (data.effects != null)
        {
            // Iteramos sobre a nova classe 'EffectEntry'
            foreach (EffectEntry entry in data.effects)
            {
                // Checamos a entry, o SO e se o gatilho está nela
                if (entry != null && entry.effectSO != null && entry.trigger == EffectTriggerType.OnPlay)
                {
                    // Agora checamos se o SO precisa de alvo
                    if (entry.effectSO.requiresTarget)
                    {
                        effectThatNeedsTarget = entry.effectSO;
                        dataForTarget = entry.parameters; // Guarda os dados também!
                        break; 
                    }
                }
            }
        }

        if (effectThatNeedsTarget != null)
        {
            if (combat.isEnemy)
            {
                OpponentAI ai = FindFirstObjectByType<OpponentAI>();
                if (ai != null)
                {
                    // 👇 ENVIAMOS OS DOIS PARÂMETROS PARA A IA
                    ai.StartCoroutine(ai.ResolveAITargetingCoroutine(combat, data, effectThatNeedsTarget, dataForTarget));
                }
            }
            else
            {
                // 👇 ENVIAMOS OS DOIS PARÂMETROS PARA O PLAYER
                EffectTargetManager.Instance.StartTargeting(combat, data, effectThatNeedsTarget, dataForTarget);
            }
        }
        else
        {
            TriggerOnPlayEffects(); 
        }
    }
    
    public void SetManagers(HandManager hand, ManaManager mana)
    {
        handManager = hand;
        manaManager = mana;
    }
}