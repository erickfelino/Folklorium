// EffectTriggerType.cs
public enum EffectTriggerType
{
    OnPlay,      // Quando a carta cai na mesa
    OnAttack,    // Quando a carta ataca alguém
    OnDamaged,
    OnDeath,     // Quando a carta morre
    OnTurnStart, // No início do turno
    OnTurnEnd    // No fim do turno
}