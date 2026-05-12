using UnityEngine;
using Folklorium;

public class EffectTargetManager : MonoBehaviour
{
    private static EffectTargetManager _instance;

    public static EffectTargetManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<EffectTargetManager>();
            }
            return _instance;
        }
    }

    [SerializeField] private TargetingArrow arrow;
    private Camera mainCamera;

    public bool isWaitingForTarget = false;
    
    private IEffectSource currentSource;
    private CardEffect currentPendingEffect; 
    private EffectData currentPendingData; 

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
    }

    void Start()
    {
        mainCamera = Camera.main;
    }

    public void StartTargeting(IEffectSource source, CardData cardData, CardEffect effect, EffectData data)
    {
        // =========================================================
        // RADAR INTELIGENTE
        // =========================================================
        PlayerHealth enemyHealth = GameObject.FindGameObjectWithTag("EnemyHealth")?.GetComponent<PlayerHealth>();
        
        bool canTargetEnemyPlayer = enemyHealth != null && effect.IsValidTarget(source, null, enemyHealth, data);
        bool hasValidCardTarget = false;
        
        CardCombat[] allCards = FindObjectsByType<CardCombat>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        
        foreach (var c in allCards)
        {
            if (c != null && c.GetComponent<CardDrag>() != null && c.GetComponent<CardDrag>().isPlayed && c.currentLife > 0)
            {
                if (effect.IsValidTarget(source, c, null, data))
                {
                    hasValidCardTarget = true;
                    break; // Achou pelo menos 1, já pode parar de procurar!
                }
            }
        }

        // 👇 NOVO: TRAVA NÍVEL 4 (O SALVA-VIDAS)
        // Se NÃO pode bater na torre E NÃO tem lacaio válido... Cancela tudo e libera o jogo!
        if (!canTargetEnemyPlayer && !hasValidCardTarget)
        {
            Debug.Log($"[Radar]: A carta {cardData.cardName} foi jogada, mas não há NENHUM alvo válido. Resolvendo sem efeito.");
            // Não puxa a seta e não tranca o turno. O jogo segue livre!
            return; 
        }

        // Se pode bater na torre E NÃO PODE bater em mais nada... AUTO-FIRE!
        if (canTargetEnemyPlayer && !hasValidCardTarget)
        {
            Debug.Log($"Auto-Target: {cardData.cardName} atirou direto na Torre!");
            
            // Se for magia, finaliza pelo SpellSlot para gastar mana
            if (source is SpellSlot spell)
            {
                spell.ResolveSpellEffects(null, enemyHealth);
            }
            else // Se for criatura (CardCombat), segue o fluxo normal
            {
                CardEffectContext context = new CardEffectContext { source = source, targetPlayer = enemyHealth };
                GameAction action = effect.CreateAction(context, data);
                if (action != null) ActionSystem.Instance.AddAction(action);
            }
            return;
        }
        // =========================================================

        TurnManager.LockTurn(); // 🔒 TRANCA A PORTA

        currentSource = source;
        currentPendingEffect = effect;
        currentPendingData = data;

        isWaitingForTarget = true;
        if (arrow != null)
        {
            arrow.SetColor(Color.cyan);
            arrow.ShowArrow(true);
            arrow.UpdateArrow(source.EffectTransform.position, source.EffectTransform.position);
        }

        NotifyBoardOfEffectTargetingState(true);
    }

    void Update()
    {
        if (!isWaitingForTarget) return;

        if (arrow != null)
        {
            Vector3 endPoint = GetMouseWorldPosition();
            arrow.UpdateArrow(currentSource.EffectTransform.position, endPoint);
        }

        // Botão Esquerdo: Confirma o alvo
        if (Input.GetMouseButtonDown(0))
        {
            TrySelectTarget();
        }
        // 👇 NOVO: Botão Direito do Mouse: Cancela a mira!
        else if (Input.GetMouseButtonDown(1))
        {
            CancelTargeting();
        }
    }

    private void TrySelectTarget()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            CardCombat targetCard = hit.collider.GetComponent<CardCombat>();
            PlayerHealth targetPlayer = hit.collider.GetComponent<PlayerHealth>();

            if (currentPendingEffect.IsValidTarget(currentSource, targetCard, targetPlayer, currentPendingData))
            {
                CardEffectContext context = new CardEffectContext
                {
                    source = currentSource,
                    targetCard = targetCard,
                    targetPlayer = targetPlayer,
                    
                };
                
                ExecuteAndFinish(context); 
            }
            else
            {
                Debug.Log("Alvo bloqueado pelas regras do Efeito!");
            }
        }
    }

    // 👇 NOVO: Método para limpar a sujeira caso o jogador cancele com botão direito
    private void CancelTargeting()
    {
        Debug.Log("Jogador cancelou a mira do efeito.");
        
        isWaitingForTarget = false;
        if (arrow != null) arrow.ShowArrow(false);

        NotifyBoardOfEffectTargetingState(false);

        // Limpa a memória
        currentSource = null; 
        currentPendingEffect = null;
        currentPendingData = null;

        TurnManager.UnlockTurn(); // 🔓 DESTRAVA A PORTA!
    }

    // =========================================================
    // RESOLUÇÃO DE ALVO ALEATÓRIO
    // =========================================================
    public void ResolveRandomTarget(IEffectSource source, CardEffect effect, EffectData data)
    {
        System.Collections.Generic.List<CardCombat> validCards = new System.Collections.Generic.List<CardCombat>();
        System.Collections.Generic.List<PlayerHealth> validPlayers = new System.Collections.Generic.List<PlayerHealth>();

        // 1. Vasculha os Jogadores (Heróis/Torres)
        PlayerHealth[] players = FindObjectsByType<PlayerHealth>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        foreach (var p in players)
        {
            if (effect.IsValidTarget(source, null, p, data))
            {
                validPlayers.Add(p);
            }
        }

        // 2. Vasculha as Cartas na Mesa
        CardCombat[] allCards = FindObjectsByType<CardCombat>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        foreach (var c in allCards)
        {
            if (c != null && c.GetComponent<CardDrag>() != null && c.GetComponent<CardDrag>().isPlayed && c.currentLife > 0)
            {
                if (effect.IsValidTarget(source, c, null, data))
                {
                    validCards.Add(c);
                }
            }
        }

        int totalTargets = validCards.Count + validPlayers.Count;

        // Se não tem ninguém válido na mesa, o efeito não faz nada
        if (totalTargets == 0)
        {
            Debug.Log($"[Aleatório]: Efeito ativou, mas não havia alvos válidos na mesa!");
            return; 
        }

        // Sorteia um número entre 0 e o total de alvos
        int randomIndex = UnityEngine.Random.Range(0, totalTargets);

        CardEffectContext context = new CardEffectContext
        {
            source = source,
        };

        // Descobre se o número sorteado caiu na lista de Cartas ou de Jogadores
        if (randomIndex < validCards.Count)
        {
            context.targetCard = validCards[randomIndex];
            Debug.Log($"[Aleatório]: O alvo sorteado foi a carta: {context.targetCard.name}");
        }
        else
        {
            context.targetPlayer = validPlayers[randomIndex - validCards.Count];
            Debug.Log($"[Aleatório]: O alvo sorteado foi o jogador: {context.targetPlayer.name}");
        }

        // Cria a ação e manda pro ActionSystem
        GameAction action = effect.CreateAction(context, data);
        if (action != null)
        {
            ActionSystem.Instance.AddAction(action);
        }
    }

    private void ExecuteAndFinish(CardEffectContext context)
    {
        isWaitingForTarget = false;
        if (arrow != null) arrow.ShowArrow(false);
        NotifyBoardOfEffectTargetingState(false);

        // VERIFICAÇÃO DE QUEM É A FONTE
        if (currentSource is SpellSlot spell)
        {
            // Se for magia, o SpellSlot assume o comando para gastar mana e rodar TODOS os efeitos
            spell.ResolveSpellEffects(context.targetCard, context.targetPlayer);
        }
        else
        {
            // Se for uma criatura (CardCombat), a mana já foi gasta ao jogar a carta,
            // então apenas executamos a ação do efeito específico (Battlecry).
            if (currentPendingEffect != null)
            {
                GameAction action = currentPendingEffect.CreateAction(context, currentPendingData);
                if (action != null) ActionSystem.Instance.AddAction(action);
            }
        }

        // Limpeza padrão
        currentSource = null;  
        currentPendingEffect = null;
        currentPendingData = null;
        TurnManager.UnlockTurn(); 
    }

    // =========================================================
    // FEEDBACK VISUAL (GLOW) PARA EFEITOS
    // =========================================================
    private void NotifyBoardOfEffectTargetingState(bool isTargetingMode)
    {
        // ==========================================
        // 1. LÓGICA DA TORRE INIMIGA (TargetGlow filho)
        // ==========================================
        PlayerHealth playerHealth = GameObject.FindGameObjectWithTag("PlayerHealth")?.GetComponent<PlayerHealth>();
        PlayerHealth enemyHealth = GameObject.FindGameObjectWithTag("EnemyHealth")?.GetComponent<PlayerHealth>();
        if (playerHealth != null && enemyHealth != null)
        {
            // Busca o GameObject filho chamado "TargetGlow"
            Transform towerPlayerGlowObject = playerHealth.transform.Find("TargetGlow"); 
            Transform towerEnemyGlowObject = enemyHealth.transform.Find("TargetGlow");
            
            if (towerPlayerGlowObject != null && towerEnemyGlowObject != null)
            {
                if (isTargetingMode)
                {
                    // Pergunta ao efeito se a torre é um alvo válido
                    bool isValidEnemyTowerTarget = currentPendingEffect.IsValidTarget(currentSource, null, enemyHealth, currentPendingData);
                    bool isValidPlayerTowerTarget = currentPendingEffect.IsValidTarget(currentSource, null, playerHealth, currentPendingData);
                    
                    // Liga o brilho APENAS se a torre for um alvo válido
                    towerPlayerGlowObject.gameObject.SetActive(isValidPlayerTowerTarget);
                    towerEnemyGlowObject.gameObject.SetActive(isValidEnemyTowerTarget);
                }
                else
                {
                    // Seta solta/cancelada: desliga o brilho da torre
                    towerPlayerGlowObject.gameObject.SetActive(false);
                    towerEnemyGlowObject.gameObject.SetActive(false);
                }
            }
        }

        // ==========================================
        // 2. LÓGICA DAS CARTAS NA MESA
        // ==========================================
        CardCombat[] allCards = Object.FindObjectsByType<CardCombat>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        
        foreach (CardCombat card in allCards)
        {
            CardDrag drag = card.GetComponent<CardDrag>();
            if (drag != null && drag.isPlayed)
            {
                if (isTargetingMode)
                {
                    bool isValidTarget = currentPendingEffect.IsValidTarget(currentSource, card, null, currentPendingData);

                    if (currentSource is CardCombat sourceCard && card == sourceCard)
                    {
                        card.RefreshGlowState(false, false); 
                        continue; 
                    }

                    card.RefreshGlowState(true, isValidTarget);
                }
                else
                {
                    card.RefreshGlowState(false, false);
                }
            }
        }
    }

    private Vector3 GetMouseWorldPosition()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, currentSource.EffectTransform.position); 
        if (groundPlane.Raycast(ray, out float distance)) return ray.GetPoint(distance); 
        return currentSource.EffectTransform.position; 
    }
}