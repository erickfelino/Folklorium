using System.Collections.Generic;
using Folklorium;
using UnityEngine;

public class DeckManager : MonoBehaviour
{
    public List<Card> allCards = new List<Card>();
    
    // Deixamos exposto no Inspector para você dizer de quem é essa mão!
    [SerializeField] private HandManager hand; 
    
    private int currentIndex = 0;

    void Start()
    {
        // Adiciona os cards da pasta Resources
        Card[] cards = Resources.LoadAll<Card>("Cards");
        allCards.AddRange(cards);

        ShuffleDeck(); // Embaralha as cartas antes de comprar!

        for (int i = 0; i < 6; i++)
        {
            DrawCard();
        }
    }

    public void DrawCard()
    {
        if (allCards.Count == 0 || currentIndex >= allCards.Count)
        {
            Debug.Log("Acabaram as cartas do baralho!");
            return;
        }

        Card nextCard = allCards[currentIndex];
        hand.AddCardToHand(nextCard);
        currentIndex++;
    }
    private void ShuffleDeck()
    {
        for (int i = 0; i < allCards.Count; i++)
        {
            Card temp = allCards[i];
            int randomIndex = Random.Range(i, allCards.Count);
            allCards[i] = allCards[randomIndex];
            allCards[randomIndex] = temp;
        }
    }
}