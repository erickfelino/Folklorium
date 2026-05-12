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
        attackTokenText.text = cardData.attack.ToString();
        lifeText.text = cardData.life.ToString();
        lifeTokenText.text = cardData.life.ToString();

        if (cardData.art != null) cardImage.material.mainTexture = cardData.art.texture;
        gameObject.name = cardData.cardName;
    }
    public void UpdateStatusText(int currentLife, int currentAttack)
    {
        // 1. Atualiza os números em tudo
        lifeText.text = currentLife.ToString();
        lifeTokenText.text = currentLife.ToString();
        attackText.text = currentAttack.ToString();
        attackTokenText.text = currentAttack.ToString();

        // 2. Lógica de Cores da VIDA
        if (currentLife < cardData.life)
        {
            lifeText.color = Color.red;
            lifeTokenText.color = Color.red; // <- Adicionado
        }
        else if (currentLife > cardData.life)
        {
            lifeText.color = Color.green;
            lifeTokenText.color = Color.green; // <- Adicionado
        }
        else
        {
            lifeText.color = Color.white;
            lifeTokenText.color = Color.white; // <- Adicionado
        }

        // 3. Lógica de Cores do ATAQUE
        if (currentAttack > cardData.attack)
        {
            attackText.color = Color.green;
            attackTokenText.color = Color.green; // <- Adicionado
        }
        else
        {
            // Se no futuro você tiver "Debuff" de ataque, pode adicionar o vermelho aqui!
            attackText.color = Color.white;
            attackTokenText.color = Color.white; // <- Adicionado
        }
    }
}