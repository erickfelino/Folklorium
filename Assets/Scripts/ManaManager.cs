using UnityEngine;
using TMPro;
using System;

public class ManaManager : MonoBehaviour
{
    [Header("Status")]
    public int maxMana = 1; 
    public int currentMana;

    [Header("Interface (UI)")]
    public TMP_Text manaText; 

    public event Action<int> OnManaChanged;

    void Start()
    {
        RefillMana();
    }

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

    private void UpdateUI()
    {
        if (manaText != null) manaText.text = $"{currentMana}/{maxMana}";
    }
}