using UnityEngine;
using TMPro;
using System;

public class ManaManager : MonoBehaviour
{
    [Header("Status")]
    public int maxMana = 0; // Agora todo mundo começa com 0!
    public int currentMana = 0;

    [Header("Interface (UI)")]
    public TMP_Text manaText; 

    public event Action<int> OnManaChanged;

    // REMOVEMOS O Start() DAQUI! Ele não dita mais as regras.

    public void RefillMana()
    {
        currentMana = maxMana;
        UpdateUI();
        
        OnManaChanged?.Invoke(currentMana); 
    }

    public bool HasEnoughMana(int cost)
    {
        return currentMana >= cost;
    }

    public void SpendMana(int cost)
    {
        currentMana -= cost;
        UpdateUI();
        
        OnManaChanged?.Invoke(currentMana);
    }

    // Transformei em 'public' para o TurnManager poder atualizar a UI no Start do jogo
    public void UpdateUI() 
    {
        if (manaText != null) manaText.text = $"{currentMana}/{maxMana}";
    }
}