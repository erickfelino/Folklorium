using System.Collections.Generic;
using Folklorium;
using UnityEngine;

public class DeckManager : MonoBehaviour
{
    [Header("Deck Settings")]
    [Tooltip("Nome da pasta dentro de Resources onde estão os ScriptableObjects deste deck.")]
    [SerializeField] private string deckFolderPath = "Cards/Red Cards";

    public List<CardData> allCards = new List<CardData>();
    private int currentIndex = 0;

    void Start()
    {
        // 👇 Agora ele carrega da pasta que você escrever no Inspector!
        CardData[] cards = Resources.LoadAll<CardData>(deckFolderPath);

        if (cards.Length == 0)
        {
            Debug.LogWarning($"Nenhuma carta encontrada na pasta Resources/{deckFolderPath}! Verifique o nome.");
        }
        else
        {
            allCards.AddRange(cards);
            ShuffleDeck();
        }
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