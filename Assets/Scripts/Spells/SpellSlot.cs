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
    private bool isSpent;
    
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
        if (_renderers == null || _renderers.Length == 0) return;

        // Definimos as cores baseadas no estado
        Color finalColor = isSpent ? Color.gray : Color.white;
        
        // Se estiver gasto, a emissão fica preta (apagada). Se não, fica branca (brilho normal)
        // Nota: O shader Standard usa "_EmissionColor" para controlar o brilho via código
        Color emissionColor = isSpent ? Color.black : Color.white; 

        foreach (var r in _renderers)
        {
            // Muda a cor principal (Albedo)
            r.material.color = finalColor;

            // Muda a cor da emissão para "apagar" as runas azuis
            r.material.SetColor("_EmissionColor", emissionColor);
            
            // Dica: Se o brilho não sumir totalmente, pode ser necessário usar:
            // DynamicGI.SetEmissive(r, isSpent ? 0 : 1);
        }
    }
}