using UnityEngine;
using System.Collections;

public class TurnManager : MonoBehaviour
{
    [Header("Gerenciadores de Mana")]
    [SerializeField] private ManaManager playerManaManager;
    [SerializeField] private ManaManager enemyManaManager;
    
    [Header("Outros Gerenciadores")]
    [SerializeField] private DeckManager playerDeckManager;
    [SerializeField] private DeckManager enemyDeckManager;
    [SerializeField] private EndTurnButton endTurnButton;
    [SerializeField] private OpponentAI opponentAI; // Referência para a nossa nova IA!
    
    public bool isPlayerTurn = true;

    public void StartPlayerTurn()
    {
        Debug.Log("Seu Turno!");
        isPlayerTurn = true;
        
        playerDeckManager.DrawCard(); // Compra carta pro jogador

        // Aumenta e recarrega SÓ a mana do jogador
        if (playerManaManager.maxMana < 6)
        {
            playerManaManager.maxMana++;
        }
        playerManaManager.RefillMana();

        endTurnButton.SetPlayerTurnVisuals();
    }

    public void EndPlayerTurn()
    {
        if (!isPlayerTurn) return; 
        
        isPlayerTurn = false;
        endTurnButton.SetOpponentTurnVisuals(); 
        
        StartCoroutine(SimulateOpponentTurn());
    }

    private IEnumerator SimulateOpponentTurn()
    {
        Debug.Log("Turno do Inimigo!");
        
        // 1. O INIMIGO COMPRA UMA CARTA!
        enemyDeckManager.DrawCard();

        if (enemyManaManager.maxMana < 6)
        {
            enemyManaManager.maxMana++;
        }
        enemyManaManager.RefillMana();

        yield return new WaitForSeconds(1f); 
        
        // CHAMA A IA PARA JOGAR (Ainda com listas vazias por enquanto)
        yield return StartCoroutine(opponentAI.ProcessTurn());
    }
}