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
        myManaCost = cardData.mana;
        SetupCardRules(cardData);

        validZones = GameObject.FindGameObjectsWithTag(dropZoneTag);

        if (validZones.Length > 0)
        {
            originalZoneColor = validZones[0].GetComponent<Renderer>().material.color;
        }

        handScale = transform.localScale;
    }
    void Update()
    {
        // Se a carta já está na mesa ou está sendo segurada pelo mouse, ignoramos a checagem automática
        if (isPlayed || isDragging) return;

        // Checamos as duas condições mágicas para a carta poder ser jogada:
        bool isMyTurn = turnManager != null && turnManager.isPlayerTurn;
        bool hasMana = manaManager != null && manaManager.HasEnoughMana(myManaCost);
        
        // A carta deve brilhar se for o nosso turno E tivermos mana
        bool shouldGlow = isMyTurn && hasMana;

        // Ativa ou desativa o glow apenas se o estado atual for diferente do que deveria ser (poupa processamento)
        if (dragGlow != null && dragGlow.activeSelf != shouldGlow)
        {
            dragGlow.SetActive(shouldGlow);
            dragGlow.GetComponent<Renderer>().material.color = Color.green;
        }
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
        if (dragGlow != null)
        {
            dragGlow.SetActive(true);
            dragGlow.GetComponent<Renderer>().material.color = Color.blue;
        }

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

        // 👇 Pega o BoxCollider principal e ajusta para o tamanho do Token
        BoxCollider boxCol = GetComponent<BoxCollider>();
        if (boxCol != null)
        {
            boxCol.enabled = true;
            boxCol.center = tokenColliderCenter; // Sobe o collider para a arte
            boxCol.size = tokenColliderSize;     // Encolhe para o tamanho da arte
        }

        if (dragGlow != null)
        {
            // Move o centro do glow para o mesmo centro do collider (focado na arte)
            dragGlow.transform.localPosition = tokenColliderCenter;
            
            // Multiplicamos por 1.05f para ele ficar uma "bordinha" um pouco maior que a carta
            dragGlow.transform.localScale = new Vector3(
                tokenColliderSize.x * 1.05f, 
                tokenColliderSize.y * 1.05f, 
                tokenColliderSize.z
            );
        }

        ResolveOnPlayEffects();

        foreach(GameObject obj in objectsToHideOnBoard) if(obj) obj.SetActive(false);
        foreach(GameObject obj in objectsToShowOnBoard) if(obj) obj.SetActive(true);

        MeshRenderer mesh = GetComponent<MeshRenderer>();
        if(mesh != null) mesh.enabled = false;

        Vector3 finalPos = new Vector3(targetZone.position.x, targetZone.position.y + 0.1f, targetZone.position.z - 0.15f);

        if (isEnemy)
        {
            // Rotação do lacaio inimigo (virado para você)
            transform.localRotation = Quaternion.Euler(-90f, 0f, 180f);
        }
        else
        {
            // 👇 CORREÇÃO: Força o lacaio do jogador a deitar corretamente com a arte para cima!
            transform.localRotation = Quaternion.Euler(-90f, 0f, 0f); 
        }

        // --- 👇 NOVA LÓGICA DE ESCALA AQUI 👇 ---

        // Pega a escala da zona (assumo que seja ~1,1,1)
        Vector3 targetScale = targetZone.localScale;
        
        // 👇 O FATOR PERFEITO AQUI: Aumentamos de 20 para 50 para preencher a zona!
        float fatorDeCompensacao = 50f; 

        // 👇 ESPESSURA FORÇADA: Usamos um número pequeno e fixo para a carta não ficar grossa
        // Isso impede que os "191.5" voltem a assombrar.
        float espessuraValue = 0.5f;

        // Montamos a escala final respeitando a rotação (-90 X) da carta
        Vector3 finalScale = new Vector3(
            targetScale.x * fatorDeCompensacao, // Largura (X) vira ~50 (tamanho visual)
            targetScale.z * fatorDeCompensacao, // 👇 Comprimento visual (Z original vira Y visual) vira ~50
            espessuraValue // 👇 Espessura real (Y original vira Z visual) vira 0.5f FIXO E FINO!
        );

        // --- ☝️ FIM DA NOVA LÓGICA ☝️ ---

        // O GRANDE SALTO DO DOTWEEN
        transform.DOKill(); 
        
        // 👇 ESTA LINHA PRECISA ESTAR DESCOMENTADA!
        // Ela é quem faz a carta mudar de tamanho para o tamanho final correto durante o pulo
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

    // Método burro e limpo. Ele não sabe as regras do jogo, só pinta a cor.
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