using UnityEngine;
using System; 
using System.Collections;

public class TurnManager : MonoBehaviour
{
    [Header("Configurações da Partida")]
    [SerializeField] private int startingHandSize = 3; // Tamanho base da mão inicial

    [Header("Gerenciadores de Mana")]
    [SerializeField] private ManaManager playerManaManager;
    [SerializeField] private ManaManager enemyManaManager;
    
    [Header("Gerenciadores de Mão")]
    [SerializeField] private HandManager playerHandManager;
    [SerializeField] private HandManager enemyHandManager;

    [Header("Outros Gerenciadores")]
    [SerializeField] private OpponentAI opponentAI;
    
    public bool isPlayerTurn { get; private set; }
    public event Action<bool> OnTurnChanged;

    private void Start()
    {
        // 1. Configuração Inicial Limpa
        playerManaManager.maxMana = 0;
        playerManaManager.currentMana = 0;
        playerManaManager.UpdateUI();

        enemyManaManager.maxMana = 0;
        enemyManaManager.currentMana = 0;
        enemyManaManager.UpdateUI();

        // 2. Joga a moeda pra cima!
        StartCoroutine(DecideWhoGoesFirst());
    }

    private IEnumerator DecideWhoGoesFirst()
    {
        Debug.Log("Sorteando quem começa...");
        yield return new WaitForSeconds(1f); 

        bool playerGoesFirst = UnityEngine.Random.value > 0.5f;

        // 3. Distribui as cartas ANTES do turno começar, aplicando a vantagem do 2º jogador
        yield return StartCoroutine(DealStartingHands(playerGoesFirst));

        // 4. Inicia a partida de fato
        if (playerGoesFirst)
        {
            Debug.Log("O Jogador ganhou no cara ou coroa! Você começa.");
            isPlayerTurn = true;
            StartPlayerTurn();
        }
        else
        {
            Debug.Log("A IA ganhou no cara ou coroa! Ela começa.");
            isPlayerTurn = false;
            StartCoroutine(SimulateOpponentTurn());
        }
    }

    // ==========================================
    // NOVA FASE: DISTRIBUIÇÃO DA MÃO INICIAL
    // ==========================================
    private IEnumerator DealStartingHands(bool playerGoesFirst)
    {
        Debug.Log("Distribuindo cartas iniciais...");

        // Define a quantidade baseada em quem joga primeiro
        int playerDrawCount = playerGoesFirst ? startingHandSize : startingHandSize + 1;
        int enemyDrawCount = playerGoesFirst ? startingHandSize + 1 : startingHandSize;

        // Jogador compra as cartas dele
        for (int i = 0; i < playerDrawCount; i++)
        {
            playerHandManager.DrawCardFromDeck();
            yield return new WaitForSeconds(0.2f); // Dá um tempinho para a carta animar
        }

        yield return new WaitForSeconds(0.5f); // Pausa dramática entre os jogadores

        // IA compra as cartas dela
        for (int i = 0; i < enemyDrawCount; i++)
        {
            enemyHandManager.DrawCardFromDeck();
            yield return new WaitForSeconds(0.2f); // Dá um tempinho para a carta animar
        }

        yield return new WaitForSeconds(1f); // Pausa final antes do "Turno 1" começar
    }

    public void StartPlayerTurn()
    {
        isPlayerTurn = true;
        OnTurnChanged?.Invoke(isPlayerTurn);

        Debug.Log("Seu Turno!");
        
        playerHandManager.DrawCardFromDeck(); 

        if (playerManaManager.maxMana < 6)
        {
            playerManaManager.maxMana++;
        }
        playerManaManager.RefillMana();
    }

    public void EndPlayerTurn()
    {
        if (!isPlayerTurn) return; 
        
        isPlayerTurn = false;
        OnTurnChanged?.Invoke(isPlayerTurn);
        
        StartCoroutine(SimulateOpponentTurn());
    }

    private IEnumerator SimulateOpponentTurn()
    {
        isPlayerTurn = false;
        OnTurnChanged?.Invoke(isPlayerTurn);

        Debug.Log("Turno do Inimigo!");
        
        enemyHandManager.DrawCardFromDeck();

        if (enemyManaManager.maxMana < 6)
        {
            enemyManaManager.maxMana++;
        }
        enemyManaManager.RefillMana();

        yield return new WaitForSeconds(1f); 
        
        yield return StartCoroutine(opponentAI.ProcessTurn());
        
        StartPlayerTurn();
    }
}