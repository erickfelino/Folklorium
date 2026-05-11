using UnityEngine;

public class CardEffectContext
{
    public IEffectSource source;
    public CardCombat targetCard;
    public PlayerHealth targetPlayer;
    public HandManager playerHand;    //De quem é a mão
    public bool IsEnemySource => source != null && source.IsEnemy;
    
}