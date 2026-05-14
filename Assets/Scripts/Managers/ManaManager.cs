using UnityEngine;
using TMPro;
using System;
using System.Collections.Generic;
using System.Linq;
using Folklorium;

public class ManaManager : MonoBehaviour
{
    private static readonly List<ManaManager> instances = new();

    [SerializeField] private bool isEnemySide;

    public bool IsEnemySide => isEnemySide;

    public static ManaManager GetForSide(bool enemySide)
    {
        return instances.Find(m => m != null && m.isEnemySide == enemySide);
    }

    [Serializable]
    private class TemporaryManaModifier
    {
        public int delta;
        public int remainingTurns;
        public EffectTurnScope scope;
    }

    private readonly List<TemporaryManaModifier> tempModifiers = new();

    [Header("Status")]
    public int maxMana = 0;
    public int currentMana = 0;

    [Header("Interface (UI)")]
    public TMP_Text manaText;

    public event Action<int> OnManaChanged;

    private TurnManager turnManager;

    private void Awake()
    {
        turnManager = FindFirstObjectByType<TurnManager>();
    }

    private void OnEnable()
    {
        if (!instances.Contains(this))
            instances.Add(this);

        if (turnManager != null)
            turnManager.OnTurnChanged += HandleTurnChanged;
    }

    private void OnDisable()
    {
        instances.Remove(this);

        if (turnManager != null)
            turnManager.OnTurnChanged -= HandleTurnChanged;
    }

    private void HandleTurnChanged(bool isPlayerTurn)
    {
        bool currentSideIsEnemy = !isPlayerTurn;

        for (int i = tempModifiers.Count - 1; i >= 0; i--)
        {
            TemporaryManaModifier mod = tempModifiers[i];

            bool countsThisTurn =
                mod.scope == EffectTurnScope.GlobalTurns ||
                mod.scope == EffectTurnScope.OwnerTurns && isEnemySide == currentSideIsEnemy;

            if (!countsThisTurn)
                continue;

            mod.remainingTurns--;

            if (mod.remainingTurns <= 0)
            {
                tempModifiers.RemoveAt(i);
            }
        }

        int effectiveMax = GetEffectiveMaxMana();
        if (currentMana > effectiveMax)
            currentMana = effectiveMax;

        UpdateUI();
    }

    private int GetEffectiveMaxMana()
    {
        int tempTotal = tempModifiers.Sum(m => m.delta);
        return Mathf.Max(0, maxMana + tempTotal);
    }

    public void RefillMana()
    {
        currentMana = GetEffectiveMaxMana();
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

    public void ModifyMana(int delta)
    {
        maxMana = Mathf.Max(0, maxMana + delta);

        int effectiveMax = GetEffectiveMaxMana();
        if (currentMana > effectiveMax)
            currentMana = effectiveMax;

        UpdateUI();
        OnManaChanged?.Invoke(currentMana);
    }

    public void AddTemporaryManaModifier(int delta, int durationTurns, EffectTurnScope scope)
    {
        if (delta == 0 || durationTurns <= 0)
            return;

        tempModifiers.Add(new TemporaryManaModifier
        {
            delta = delta,
            remainingTurns = durationTurns,
            scope = scope
        });

        int effectiveMax = GetEffectiveMaxMana();
        if (currentMana > effectiveMax)
            currentMana = effectiveMax;

        UpdateUI();
        OnManaChanged?.Invoke(currentMana);
    }

    public void UpdateUI()
    {
        if (manaText != null)
            manaText.text = $"{currentMana}/{GetEffectiveMaxMana()}";
    }
}