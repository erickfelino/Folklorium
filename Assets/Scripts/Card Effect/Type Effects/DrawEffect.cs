using UnityEngine;

[CreateAssetMenu(menuName = "Card Effects/Draw Effect")]
public class DrawEffect : CardEffect
{
    public override void Execute(CardEffectContext context)
    {
        int damage = context.source.GetComponent<CardDisplay>().cardData.damageValue;

        for (int i = 0; i < damage; i++)
            {
                context.playerHand.DrawCardFromDeck();
            }
    }
}