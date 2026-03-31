using UnityEngine;

[CreateAssetMenu(menuName = "Card Effects/Damage Effect")]
public class DamageEffect : CardEffect
{
    public override System.Type GetDataType()
    {
        return typeof(DamageEffectData);
    }

    public override GameAction CreateAction(CardEffectContext context, EffectData rawData)
    {
        // 1. Lemos a caixa misteriosa (rawData) e checamos se ela é uma caixa de Dano
        if (rawData is DamageEffectData damageData)
        {
            // 2. Pegamos o valor que você configurou lá no Inspector da carta!
            int damageToDeal = damageData.damage;

            // 3. Cospe o ticket pronto
            return new DamageAction(context.source, context.targetCard, context.targetPlayer, damageToDeal);
        }

        // Sistema anti-falhas: se o designer arrastou o efeito de dano, mas escolheu "BuffData" na Unity
        Debug.LogError($"[DamageEffect] ERRO: A carta '{context.source.name}' tentou usar o DamageEffect, mas os dados passados não são DamageEffectData!");
        return null;
    }
}