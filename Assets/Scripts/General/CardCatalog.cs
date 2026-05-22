using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Folklorium;

public class CardCatalog : MonoBehaviour
{
    public static CardCatalog Instance { get; private set; }

    [SerializeField] private string resourcesPath = "Cards";

    private readonly List<CardData> allCards = new List<CardData>();
    private Dictionary<string, CardData> byName = new Dictionary<string, CardData>();

    public IReadOnlyList<CardData> AllCards => allCards;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        Reload();
    }

    public void Reload()
    {
        allCards.Clear();

        CardData[] loaded = Resources.LoadAll<CardData>(resourcesPath);
        allCards.AddRange(loaded);

        byName = allCards
            .GroupBy(c => c.cardName)
            .ToDictionary(g => g.Key, g => g.First());
    }

    public CardData GetByName(string cardName)
    {
        if (string.IsNullOrWhiteSpace(cardName))
            return null;

        return byName.TryGetValue(cardName, out CardData card) ? card : null;
    }

    public List<CardData> GetFiltered(CardData.CardType? colorFilter, bool includeSpells = true)
    {
        IEnumerable<CardData> query = allCards;

        if (colorFilter.HasValue)
            query = query.Where(c => c.cardColorGroup == colorFilter.Value);

        if (!includeSpells)
            query = query.Where(c => c.cardRole != CardData.CardRole.Spell);

        return query
            .OrderBy(c => c.cardRole == CardData.CardRole.Spell ? 1 : 0) // spells por último
            .ThenBy(c => c.mana)
            .ThenBy(c => c.cardName)
            .ToList();
    }
}