using UnityEngine;
using DG.Tweening; // Coloquei aqui por precaução, caso você queira animar coisas extras no futuro!
using Folklorium;
using static Folklorium.Card;
using Unity.VisualScripting;

public class CardDrag : MonoBehaviour
{
    private string dropZoneTag;
    private Color highlightColor;
    private GameObject[] validZones; 
    private Color originalZoneColor;
    private Plane dragPlane; 
    private HandManager handManager;

    [Header("Mana")]
    private ManaManager manaManager;
    private int myManaCost;

    [Header("Configurações de Tabuleiro")]
    private Vector3 handScale; 
    public GameObject[] objectsToHideOnBoard; 
    public GameObject[] objectsToShowOnBoard;
    private bool isPlayed = false;
    public bool isDragging = false;

    [Header("Efeitos Visuais")]
    public GameObject dragGlow; 
    public bool isHovering = false;

    void Start()
    {
        Card cardData = GetComponent<CardDisplay>().cardData;
        myManaCost = cardData.mana;
        SetupCardRules(cardData);

        validZones = GameObject.FindGameObjectsWithTag(dropZoneTag);

        if (validZones.Length > 0)
        {
            originalZoneColor = validZones[0].GetComponent<Renderer>().material.color;
        }

        handScale = transform.localScale;
    }

    private void SetupCardRules(Card data)
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

    // ==========================================
    // GATILHOS DO MOUSE COM DOTWEEN
    // ==========================================

    void OnMouseEnter()
    {
        if (!isDragging && !isPlayed)
        {
            isHovering = true;
            // AVISA O HAND MANAGER PRA FAZER A CARTA SALTAR E CRESCER
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
                // AVISA O HAND MANAGER PRA DEVOLVER A CARTA PRO LEQUE
                handManager.CancelHoverOrDrag(this.gameObject); 
            }
        }
    }

    void OnMouseDown()
    {
        if (isPlayed) return;

        if (manaManager != null && !manaManager.HasEnoughMana(myManaCost))
        {
            Debug.Log("Mana Insuficiente!");
            // A carta dá uma tremidinha balançando a cabeça dizendo "não"
            transform.DOShakeRotation(0.3f, Vector3.forward * 15f); 
            return; // Bloqueia todo o resto do código, a carta não sai do lugar!
        }

        isDragging = true;
        isHovering = false; 
        if (dragGlow != null) dragGlow.SetActive(true); 

        // AVISA O HAND MANAGER PRA ENCOLHER A CARTA RAPIDINHO
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
            // Mantemos o LERP aqui porque ele é o mestre de seguir o mouse em tempo real!
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * 25f);
        }
    }

    void OnMouseUp()
    {
        if (isPlayed || !isDragging) return;

        isDragging = false;
        if (dragGlow != null) dragGlow.SetActive(false); 

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        GetComponent<Collider>().enabled = false;

        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider.CompareTag(dropZoneTag) && hit.transform.childCount == 0) 
            {
                if (manaManager != null) manaManager.SpendMana(myManaCost);
                // 1. MARCA COMO JOGADA E TIRA DA MÃO (Isso evita bugs)
                isPlayed = true;
                if (handManager != null)
                {
                    handManager.RemoveCardFromHand(gameObject);
                }
                transform.SetParent(hit.transform);

                // 2. ESCONDE A ARTE/TEXTOS PARA VIRAR TOKEN
                foreach(GameObject obj in objectsToHideOnBoard)
                {
                    if(obj != null) obj.SetActive(false);
                }

                foreach(GameObject obj in objectsToShowOnBoard)
                {
                    if(obj != null) obj.SetActive(true);
                }
                GetComponent<MeshRenderer>().enabled = false; 

                // 3. CALCULA O DESTINO E O TAMANHO DO TOKEN
                Vector3 finalPos = new Vector3(hit.transform.position.x, hit.transform.position.y + 0.1f, hit.transform.position.z - 0.15f);
                Vector3 targetScale = hit.transform.localScale;
                float fatorDeCompensacao = 20f; 
                Vector3 finalScale = new Vector3(
                    targetScale.x * fatorDeCompensacao, 
                    targetScale.z * fatorDeCompensacao, 
                    handScale.z                         
                );

                // ==========================================
                // 4. A MÁGICA DO DOTWEEN (O PULO DO TOKEN)
                // ==========================================
                transform.DOKill(); // Mata o lerp do drag

                // O GRANDE SALTO: Vai até a finalPos, subindo 2 metros de altura, 1 vez, durando 0.4 segundos
                transform.DOJump(finalPos, jumpPower: 0.7f, numJumps: 1, duration: 1.2f).SetEase(Ease.OutQuad);
            }
            else
            {
                ReturnToHand();
            }
        }
        else
        {
            ReturnToHand();
        }

        GetComponent<Collider>().enabled = true;

        foreach (GameObject zone in validZones)
        {
            zone.GetComponent<Renderer>().material.color = originalZoneColor;
        }
    }

    // Atualizei esse método para usar a mágica do DOTween!
    private void ReturnToHand()
    {
        if (handManager != null)
        {
            // Em vez de teletransportar duro de volta, a carta vai voar suavemente 
            // de volta pro exato espaço dela no leque graças à nossa função de cancelar do HandManager!
            handManager.CancelHoverOrDrag(this.gameObject);
        }
    }
    public void SetManagers(HandManager hand, ManaManager mana)
    {
        handManager = hand;
        manaManager = mana;
    }
}