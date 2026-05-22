using System.Collections.Generic;
using System.Linq;
using Folklorium;
using UnityEngine;

public class DeckManager : MonoBehaviour
{
    [Header("Deck Settings")]
    [SerializeField] private bool loadFromSavedDeck = false;
    [SerializeField] private string deckFolderPath = "Cards/Red Cards/Creatures";

    public List<CardData> allCards = new List<CardData>();
    private int currentIndex = 0;

    void Start()
    {
        allCards.Clear();

        if (loadFromSavedDeck)
        {
            LoadFromSavedDeck();
        }
        else
        {
            LoadFromResources();
        }

        ShuffleDeck();
    }

    private void LoadFromSavedDeck()
    {
        DeckSaveData save = DeckSaveService.Load();
        if (CardCatalog.Instance == null)
            return;

        foreach (string cardName in save.mainDeckCards)
        {
            CardData card = CardCatalog.Instance.GetByName(cardName);
            if (card != null)
                allCards.Add(card);
        }
    }

    private void LoadFromResources()
    {
        CardData[] cards = Resources.LoadAll<CardData>(deckFolderPath);

        if (cards.Length == 0)
        {
            Debug.LogWarning($"Nenhuma carta encontrada na pasta Resources/{deckFolderPath}!");
            return;
        }

        allCards.AddRange(cards);
    }

    public CardData DrawCard()
    {
        if (allCards.Count == 0 || currentIndex >= allCards.Count)
        {
            Debug.Log($"Acabaram as cartas do baralho ({deckFolderPath})!");
            return null;
        }

        CardData nextCard = allCards[currentIndex];
        currentIndex++;
        return nextCard;
    }

    private void ShuffleDeck()
    {
        for (int i = 0; i < allCards.Count; i++)
        {
            CardData temp = allCards[i];
            int randomIndex = Random.Range(i, allCards.Count);
            allCards[i] = allCards[randomIndex];
            allCards[randomIndex] = temp;
        }
    }
}