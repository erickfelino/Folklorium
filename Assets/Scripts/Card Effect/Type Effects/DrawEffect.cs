using UnityEngine;
using Folklorium;

[CreateAssetMenu(fileName = "NewDrawEffect", menuName = "Card Effects/Draw")]
public class DrawEffect : CardEffect
{
    public override System.Type GetDataType()
    {
        return typeof(DrawCardEffectData);
    }

    public override bool IsValidTarget(IEffectSource source, CardCombat targetCard, PlayerHealth targetPlayer, EffectData rawData)
    {
        return true;
    }

    public override GameAction CreateAction(CardEffectContext context, EffectData rawData)
    {
        int amountToDraw = 1;

        if (rawData is DrawCardEffectData drawData)
        {
            amountToDraw = drawData.amount > 0 ? drawData.amount : 1;
        }
        else
        {
            Debug.LogWarning("O pacote de dados passado para o DrawEffect não é um DrawCardEffectData!");
        }

        HandManager handToDraw = context.playerHand;
        if (handToDraw == null)
        {
            handToDraw = HandManager.GetForSide(context.IsEnemySource);
        }

        if (handToDraw == null)
        {
            Debug.LogWarning("[DrawEffect] Não foi possível encontrar a mão correta para comprar cartas.");
            return null;
        }

        return new DrawAction(handToDraw, amountToDraw, context.IsEnemySource);
    }
}