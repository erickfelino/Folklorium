using UnityEngine;

public class GameFlowManager : MonoBehaviour
{
    public static GameFlowManager Instance { get; private set; }

    [SerializeField] private GameOverUI gameOverUI;

    private bool isGameOver = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void TriggerVictory()
    {
        if (isGameOver) return;
        isGameOver = true;
        if (gameOverUI != null) gameOverUI.ShowVictory();
    }

    public void TriggerDefeat()
    {
        if (isGameOver) return;
        isGameOver = true;
        if (gameOverUI != null) gameOverUI.ShowDefeat();
    }
}