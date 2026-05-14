using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Folklorium;

public class SpellManager : MonoBehaviour
{
    private static readonly List<SpellManager> instances = new();

    [SerializeField] private bool isEnemySide;
    [SerializeField] private SpellSlot spellSlot1;
    [SerializeField] private SpellSlot spellSlot2;
    [SerializeField] private ManaManager manaManager;
    [SerializeField] private TargetingArrow targetingArrow;

    private int freeCastCharges = 0;

    private bool isReactivateSelecting = false;
    private IEffectSource reactivateSource;
    private SpellSlot hoveredReactivateSlot;

    public bool IsEnemySide => isEnemySide;
    public bool IsReactivateSelectionActive => isReactivateSelecting;

    public static SpellManager GetForSide(bool enemySide)
    {
        return instances.Find(m => m != null && m.isEnemySide == enemySide);
    }

    private void Awake()
    {
        if (targetingArrow == null)
            targetingArrow = FindFirstObjectByType<TargetingArrow>();
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

    private void Update()
    {
        if (!isReactivateSelecting)
            return;

        if (targetingArrow != null)
        {
            Vector3 start = reactivateSource != null ? reactivateSource.EffectTransform.position : transform.position;
            Vector3 end = hoveredReactivateSlot != null ? hoveredReactivateSlot.transform.position : start;

            targetingArrow.SetColor(Color.cyan);
            targetingArrow.ShowArrow(true);
            targetingArrow.UpdateArrow(start, end);
        }

        if (Input.GetMouseButtonDown(1))
        {
            CancelReactivateSelection();
        }
    }

    public void SetupSpells(CardData spell1, CardData spell2, bool enemySide)
    {
        isEnemySide = enemySide;

        if (spellSlot1 != null) spellSlot1.Configure(spell1, enemySide, manaManager);
        if (spellSlot2 != null) spellSlot2.Configure(spell2, enemySide, manaManager);
    }

    public SpellSlot[] GetSpellSlots()
    {
        return new SpellSlot[] { spellSlot1, spellSlot2 };
    }

    public void GrantFreeCast(int charges = 1)
    {
        freeCastCharges += Mathf.Max(0, charges);
    }

    public int PeekCost(int baseCost)
    {
        return freeCastCharges > 0 && baseCost > 0 ? 0 : Mathf.Max(0, baseCost);
    }

    public bool TryConsumeFreeCastIfApplied(int baseCost)
    {
        if (baseCost <= 0 || freeCastCharges <= 0)
            return false;

        freeCastCharges--;
        return true;
    }

    public IEnumerator BeginReactivateSelection(IEffectSource source)
    {
        if (source == null)
            yield break;

        List<SpellSlot> spentSlots = GetSpellSlots()
            .Where(s => s != null && s.IsSpent)
            .ToList();

        if (spentSlots.Count == 0)
        {
            Debug.Log("Não há magias esgotadas para reativar.");
            yield break;
        }

        if (spentSlots.Count == 1)
        {
            spentSlots[0].RestoreOneUse();
            yield break;
        }

        isReactivateSelecting = true;
        reactivateSource = source;
        hoveredReactivateSlot = null;

        TurnManager.LockTurn();

        while (isReactivateSelecting)
            yield return null;
    }

    public void SetHoveredReactivateSlot(SpellSlot slot)
    {
        if (!isReactivateSelecting)
            return;

        if (slot != null && slot.IsSpent)
        {
            hoveredReactivateSlot = slot;
        }
    }

    public void ClearHoveredReactivateSlot(SpellSlot slot)
    {
        if (!isReactivateSelecting)
            return;

        if (hoveredReactivateSlot == slot)
            hoveredReactivateSlot = null;
    }

    public void ChooseReactivateSlot(SpellSlot slot)
    {
        if (!isReactivateSelecting || slot == null || !slot.IsSpent)
            return;

        slot.RestoreOneUse();
        FinishReactivateSelection();
    }

    public void CancelReactivateSelection()
    {
        if (!isReactivateSelecting)
            return;

        FinishReactivateSelection();
    }

    private void FinishReactivateSelection()
    {
        isReactivateSelecting = false;
        reactivateSource = null;
        hoveredReactivateSlot = null;

        if (targetingArrow != null)
            targetingArrow.ShowArrow(false);

        TurnManager.UnlockTurn();
    }
}