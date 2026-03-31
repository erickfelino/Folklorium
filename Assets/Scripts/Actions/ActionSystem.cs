using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ActionSystem : MonoBehaviour
{
    // Singleton para acesso fácil
    public static ActionSystem Instance { get; private set; }

    // A Fila de Ações do Nível 3!
    private Queue<GameAction> actionQueue = new Queue<GameAction>();
    public bool isProcessingAction = false;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(this.gameObject);
        else Instance = this;
    }

    // O "Saguão": Adiciona uma nova ordem à fila
    public void AddAction(GameAction action)
    {
        actionQueue.Enqueue(action);
        if (!isProcessingAction)
        {
            StartCoroutine(ProcessNextAction());
        }
    }

    // O "Motor": Executa o trabalho, uma por uma
    private IEnumerator ProcessNextAction()
    {
        while (actionQueue.Count > 0)
        {
            isProcessingAction = true;
            GameAction currentAction = actionQueue.Dequeue();

            // SINAL VERMELHO GLOBAL! (Migrando a nossa lógica de Pausa pra cá)
            TurnManager.LockTurn(); 

            // 👇 O trabalho é executado e o motor ESPERA o tempo dele terminar
            yield return StartCoroutine(currentAction.Perform());

            TurnManager.UnlockTurn(); // SINAL VERDE GLOBAL
        }
        isProcessingAction = false;
    }

    // Retorna TRUE se qualquer engrenagem do jogo ainda estiver rodando ou esperando o jogador
    public bool IsGameBusy()
    {
        // Procura a IA para saber se ela está ocupada mirando
        OpponentAI ai = Object.FindFirstObjectByType<OpponentAI>();
        bool aiIsBusy = ai != null && ai.isAITargeting;

        return isProcessingAction || 
               !IsQueueEmpty() || 
               EffectTargetManager.Instance.isWaitingForTarget ||
               aiIsBusy; // <--- NOVA TRAVA ADICIONADA AQUI
    }

    public bool IsQueueEmpty()
    {
        return actionQueue.Count == 0;
    }
}