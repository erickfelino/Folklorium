using System.Collections;
using UnityEngine;
using Folklorium;

public class SummonAction : GameAction
{
    private int attack;
    private int health;
    private int quantity;
    private int boardSide;
    private bool isEnemySource; 

    public SummonAction(int attack, int health, int quantity, int boardSide, bool isEnemySource)
    {
        this.attack = attack;
        this.health = health;
        this.quantity = quantity;
        this.boardSide = boardSide;
        this.isEnemySource = isEnemySource;
    }

    public override IEnumerator Perform()
    {
        bool spawnForEnemy = isEnemySource; 
        if (boardSide == 1) spawnForEnemy = !spawnForEnemy; 

        string sideName = spawnForEnemy ? "Inimigo" : "Jogador";
        Debug.Log($"[Ação] Invocando {quantity} token(s) {attack}/{health} para o {sideName}...");

        for (int i = 0; i < quantity; i++)
        {
            // Pede para o sistema global fabricar o lacaio na mesa!
            if (TokenSpawner.Instance != null)
            {
                yield return new WaitForSeconds(1f);
                Debug.Log("Tentando invocar token");
                TokenSpawner.Instance.SpawnToken(attack, health, spawnForEnemy);
            }
            else
            {
                Debug.LogError("TokenSpawner não encontrado na cena! Crie um GameObject vazio com ele.");
            }
            
            yield return new WaitForSeconds(0.3f); 
        }
    }
}