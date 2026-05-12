using UnityEngine;
using System.Linq;
using Folklorium;

public class SpellSlot : MonoBehaviour, IEffectSource
{
    [SerializeField] private CardData spellData;
    [SerializeField] private ManaManager manaManager;
    [SerializeField] private EffectTargetManager effectTargetManager;
    private TurnManager turnManager;
    private bool isEnemy;
    private int manaCostModifier;
    private int remainingUses = 1;
    private int extraUsesBonus = 0;
    
    // 1. Mudamos para um array, pois agora temos vários sub-objetos (cilindros)
    private Renderer[] _renderers;

    public bool IsEnemy => isEnemy;
    public Transform EffectTransform => transform;
    public GameObject EffectGameObject => gameObject;
    public string SourceName => spellData != null ? spellData.cardName : gameObject.name;

    private void Awake() 
    {
        // 2. Buscamos todos os renderers nos filhos (pCylinder1, pCylinder2, etc)
        _renderers = GetComponentsInChildren<Renderer>();
        turnManager = FindFirstObjectByType<TurnManager>();
    }

    public void Configure(CardData data, bool enemySide, ManaManager mana)
    {
        spellData = data;
        isEnemy = enemySide;
        manaManager = mana;

        RefreshVisual();
    }

    private void OnMouseDown()
    {
        TryCast();
    }

    private void TryCast()
    {
        // --- AS NOVAS TRAVAS DE SEGURANÇA ---
        
        // 1. Se já foi usada ou dados estão faltando
        if (remainingUses <= 0 || spellData == null || manaManager == null || turnManager == null)
        return;

        // 2. Se for uma magia do inimigo, o jogador não pode clicar para usar
        if (isEnemy)
            return;

        // 3. Se não for o turno do jogador
        if (!turnManager.IsPlayerTurn)
        {
            Debug.Log("Não é seu turno!");
            return;
        }

        // 4. Se o jogo estiver ocupado resolvendo outro efeito (evita spam de cliques)
        if (TurnManager.IsResolvingEffect)
            return;

        // --- FIM DAS TRAVAS ---

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

        manaManager.SpendMana(GetEffectiveManaCost());

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

        remainingUses = Mathf.Max(1, 1 + extraUsesBonus);
        RefreshVisual();
    }

    public int GetEffectiveManaCost()
    {
        if (spellData == null)
            return 0;

        return Mathf.Max(0, spellData.mana + manaCostModifier);
    }

    public void ConsumeUse()
    {
        if (remainingUses <= 0)
            return;

        remainingUses--;
        RefreshVisual();
    }

    private void RefreshVisual()
    {
        if (_renderers == null || _renderers.Length == 0) return;

        bool isAvailable = remainingUses > 0;

        Color finalColor = isAvailable ? Color.white : Color.gray;
        Color emissionColor = isAvailable ? Color.white : Color.black;

        foreach (var r in _renderers)
        {
            if (!isAvailable)
            {
                r.material.color = finalColor;
                r.material.SetColor("_EmissionColor", emissionColor);
                
            }
        }
    }
}