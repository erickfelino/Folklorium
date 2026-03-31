using UnityEngine;
using DG.Tweening;
using Folklorium;

public class CardDrag : MonoBehaviour
{
    private Plane dragPlane;
    private HandManager handManager;
    private ManaManager manaManager;
    private CardPlayController playController;

    private int myManaCost;

    public bool isPlayed { get; private set; } = false;
    public bool isDragging = false;
    public static bool isAnyCardDragging = false;

    [Header("Efeitos Visuais")]
    public GameObject dragGlow;
    public bool isHovering = false;

    public HandManager HandManagerRef => handManager;
    public ManaManager ManaManagerRef => manaManager;
    public int ManaCost => myManaCost;
    public bool IsPlayed => isPlayed;

    void Start()
    {
        CardData cardData = GetComponent<CardDisplay>().cardData;
        myManaCost = cardData.mana;
        playController = CardPlayController.Instance != null ? CardPlayController.Instance : FindFirstObjectByType<CardPlayController>();
    }

    void Update()
    {
        if (isPlayed || isDragging) return;

        bool shouldGlow = playController != null && playController.CanBeginDrag(this);

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
            handManager?.TriggerHover(this.gameObject);
        }
    }

    void OnMouseExit()
    {
        if (!isPlayed)
        {
            isHovering = false;
            if (!isDragging)
            {
                handManager?.CancelHoverOrDrag(this.gameObject);
            }
        }
    }

    void OnMouseDown()
    {
        if (isPlayed) return;

        if (playController != null && !playController.CanBeginDrag(this))
        {
            playController.PlayDeniedFeedback(this);
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

        handManager?.TriggerDrag(this.gameObject);
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

        foreach (RaycastHit hit in hits)
        {
            BoardSlot slot = hit.collider.GetComponentInParent<BoardSlot>();
            if (slot == null) continue;

            if (playController != null && playController.TryPlayCard(this, slot))
            {
                placedCard = true;
                break;
            }
        }

        if (!placedCard)
        {
            ReturnToHand();
        }

        if (!isPlayed)
            GetComponent<Collider>().enabled = true;
    }

    private void ReturnToHand()
    {
        handManager?.CancelHoverOrDrag(this.gameObject);
    }

    public void MarkAsPlayed()
    {
        isPlayed = true;
    }

    public void ShakeInvalidPlay()
    {
        transform.DOShakeRotation(0.3f, Vector3.forward * 15f);
    }

    public void TransformIntoTokenAndJump(BoardSlot targetSlot, bool isEnemy)
    {
        CardBoardView view = GetComponent<CardBoardView>();
        if (view != null)
        {
            view.PlayPlacementAnimation(targetSlot, isEnemy);
        }
        else
        {
            MarkAsPlayed();
            transform.SetParent(targetSlot.transform);
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