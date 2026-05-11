using UnityEngine;
using System.Linq;
using Folklorium;

public class SpellSlot : MonoBehaviour, IEffectSource
{
    [SerializeField] private CardData spellData;
    [SerializeField] private ManaManager manaManager;
    [SerializeField] private EffectTargetManager effectTargetManager;
    private TurnManager turnManager; // 1. Referência para o TurnManager

    private bool isEnemy;
    private bool isSpent;
    private Renderer _renderer;

    public bool IsEnemy => isEnemy;
    public Transform EffectTransform => transform;
    public GameObject EffectGameObject => gameObject;
    public string SourceName => spellData != null ? spellData.cardName : gameObject.name;

    private void Awake() 
    {
        _renderer = GetComponent<Renderer>();
        // 2. Busca o TurnManager na cena (seguindo o padrão do seu CardCombat)
        turnManager = FindFirstObjectByType<TurnManager>();
    }

    public void Configure(CardData data, bool enemySide, ManaManager mana)
    {
        spellData = data;
        isEnemy = enemySide;
        manaManager = mana;
        isSpent = false;

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
        if (isSpent || spellData == null || manaManager == null || turnManager == null)
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

        if (!manaManager.HasEnoughMana(spellData.mana))
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
        // ... (resto do seu código permanece igual)
        if (spellData == null || spellData.effects == null)
            return;

        manaManager.SpendMana(spellData.mana);

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

        isSpent = true;
        RefreshVisual();
    }

    private void RefreshVisual()
    {
        if (_renderer == null) return;
        _renderer.material.color = isSpent ? Color.gray : Color.white;
    }
}  