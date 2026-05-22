using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using Folklorium;

public class DeckEditorController : MonoBehaviour
{
    [Header("Catalog")]
    [SerializeField] private Transform catalogContent;
    [SerializeField] private DeckCardView cardViewPrefab;

    [Header("Selected Deck")]
    [SerializeField] private Transform selectedDeckContent;
    [SerializeField] private DeckEntryView selectedEntryPrefab;

    [Header("Spell Slots")]
    [SerializeField] private Transform spellSlotsContent;
    [SerializeField] private DeckEntryView spellSlotPrefab;

    [Header("Info")]
    [SerializeField] private TMP_Text deckCountText;
    [SerializeField] private TMP_Text auraText;
    [SerializeField] private TMP_Text statusText;

    [Header("Filter")]
    [SerializeField] private CardData.CardType? currentFilter = null;

    private readonly List<CardData> mainDeck = new List<CardData>();
    private readonly List<CardData> selectedSpells = new List<CardData>();

    private List<CardData> currentCatalog = new List<CardData>();

    private void Start()
    {
        LoadSavedDeck();
        RefreshCatalog();
        RefreshSelectedUI();
        RefreshSpellUI();
        RefreshInfo();
    }

    public void SetFilterAll()
    {
        currentFilter = null;
        RefreshCatalog();
    }

    public void SetFilterRed()
    {
        currentFilter = CardData.CardType.Red;
        RefreshCatalog();
    }

    public void SetFilterBlue()
    {
        currentFilter = CardData.CardType.Blue;
        RefreshCatalog();
    }

    public void SetFilterYellow()
    {
        currentFilter = CardData.CardType.Yellow;
        RefreshCatalog();
    }

    public void SetFilterGreen()
    {
        currentFilter = CardData.CardType.Green;
        RefreshCatalog();
    }

    public void SetFilterNeutral()
    {
        currentFilter = CardData.CardType.Neutral;
        RefreshCatalog();
    }

    private void LoadSavedDeck()
    {
        DeckSaveData data = DeckSaveService.Load();

        mainDeck.Clear();
        selectedSpells.Clear();

        if (CardCatalog.Instance == null)
        {
            SetStatus("CardCatalog não foi encontrado.");
            return;
        }

        foreach (string cardName in data.mainDeckCards)
        {
            CardData card = CardCatalog.Instance.GetByName(cardName);
            if (card != null)
                mainDeck.Add(card);
        }

        foreach (string spellName in data.selectedSpells)
        {
            CardData spell = CardCatalog.Instance.GetByName(spellName);
            if (spell != null)
                selectedSpells.Add(spell);
        }
    }

    private void RefreshCatalog()
    {
        if (catalogContent == null || cardViewPrefab == null || CardCatalog.Instance == null)
            return;

        foreach (Transform child in catalogContent)
            Destroy(child.gameObject);

        currentCatalog = CardCatalog.Instance.GetFiltered(currentFilter, true);

        foreach (CardData card in currentCatalog)
        {
            DeckCardView view = Instantiate(cardViewPrefab, catalogContent);

            int selectedCount = 0;

            if (card.cardRole == CardData.CardRole.Spell)
            {
                selectedCount = selectedSpells.Count(s => s.cardName == card.cardName);
            }
            else
            {
                selectedCount = mainDeck.Count(c => c.cardName == card.cardName);
            }

            view.Bind(card, OnCatalogCardClicked, selectedCount);
        }
    }

    private void OnCatalogCardClicked(CardData card)
    {
        if (card == null)
            return;

        if (card.cardRole == CardData.CardRole.Spell)
        {
            TryAddSpell(card);
        }
        else
        {
            TryAddMainDeckCard(card);
        }

        RefreshCatalog();
        RefreshSelectedUI();
        RefreshSpellUI();
        RefreshInfo();
    }

    private void TryAddMainDeckCard(CardData card)
    {
        if (!DeckRules.CanAddMainDeckCard(card, mainDeck))
        {
            SetStatus(mainDeck.Count >= DeckRules.MaxMainDeckCards
                ? "Deck cheio. Remova uma carta antes."
                : "Limite de cópias atingido para esta carta.");
            return;
        }

        mainDeck.Add(card);
        SetStatus($"{card.cardName} adicionado ao deck.");
    }

    private void TryAddSpell(CardData spell)
    {
        if (!DeckRules.CanAddSpell(spell, mainDeck, selectedSpells))
        {
            if (selectedSpells.Count >= DeckRules.MaxSpellSlots)
            {
                SetStatus("Os 2 slots de magia já estão cheios. Remova uma magia antes.");
            }
            else
            {
                SetStatus("Esta magia não combina com as cores do deck atual.");
            }
            return;
        }
        else if (selectedSpells.Count >= DeckRules.MaxSpellSlots)
            {
                SetStatus("Os 2 slots de magia já estão cheios. Remova uma magia antes.");
                return;
            }
        selectedSpells.Add(spell);
        SetStatus($"{spell.cardName} adicionado a uma magia.");
    }

    private void RefreshSelectedUI()
    {
        if (selectedDeckContent == null || selectedEntryPrefab == null)
            return;

        foreach (Transform child in selectedDeckContent)
            Destroy(child.gameObject);

        var groups = mainDeck
            .GroupBy(c => c.cardName)
            .Select(g => new { Card = g.First(), Count = g.Count() })
            .OrderByDescending(x => x.Card.cardRole == CardData.CardRole.Commander)
            .ThenByDescending(x => x.Count);

        foreach (var entry in groups)
        {
            DeckEntryView view = Instantiate(selectedEntryPrefab, selectedDeckContent);
            string label = $"{entry.Card.cardName} x{entry.Count}";
            view.Bind(entry.Card, label, () => RemoveMainDeckCard(entry.Card));
        }
    }

    private void RefreshSpellUI()
    {
        if (spellSlotsContent == null || spellSlotPrefab == null)
            return;

        foreach (Transform child in spellSlotsContent)
            Destroy(child.gameObject);

        for (int i = 0; i < DeckRules.MaxSpellSlots; i++)
        {
            DeckEntryView view = Instantiate(spellSlotPrefab, spellSlotsContent);

            int slotIndex = i; // <- cópia local segura

            if (slotIndex < selectedSpells.Count)
            {
                CardData spell = selectedSpells[slotIndex];
                view.Bind(spell, $"Slot {slotIndex + 1}: {spell.cardName}", () => RemoveSpellAt(slotIndex));
            }
            else
            {
                view.Bind(null, $"Magic Slot {slotIndex + 1}: vazio", null);
            }
        }
    }

    private void RemoveMainDeckCard(CardData card)
    {
        if (card == null)
            return;

        int index = mainDeck.FindLastIndex(c => c.cardName == card.cardName);
        if (index >= 0)
        {
            mainDeck.RemoveAt(index);
            SetStatus($"{card.cardName} removido do deck.");
        }

        RefreshCatalog();
        RefreshSelectedUI();
        RefreshSpellUI();
        RefreshInfo();
    }

    private void RemoveSpellAt(int index)
    {
        if (index < 0 || index >= selectedSpells.Count)
            return;

        CardData removed = selectedSpells[index];
        selectedSpells.RemoveAt(index);
        SetStatus($"{removed.cardName} removida das magias.");

        RefreshCatalog();
        RefreshSelectedUI();
        RefreshSpellUI();
        RefreshInfo();
    }

    private void RefreshInfo()
    {
        if (deckCountText != null)
            deckCountText.text = $"Deck: {mainDeck.Count}/{DeckRules.MaxMainDeckCards}";

        DeckAuraKind aura = DeckRules.ResolveAuraKind(mainDeck);
        if (auraText != null)
            auraText.text = $"Aura: {aura}";

        if (selectedSpells.Count == 2)
        {
            if (statusText != null && string.IsNullOrWhiteSpace(statusText.text))
                statusText.text = "2/2 magias selecionadas.";
        }
    }

    private void SetStatus(string msg)
    {
        if (statusText != null)
            statusText.text = msg;
    }

    public void SaveDeck()
    {
        if (mainDeck.Count != DeckRules.MaxMainDeckCards)
        {
            SetStatus($"Seu deck precisa ter {DeckRules.MaxMainDeckCards} cartas.");
            return;
        }

        if (selectedSpells.Count > DeckRules.MaxSpellSlots)
        {
            SetStatus("Excedeu o limite de 2 magias.");
            return;
        }

        // Validação final de spells em relação às cores do deck
        foreach (CardData spell in selectedSpells)
        {
            if (!DeckRules.CanAddSpell(spell, mainDeck, selectedSpells))
            {
                SetStatus($"A magia {spell.cardName} não combina com o deck atual.");
                return;
            }
        }

        DeckSaveData save = new DeckSaveData();
        save.mainDeckCards = mainDeck.Select(c => c.cardName).ToList();
        save.selectedSpells = selectedSpells.Select(c => c.cardName).ToList();

        DeckSaveService.Save(save);
        SetStatus("Deck salvo com sucesso.");
    }

    public void BackToMenu()
    {
        SceneLoader.LoadMainMenu();
    }
}