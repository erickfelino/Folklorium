using UnityEngine;
using System.Linq;
using Folklorium;

public class SpellSlot : MonoBehaviour, IEffectSource
{
    [SerializeField] private CardData spellData;
    [SerializeField] private ManaManager manaManager;
    [SerializeField] private EffectTargetManager effectTargetManager;
    [SerializeField] private SpellManager spellManager;

    private TurnManager turnManager;
    private bool isEnemy;
    private int manaCostModifier;

    private int maxUses = 1;
    private int remainingUses = 1;
    private int extraUsesBonus = 0;

    private Renderer[] _renderers;
    private Color[] originalColors;
    private Color[] originalEmissionColors;

    public bool IsEnemy => isEnemy;
    public Transform EffectTransform => transform;
    public GameObject EffectGameObject => gameObject;
    public string SourceName => spellData != null ? spellData.cardName : gameObject.name;
    public bool IsSpent => remainingUses <= 0;

    private void Awake()
    {
        _renderers = GetComponentsInChildren<Renderer>();
        turnManager = FindFirstObjectByType<TurnManager>();

        if (spellManager == null)
            spellManager = GetComponentInParent<SpellManager>();

        CacheOriginalVisuals();
    }

    private void CacheOriginalVisuals()
    {
        if (_renderers == null)
            return;

        originalColors = new Color[_renderers.Length];
        originalEmissionColors = new Color[_renderers.Length];

        for (int i = 0; i < _renderers.Length; i++)
        {
            if (_renderers[i] == null)
                continue;

            originalColors[i] = _renderers[i].material.color;

            if (_renderers[i].material.HasProperty("_EmissionColor"))
            {
                originalEmissionColors[i] = _renderers[i].material.GetColor("_EmissionColor");
            }
        }
    }

    public void Configure(CardData data, bool enemySide, ManaManager mana)
    {
        spellData = data;
        isEnemy = enemySide;
        manaManager = mana;

        maxUses = Mathf.Max(1, 1 + extraUsesBonus);
        remainingUses = maxUses;

        RefreshVisual();
    }

    private void OnMouseDown()
    {
        if (spellManager != null && spellManager.IsReactivateSelectionActive)
        {
            if (IsSpent)
                spellManager.ChooseReactivateSlot(this);

            return;
        }

        TryCast();
    }

    private void OnMouseEnter()
    {
        if (spellManager != null && spellManager.IsReactivateSelectionActive)
        {
            spellManager.SetHoveredReactivateSlot(this);
        }
    }

    private void OnMouseExit()
    {
        if (spellManager != null && spellManager.IsReactivateSelectionActive)
        {
            spellManager.ClearHoveredReactivateSlot(this);
        }
    }

    private void TryCast()
    {
        if (remainingUses <= 0 || spellData == null || manaManager == null || turnManager == null)
            return;

        if (isEnemy)
            return;

        if (!turnManager.IsPlayerTurn)
        {
            Debug.Log("Não é seu turno!");
            return;
        }

        if (TurnManager.IsResolvingEffect)
            return;

        if (!manaManager.HasEnoughMana(GetEffectiveManaCost()))
        {
            Debug.Log("Mana insuficiente!");
            return;
        }

        var firstTargetedEffect = spellData.effects?
            .FirstOrDefault(e => e != null && e.effectSO != null && e.trigger == EffectTriggerType.OnPlay && e.effectSO.requiresTarget);

        if (firstTargetedEffect != null)
        {
            if (firstTargetedEffect.parameters != null && firstTargetedEffect.parameters.RandomizeTarget())
            {
                EffectTargetManager.Instance.ResolveRandomTarget(this, firstTargetedEffect.effectSO, firstTargetedEffect.parameters);
            }
            else
            {
                effectTargetManager.StartTargeting(this, spellData, firstTargetedEffect.effectSO, firstTargetedEffect.parameters);
            }
            return;
        }

        ResolveSpellEffects(null, null);
    }

    public void ResolveSpellEffects(CardCombat targetCard, PlayerHealth targetPlayer)
    {
        if (spellData == null || spellData.effects == null)
            return;

        int baseCost = Mathf.Max(0, spellData.mana + manaCostModifier);

        manaManager.SpendMana(GetEffectiveManaCost());

        if (spellManager != null)
            spellManager.TryConsumeFreeCastIfApplied(baseCost);

        foreach (var entry in spellData.effects)
        {
            if (entry == null || entry.effectSO == null || entry.trigger != EffectTriggerType.OnPlay)
                continue;

            CardEffectContext context = new CardEffectContext
            {
                source = this,
                targetCard = targetCard,
                targetPlayer = targetPlayer
            };

            GameAction action = entry.effectSO.CreateAction(context, entry.parameters);
            if (action != null)
                ActionSystem.Instance.AddAction(action);
        }

        ConsumeUse();
        RefreshVisual();
    }

    public void ApplyAuraModifiers(int manaDiscount, int extraUses)
    {
        manaCostModifier = manaDiscount;
        extraUsesBonus = extraUses;

        maxUses = Mathf.Max(1, 1 + extraUsesBonus);
        remainingUses = maxUses;

        RefreshVisual();
    }

    public int GetEffectiveManaCost()
    {
        if (spellData == null)
            return 0;

        int baseCost = Mathf.Max(0, spellData.mana + manaCostModifier);
        return spellManager != null ? spellManager.PeekCost(baseCost) : baseCost;
    }

    public void ConsumeUse()
    {
        if (remainingUses <= 0)
            return;

        remainingUses--;
        RefreshVisual();
    }

    public void RestoreOneUse()
    {
        if (remainingUses < maxUses)
            remainingUses++;

        RefreshVisual();
    }

    private void RefreshVisual()
    {
        if (_renderers == null || _renderers.Length == 0)
            return;

        bool isAvailable = remainingUses > 0;

        for (int i = 0; i < _renderers.Length; i++)
        {
            Renderer r = _renderers[i];
            if (r == null) continue;

            if (isAvailable)
            {
                r.material.color = originalColors != null && i < originalColors.Length ? originalColors[i] : Color.white;

                if (r.material.HasProperty("_EmissionColor"))
                {
                    Color emission = originalEmissionColors != null && i < originalEmissionColors.Length
                        ? originalEmissionColors[i]
                        : Color.white;

                    r.material.SetColor("_EmissionColor", emission);
                }
            }
            else
            {
                r.material.color = Color.gray;

                if (r.material.HasProperty("_EmissionColor"))
                {
                    r.material.SetColor("_EmissionColor", Color.black);
                }
            }
        }
    }
}