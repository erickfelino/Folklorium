using UnityEngine;
using Folklorium; 

public static class CardEffectExecutor
{
    public static void ExecuteEffects(CardData cardData, CardEffectContext context, EffectTriggerType currentTrigger)
    {
        if (cardData.effects == null || cardData.effects.Count == 0) return;

        // Iteramos sobre a nova classe 'EffectEntry'
        foreach (EffectEntry entry in cardData.effects)
        {
            // Verificamos se a entrada existe, se o SO foi "linkado" e se o gatilho bate
            if (entry != null && entry.effectSO != null && entry.trigger == currentTrigger)
            {
                // 👇 A MUDANÇA CONCEITUAL DO NÍVEL 3 + POLIMORFISMO 👇
                // Extraímos o molde (effectSO) e os dados da carta (parameters)
                CardEffect effectSO = entry.effectSO;
                EffectData rawData = entry.parameters;

                // Pedimos para o efeito fabricar a ação enviando O CONTEXTO e OS DADOS!
                GameAction action = effectSO.CreateAction(context, rawData);
                
                // Se o efeito retornou um ticket válido, jogamos pro Gerente!
                if (action != null)
                {
                    ActionSystem.Instance.AddAction(action);
                }
            }
        }
    }
}