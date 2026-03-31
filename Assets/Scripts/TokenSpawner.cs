using UnityEngine;
using Folklorium;

public class TokenSpawner : MonoBehaviour
{
    public static TokenSpawner Instance { get; private set; }

    [Header("Configurações do Token")]
    [Tooltip("O Prefab da carta base que será o visual do Token")]
    public GameObject cardPrefab;
    private BoardManager boardManager; 

    private void Awake()
    {
        boardManager = BoardManager.Instance != null ? BoardManager.Instance : FindFirstObjectByType<BoardManager>();
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void SpawnToken(int attack, int health, bool isEnemySide)
    {
        // 1. Definir a tag correta baseada no lado do campo!
        string targetTag = isEnemySide ? "EnemyDropZoneSoldier" : "DropZoneSoldier"; 
        
        GameObject[] allZones = GameObject.FindGameObjectsWithTag(targetTag);
        
        Transform emptyZone = null;
        foreach (GameObject zone in allZones)
        {
            if (zone.transform.childCount == 0)
            {
                emptyZone = zone.transform;
                break;
            }
        }

        if (emptyZone == null)
        {
            Debug.LogWarning($"Não há espaço vazio no lado do {(isEnemySide ? "Inimigo" : "Jogador")} para invocar o Token!");
            return;
        }

        // 2. Criar a carta no espaço encontrado
        // *** 👇 CUIDADO AQUI: A Unity cria ela GIGANTE aqui por causa do Prefab original ***
        GameObject newTokenObj = Instantiate(cardPrefab, emptyZone.position, Quaternion.identity);
        
        // --- 👇 NOVA LÓGICA DE ESCALA INICIAL AQUI 👇 ---
        // Nós forçamos uma escala inicial pequena e neutra (1,1,1) IMEDIATAMENTE após a criação.
        // Isso impede que você veja ela gigante. Agora ela vai nascer pequena e crescer até o tamanho ideal
        // durante o pulo, o que fica muito mais bonito!
        newTokenObj.transform.localScale = Vector3.one;
        // --- ☝️ FIM DA NOVA LÓGICA ☝️ ---

        // 3. Criar os dados do Token na memória da Unity
        CardData tokenData = ScriptableObject.CreateInstance<CardData>();
        tokenData.cardName = "Token";
        tokenData.attack = attack;
        tokenData.life = health;
        tokenData.cardRole = CardData.CardRole.Soldier; 
        
        // Aplicar os dados na carta visual
        CardDisplay display = newTokenObj.GetComponent<CardDisplay>();
        if (display != null)
        {
            display.cardData = tokenData;
            display.UpdateCardDisplay();
        }

        // Marcar a carta como inimiga (para a IA poder usá-la depois, se for o caso)
        CardCombat combat = newTokenObj.GetComponent<CardCombat>();
        if (combat != null)
        {
            combat.isEnemy = isEnemySide;
        }

        // 4. Transformar no lacaio do tabuleiro (sua função do CardDrag!)
        CardDrag dragScript = newTokenObj.GetComponent<CardDrag>();
        if (dragScript != null)
        {
            //dragScript.TransformIntoTokenAndJump(emptyZone, isEnemySide);
        }
    }
}