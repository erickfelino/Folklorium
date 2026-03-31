using UnityEngine;
using DG.Tweening;
using Folklorium;

public class CardDrag : MonoBehaviour
{
    private Plane dragPlane;
    private HandManager handManager;
    private TurnManager turnManager;
    private BoardManager boardManager;

    [Header("Mana")]
    private ManaManager manaManager;
    private int myManaCost;

    [Header("Ajuste de Collider no Tabuleiro")]
    [Tooltip("Centro do collider quando vira Token (para focar na arte)")]
    public Vector3 tokenColliderCenter = new Vector3(0f, 0.25f, 0f);

    [Tooltip("Tamanho do collider quando vira Token")]
    public Vector3 tokenColliderSize = new Vector3(1f, 0.5f, 0.2f);

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
        boardManager = BoardManager.Instance != null ? BoardManager.Instance : FindFirstObjectByType<BoardManager>();

        myManaCost = cardData.mana;
        handScale = transform.localScale;
    }

    void Update()
    {
        if (isPlayed || isDragging) return;

        bool isMyTurn = turnManager != null && turnManager.IsPlayerTurn;
        bool hasMana = manaManager != null && manaManager.HasEnoughMana(myManaCost);
        bool shouldGlow = isMyTurn && hasMana;

        if (dragGlow != null && dragGlow.activeSelf != shouldGlow)
        {
            dragGlow.SetActive(shouldGlow);
            dragGlow.GetComponent<Renderer>().material.color = Color.green;
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
        if (turnManager != null && !turnManager.IsPlayerTurn) return;

        if (manaManager != null && !manaManager.HasEnoughMana(myManaCost))
        {
            Debug.Log("Mana Insuficiente!");
            transform.DOShakeRotation(0.3f, Vector3.forward * 15f);
            return;
        }

        isDragging = true;
        isAnyCardDragging = true;
        isHovering = false;

        if (dragGlow != null)
        {
            dragGlow.SetActive(true);
            dragGlow.GetComponent<Renderer>().material.color = Color.blue;
        }

        handManager.TriggerDrag(this.gameObject);
        dragPlane = new Plane(Vector3.up, transform.position);
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

        if (dragGlow != null)
        {
            dragGlow.SetActive(false);
        }

        GetComponent<Collider>().enabled = false;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit[] hits = Physics.RaycastAll(ray);

        bool placedCard = false;
        CardCombat combat = GetComponent<CardCombat>();

        foreach (RaycastHit hit in hits)
        {
            BoardSlot slot = hit.collider.GetComponentInParent<BoardSlot>();
            if (slot == null) continue;

            if (boardManager != null && boardManager.TryPlaceCard(combat, slot))
            {
                if (manaManager != null) manaManager.SpendMana(myManaCost);
                if (handManager != null) handManager.RemoveCardFromHand(gameObject);

                TransformIntoTokenAndJump(slot, false);
                placedCard = true;
                break;
            }
        }

        if (!placedCard)
        {
            ReturnToHand();
        }

        if (!isPlayed) GetComponent<Collider>().enabled = true;
    }

    private void ReturnToHand()
    {
        if (handManager != null)
        {
            handManager.CancelHoverOrDrag(this.gameObject);
        }
    }

    public void TransformIntoTokenAndJump(BoardSlot targetSlot, bool isEnemy)
    {
        isPlayed = true;
        transform.SetParent(targetSlot.transform);

        BoxCollider boxCol = GetComponent<BoxCollider>();
        if (boxCol != null)
        {
            boxCol.enabled = true;
            boxCol.center = tokenColliderCenter;
            boxCol.size = tokenColliderSize;
        }

        if (dragGlow != null)
        {
            dragGlow.transform.localPosition = tokenColliderCenter;
            dragGlow.transform.localScale = new Vector3(
                tokenColliderSize.x * 1.05f,
                tokenColliderSize.y * 1.05f,
                tokenColliderSize.z
            );
        }

        ResolveOnPlayEffects();

        foreach (GameObject obj in objectsToHideOnBoard) if (obj) obj.SetActive(false);
        foreach (GameObject obj in objectsToShowOnBoard) if (obj) obj.SetActive(true);

        MeshRenderer mesh = GetComponent<MeshRenderer>();
        if (mesh != null) mesh.enabled = false;

        Vector3 finalPos = new Vector3(
            targetSlot.transform.position.x,
            targetSlot.transform.position.y + 0.1f,
            targetSlot.transform.position.z - 0.15f
        );

        if (isEnemy)
        {
            transform.localRotation = Quaternion.Euler(-90f, 0f, 180f);
        }
        else
        {
            transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);
        }

        Vector3 targetScale = targetSlot.transform.localScale;
        float fatorDeCompensacao = 50f;
        float espessuraValue = 0.5f;

        Vector3 finalScale = new Vector3(
            targetScale.x * fatorDeCompensacao,
            targetScale.z * fatorDeCompensacao,
            espessuraValue
        );

        transform.DOKill();
        transform.DOScale(finalScale, 1.2f).SetEase(Ease.OutQuad);
        transform.DOJump(finalPos, jumpPower: 0.7f, numJumps: 1, duration: 1f).SetEase(Ease.OutQuad);
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

        CardEffect effectThatNeedsTarget = null;
        EffectData dataForTarget = null;

        if (data.effects != null)
        {
            foreach (EffectEntry entry in data.effects)
            {
                if (entry != null && entry.effectSO != null && entry.trigger == EffectTriggerType.OnPlay)
                {
                    if (entry.effectSO.requiresTarget)
                    {
                        effectThatNeedsTarget = entry.effectSO;
                        dataForTarget = entry.parameters;
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
                    ai.StartCoroutine(ai.ResolveAITargetingCoroutine(combat, data, effectThatNeedsTarget, dataForTarget));
                }
            }
            else
            {
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

    public void SetGlow(bool active, Color color)
    {
        if (dragGlow != null)
        {
            dragGlow.SetActive(active);
            if (active)
            {
                dragGlow.GetComponent<Renderer>().material.color = color;
            }
        }
    }
}