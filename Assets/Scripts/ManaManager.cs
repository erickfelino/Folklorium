using UnityEngine;
using TMPro; // Para usar o TextMeshPro da UI

public class ManaManager : MonoBehaviour
{
    [Header("Status do Jogador")]
    public int maxMana = 1; // Começa com 1 cristal no turno 1
    public int currentMana;

    [Header("Interface (UI)")]
    public TMP_Text manaText; // Arraste o texto da tela aqui!

    void Start()
    {
        // Enche a mana logo que o jogo começa
        RefillMana();
    }

    public void RefillMana()
    {
        currentMana = maxMana;
        UpdateUI();
    }

    // A carta vai usar isso para perguntar se pode ser jogada
    public bool HasEnoughMana(int cost)
    {
        return currentMana >= cost;
    }

    // A carta vai usar isso para cobrar o valor depois de cair na mesa
    public void SpendMana(int cost)
    {
        currentMana -= cost;
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (manaText != null)
        {
            manaText.text = $"{currentMana}/{maxMana}";
        }
    }
}