using System.Collections.Generic;
using Folklorium;
using UnityEngine;

public class DeckManager : MonoBehaviour
{
    public List<CardData> allCards = new List<CardData>();
    private int currentIndex = 0;

    void Start()
    {
        // Adiciona os cards da pasta Resources
        CardData[] cards = Resources.LoadAll<CardData>("Cards");
        allCards.AddRange(cards);

        ShuffleDeck();
        
        // REMOVIDO o loop de comprar 6 cartas daqui. Quem decide comprar é a mão.
    }

    // AGORA ESTE MÉTODO RETORNA UMA CARTA ('Card') EM VEZ DE 'void'
    public CardData DrawCard()
    {
        if (allCards.Count == 0 || currentIndex >= allCards.Count)
        {
            Debug.Log("Acabaram as cartas do baralho!");
            return null; // Retorna nulo para avisar que o deck secou
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