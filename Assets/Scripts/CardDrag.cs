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
        Card cardData = GetComponent<CardDisplay>().cardData;
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

    void OnMouseEnter()
    {
        // Adicionamos a checagem do !isAnyCardDragging aqui!
        if (!isDragging && !isPlayed && !isAnyCardDragging)
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
        if (turnManager != null && !turnManager.isPlayerTurn) return;

        if (manaManager != null && !manaManager.HasEnoughMana(myManaCost))
        {
            Debug.Log("Mana Insuficiente!");
            // A carta dá uma tremidinha balançando a cabeça dizendo "não"
            transform.DOShakeRotation(0.3f, Vector3.forward * 15f); 
            return; // Bloqueia todo o resto do código, a carta não sai do lugar!
        }

        isDragging = true;
        isAnyCardDragging = true; // TRAVA O HOVER DAS OUTRAS CARTAS!
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
        isAnyCardDragging = false; // DESTRAVA O HOVER DAS OUTRAS CARTAS!
        if (dragGlow != null) dragGlow.SetActive(false); 

        // Desliga o collider desta carta temporariamente para o raio não bater nela mesma
        GetComponent<Collider>().enabled = false;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        
        // A MÁGICA ACONTECE AQUI: RaycastAll pega TUDO que estiver embaixo do mouse
        RaycastHit[] hits = Physics.RaycastAll(ray);
        bool placedCard = false;

        // Vamos checar tudo o que o raio furou
        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.CompareTag(dropZoneTag) && hit.transform.childCount == 0) 
            {
                // 1. Paga o custo e tira da mão
                if (manaManager != null) manaManager.SpendMana(myManaCost);
                if (handManager != null) handManager.RemoveCardFromHand(gameObject);

                // 2. Chama a animação unificada! (false = não é o inimigo)
                TransformIntoTokenAndJump(hit.transform, false);
                
                placedCard = true; // Avisa que conseguimos jogar a carta
                break; // Para de procurar, já achamos a zona!
            }
        }

        // Se olhamos tudo embaixo do mouse e não tinha nenhuma zona válida vazia...
        if (!placedCard)
        {
            ReturnToHand();
        }

        // Reativa o collider se ela voltou pra mão
        if (!isPlayed) GetComponent<Collider>().enabled = true;

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

    public void TransformIntoTokenAndJump(Transform targetZone, bool isEnemy)
    {
        isPlayed = true;
        transform.SetParent(targetZone);

        // Dentro de TransformIntoTokenAndJump...
        GetComponent<Collider>().enabled = true;

        // 👇 Troque o TriggerOnPlayEffects() antigo por este aqui:
        ResolveOnPlayEffects();

        // Troca os visuais
        foreach(GameObject obj in objectsToHideOnBoard) if(obj) obj.SetActive(false);
        foreach(GameObject obj in objectsToShowOnBoard) if(obj) obj.SetActive(true);

        MeshRenderer mesh = GetComponent<MeshRenderer>();
        if(mesh != null) mesh.enabled = false;

        // Calcula a posição do centro da zona
        Vector3 finalPos = new Vector3(targetZone.position.x, targetZone.position.y + 0.1f, targetZone.position.z - 0.15f);

        // Se for o oponente jogando, a carta precisa virar 180 graus pra te encarar
        if (isEnemy)
        {
            transform.localRotation = Quaternion.Euler(-89.98f, 0f, 180f);
        }

        // Calcula aquele Scale que você tinha feito antes!
        Vector3 targetScale = targetZone.localScale;
        float fatorDeCompensacao = 20f; 
        Vector3 finalScale = new Vector3(
            targetScale.x * fatorDeCompensacao, 
            targetScale.z * fatorDeCompensacao, 
            handScale.z                         
        );

        // O GRANDE SALTO DO DOTWEEN (Posição e Escala juntos!)
        transform.DOKill(); 
        transform.DOJump(finalPos, jumpPower: 0.7f, numJumps: 1, duration: 1.2f).SetEase(Ease.OutQuad);
        //transform.DOScale(finalScale, 1.2f).SetEase(Ease.OutQuad); Se precisar mudar o tamanho dela ao cair no board no futuro
    }

    private void TriggerOnPlayEffects()
    {
        CardCombat combat = GetComponent<CardCombat>();
        Card data = GetComponent<CardDisplay>().cardData;

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
        Card data = GetComponent<CardDisplay>().cardData;
        CardCombat combat = GetComponent<CardCombat>();

        CardEffect effectThatNeedsTarget = null;

        if (data.effects != null)
        {
            foreach (var effect in data.effects)
            {
                if (effect != null && effect.trigger == EffectTriggerType.OnPlay && effect.requiresTarget)
                {
                    effectThatNeedsTarget = effect;
                    break; 
                }
            }
        }

        if (effectThatNeedsTarget != null)
        {
            // A BIFURCAÇÃO MÁGICA
            if (combat.isEnemy)
            {
                OpponentAI ai = FindFirstObjectByType<OpponentAI>();
                if (ai != null)
                {
                    // 👇 ATUALIZE ESTA LINHA: Agora iniciamos a Coroutine!
                    ai.StartCoroutine(ai.ResolveAITargetingCoroutine(combat, data, effectThatNeedsTarget));
                }
            }
            else
            {
                // Se é o JOGADOR, acende a Seta Azul e espera o clique!
                EffectTargetManager.Instance.StartTargeting(combat, data, effectThatNeedsTarget);
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