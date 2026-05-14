using UnityEngine;
using TMPro;
using System;
using System.Collections.Generic;

public class ManaManager : MonoBehaviour
{
    private static readonly List<ManaManager> instances = new();

    [Header("Side")]
    [SerializeField] private bool isEnemySide;

    public bool IsEnemySide => isEnemySide;

    public static ManaManager GetForSide(bool enemySide)
    {
        return instances.Find(m => m != null && m.isEnemySide == enemySide);
    }

    private void OnEnable()
    {
        if (!instances.Contains(this))
            instances.Add(this);
    }

    private void OnDisable()
    {
        instances.Remove(this);
    }

    public void ModifyMana(int delta)
    {
        maxMana = Mathf.Max(0, maxMana + delta);
        currentMana = Mathf.Clamp(currentMana + delta, 0, maxMana);
        UpdateUI();
        OnManaChanged?.Invoke(currentMana);
    }

    [Header("Status")]
    public int maxMana = 0;
    public int currentMana = 0;

    [Header("Interface (UI)")]
    public TMP_Text manaText;

    public event Action<int> OnManaChanged;

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

    public void UpdateUI()
    {
        if (manaText != null) manaText.text = $"{currentMana}/{maxMana}";
    }
}