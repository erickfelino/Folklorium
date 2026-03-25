using UnityEngine;
using TMPro;
using Folklorium;

public class CardDisplay : MonoBehaviour
{
    public CardData cardData;
    public TextMeshPro nameText;
    public TextMeshPro manaText;
    public TextMeshPro attackText;
    public TextMeshPro lifeText;
    public TextMeshPro bodyText;
    public TextMeshPro attackTokenText;
    public TextMeshPro lifeTokenText;
    public Renderer cardImage;

    void Start()
    {
        UpdateCardDisplay();
    }

    public void UpdateCardDisplay()
    {
        nameText.text = cardData.cardName;
        bodyText.text  = cardData.cardBody;
        manaText.text = cardData.mana.ToString();
        attackText.text = cardData.attack.ToString();
        lifeText.text = cardData.life.ToString();
        attackTokenText.text = cardData.attack.ToString();
        lifeTokenText.text = cardData.life.ToString();

        if (cardData.art != null) cardImage.material.mainTexture = cardData.art.texture;
        gameObject.name = cardData.cardName;
    }
    public void UpdateLifeText(int currentLife)
    {
        lifeText.text = currentLife.ToString();
        lifeTokenText.text = currentLife.ToString();

        // Se a vida for menor que a original, fica vermelho (clássico de card games)
        Color textColor = (currentLife < cardData.life) ? Color.red : Color.white;
        
        if (lifeText != null) lifeText.color = textColor;
        if (lifeTokenText != null) lifeTokenText.color = textColor;
    }
}