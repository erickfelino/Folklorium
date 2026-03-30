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
    
    private CardCombat currentSource;
    private CardData currentCardData;
    private CardEffect currentPendingEffect; 
    
    // 👇 NOVA VARIÁVEL: Guarda o pacote de dados enquanto o jogador mira
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

    // 👇 RECEBE O 'EffectData' AQUI TAMBÉM
    public void StartTargeting(CardCombat source, CardData cardData, CardEffect effect, EffectData data)
    {
        // =========================================================
        // RADAR INTELIGENTE (Auto-Fire na Torre)
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
                    break;
                }
            }
        }

        // Se pode bater na torre E NÃO PODE bater em mais nada... AUTO-FIRE!
        if (canTargetEnemyPlayer && !hasValidCardTarget)
        {
            Debug.Log($"Auto-Target: {cardData.cardName} atirou o efeito direto na Torre Inimiga!");
            
            CardEffectContext context = new CardEffectContext
            {
                source = source,
                targetCard = null,
                targetPlayer = enemyHealth, 
                isEnemySource = source.isEnemy
            };
            
            // 👇 PASSAMOS O DADO PARA A AÇÃO
            GameAction action = effect.CreateAction(context, data);
            if (action != null) ActionSystem.Instance.AddAction(action);
            
            return; // Sai da função!
        }
        // =========================================================

        TurnManager.LockTurn(); // 🔒 TRANCA A PORTA

        currentSource = source;
        currentCardData = cardData;
        currentPendingEffect = effect;
        currentPendingData = data; // Guarda o pacote para usar depois!

        isWaitingForTarget = true;
        if (arrow != null)
        {
            arrow.SetColor(Color.cyan);
            arrow.ShowArrow(true);
            arrow.UpdateArrow(source.transform.position, source.transform.position); 
        }
    }

    void Update()
    {
        if (!isWaitingForTarget) return;

        if (arrow != null)
        {
            Vector3 endPoint = GetMouseWorldPosition();
            arrow.UpdateArrow(currentSource.transform.position, endPoint);
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
            CardCombat targetCard = hit.collider.GetComponent<CardCombat>();
            PlayerHealth targetPlayer = hit.collider.GetComponent<PlayerHealth>();

            if (currentPendingEffect.IsValidTarget(currentSource, targetCard, targetPlayer, currentPendingData))
            {
                CardEffectContext context = new CardEffectContext
                {
                    source = currentSource,
                    targetCard = targetCard,
                    targetPlayer = targetPlayer,
                    isEnemySource = currentSource.isEnemy
                };
                
                ExecuteAndFinish(context); // Chama a finalização
            }
            else
            {
                Debug.Log("Alvo bloqueado pelas regras do Efeito!");
            }
        }
    }

    private void ExecuteAndFinish(CardEffectContext context)
    {
        isWaitingForTarget = false;
        if (arrow != null) arrow.ShowArrow(false);

        if (currentPendingEffect != null)
        {
            // 👇 PASSAMOS O DADO QUE ESTAVA GUARDADO PARA FABRICAR A AÇÃO
            GameAction action = currentPendingEffect.CreateAction(context, currentPendingData);
            if (action != null) ActionSystem.Instance.AddAction(action);
        }
        
        // Limpa a memória
        currentSource = null; 
        currentCardData = null; 
        currentPendingEffect = null;
        currentPendingData = null;

        TurnManager.UnlockTurn(); 
    }

    private Vector3 GetMouseWorldPosition()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, currentSource.transform.position); 
        if (groundPlane.Raycast(ray, out float distance)) return ray.GetPoint(distance); 
        return currentSource.transform.position; 
    }
}