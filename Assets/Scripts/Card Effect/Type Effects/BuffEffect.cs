using UnityEngine;

[CreateAssetMenu(menuName = "Card Effects/Buff Effect")]
public class BuffEffect : CardEffect
{
    public override System.Type GetDataType()
    {
        return typeof(BuffEffectData);
    }

    public override GameAction CreateAction(CardEffectContext context, EffectData rawData)
    {
        // 1. Checamos se os dados são do tipo correto para Buff
        if (rawData is BuffEffectData buffData)
        {
            // 2. Extraímos os valores configurados no Inspector
            int attackBonus = buffData.attack;
            int healthBonus = buffData.health;

            // 3. Criamos o "Ticket de Ação" e mandamos para a fila
            return new BuffAction(context.source, context.targetCard, attackBonus, healthBonus);
        }

        Debug.LogError($"[BuffEffect] ERRO: A carta '{context.source.name}' tentou usar o BuffEffect, mas os dados passados não são BuffEffectData!");
        return null;
    }
}