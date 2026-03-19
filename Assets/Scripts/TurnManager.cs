using UnityEngine;
using System; // IMPORTANTE: Precisamos disso para usar o 'Action'
using System.Collections;

public class TurnManager : MonoBehaviour
{
    [Header("Gerenciadores de Mana")]
    [SerializeField] private ManaManager playerManaManager;
    [SerializeField] private ManaManager enemyManaManager;
    
    [Header("Gerenciadores de Mão")]
    [SerializeField] private HandManager playerHandManager;
    [SerializeField] private HandManager enemyHandManager;

    [Header("Outros Gerenciadores")]
    [SerializeField] private OpponentAI opponentAI;
    
    public bool IsPlayerTurn { get; private set; } = true;
    public event Action<bool> OnTurnChanged;

    public void StartPlayerTurn()
    {
        Debug.Log("Seu Turno!");
        IsPlayerTurn = true;
        
        playerHandManager.DrawCardFromDeck(); 

        if (playerManaManager.maxMana < 6)
        {
            playerManaManager.maxMana++;
        }
        playerManaManager.RefillMana();

        OnTurnChanged?.Invoke(IsPlayerTurn);
    }

    public void EndPlayerTurn()
    {
        if (!IsPlayerTurn) return; 
        
        IsPlayerTurn = false;
        
        OnTurnChanged?.Invoke(IsPlayerTurn);
        
        StartCoroutine(SimulateOpponentTurn());
    }

    private IEnumerator SimulateOpponentTurn()
    {
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