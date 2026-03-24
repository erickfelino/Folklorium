using UnityEngine;
using Folklorium;

public class EffectTargetManager : MonoBehaviour
{
    // Singleton para as cartas acharem esse gerente facilmente
    private static EffectTargetManager _instance;

    public static EffectTargetManager Instance
    {
        get
        {
            // Se alguém chamar e a instância estiver vazia, ele procura na cena na hora!
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<EffectTargetManager>();
            }
            return _instance;
        }
    }

    [SerializeField] private TargetingArrow arrow; // Problema 4 resolvido: Sem FindFirstObject
    private Camera mainCamera;

    private bool isWaitingForTarget = false;
    
    // Memória do que estamos fazendo
    private CardCombat currentSource;
    private Card currentCardData;
    private CardEffect currentPendingEffect; 

    void Awake()
{
    // Garante que se já existir um (ex: trocou de cena), este se destrói
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

    // A carta chama isso aqui quando cai na mesa!
    public void StartTargeting(CardCombat source, Card cardData, CardEffect effect)
    {
        currentSource = source;
        currentCardData = cardData;
        currentPendingEffect = effect;

        isWaitingForTarget = true;
        if (arrow != null)
        {
            arrow.SetColor(Color.cyan);
            arrow.ShowArrow(true);
            arrow.UpdateArrow(source.transform.position, source.transform.position); 
        }
    }

    // Problemas 1 e 2 resolvidos: Só 1 Update lendo o input na cena inteira!
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

            // O Targeting pergunta ao Efeito: "Posso atirar nisso?"
            if (currentPendingEffect.IsValidTarget(currentSource, targetCard, targetPlayer))
            {
                CardEffectContext context = new CardEffectContext
                {
                    source = currentSource,
                    targetCard = targetCard,
                    targetPlayer = targetPlayer,
                    isEnemySource = currentSource.isEnemy
                };

                ExecuteAndFinish(context);
            }
            else
            {
                // Se quiser, pode colocar um som de "erro" aqui!
                Debug.Log("Alvo bloqueado pelas regras do Efeito!");
            }
        }
    }

    private void ExecuteAndFinish(CardEffectContext context)
    {
        isWaitingForTarget = false;
        if (arrow != null) arrow.ShowArrow(false);

        // Dispara os efeitos da carta com o contexto preenchido com o alvo!
        CardEffectExecutor.ExecuteEffects(currentCardData, context, EffectTriggerType.OnPlay);
        
        // Limpa a memória
        currentSource = null; currentCardData = null; currentPendingEffect = null;
    }

    private Vector3 GetMouseWorldPosition()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, currentSource.transform.position); 
        if (groundPlane.Raycast(ray, out float distance)) return ray.GetPoint(distance); 
        return currentSource.transform.position; 
    }
}