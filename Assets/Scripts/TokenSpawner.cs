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
        if (boardManager == null)
        {
            Debug.LogWarning("TokenSpawner: BoardManager não encontrado.");
            return;
        }

        CardData tokenData = ScriptableObject.CreateInstance<CardData>();
        tokenData.cardName = "Token";
        tokenData.attack = attack;
        tokenData.life = health;
        tokenData.mana = 0;
        tokenData.cardRole = CardData.CardRole.Soldier;

        if (!boardManager.TryGetFreeSlot(tokenData, isEnemySide, out BoardSlot freeSlot))
        {
            Debug.LogWarning($"Não há espaço vazio no lado do {(isEnemySide ? "Inimigo" : "Jogador")} para invocar o Token!");
            return;
        }

        GameObject newTokenObj = Instantiate(cardPrefab, freeSlot.transform.position, Quaternion.identity);
        newTokenObj.transform.localScale = Vector3.one;

        CardDisplay display = newTokenObj.GetComponent<CardDisplay>();
        if (display != null)
        {
            display.cardData = tokenData;
            display.UpdateCardDisplay();
        }

        CardCombat combat = newTokenObj.GetComponent<CardCombat>();
        if (combat != null)
        {
            combat.isEnemy = isEnemySide;
        }

        if (!boardManager.TryPlaceCard(combat, freeSlot, BoardEntryType.Summoned))
        {
            Destroy(newTokenObj);
            return;
        }

        CardBoardView view = newTokenObj.GetComponent<CardBoardView>();
        if (view != null)
        {
            view.SetupBoardVisuals();
        }
        else
        {
            boardManager.NotifyCardPlaced(combat, freeSlot, BoardEntryType.Summoned);
        }
    }
}