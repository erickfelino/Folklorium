// EffectTriggerType.cs
public enum EffectTriggerType
{
    OnPlay,      // Quando a carta cai na mesa
    OnAttack,    // Quando a carta ataca alguém
    OnDamaged,   // Quando a carta sofre dano
    OnDeath,     // Quando a carta morre
    OnTurnStart, // No início do turno
    OnTurnEnd    // No fim do turno
}