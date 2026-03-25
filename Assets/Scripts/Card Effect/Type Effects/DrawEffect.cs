using UnityEngine;
using Folklorium;

[CreateAssetMenu(fileName = "NewDrawEffect", menuName = "Card Effects/Draw")]
public class DrawEffect : CardEffect
{
    public override System.Type GetDataType()
    {
        return typeof(DrawCardEffectData);
    }
    public override bool IsValidTarget(CardCombat source, CardCombat targetCard, PlayerHealth targetPlayer)
    {
        // Magias de comprar cartas geralmente não precisam que você clique em um alvo na mesa.
        // Então, ela sempre é um alvo válido ao ser jogada!
        return true; 
    }

    public override GameAction CreateAction(CardEffectContext context, EffectData rawData)
    {
        HandManager handToDraw = context.playerHand;
        
        int amountToDraw = 1; // Valor padrão de segurança

        // 👇 A MÁGICA DO CASTING: Tentamos transformar o pacote genérico no pacote específico!
        if (rawData is DrawCardEffectData drawData)
        {
            // Se deu certo, agora temos acesso ao "amountToDraw" que você criou no EffectData.cs!
            amountToDraw = drawData.amount > 0 ? drawData.amount : 1;
        }
        else
        {
            Debug.LogWarning("O pacote de dados passado para o DrawEffect não é um DrawCardEffectData!");
        }

        return new DrawAction(handToDraw, amountToDraw, context.isEnemySource);
    }
}