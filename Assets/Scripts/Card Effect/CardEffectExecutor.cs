using UnityEngine;
using Folklorium; // Lembre-se do namespace para achar a classe Card!

public static class CardEffectExecutor
{
    public static void ExecuteEffects(Card cardData, CardEffectContext context, EffectTriggerType currentTrigger)
    {
        if (cardData.effects == null || cardData.effects.Count == 0) return;

        foreach (var effect in cardData.effects)
        {
            // Só executa se o efeito existir E se o gatilho dele for igual ao momento atual!
            if (effect != null && effect.trigger == currentTrigger)
            {
                effect.Execute(context);
            }
        }
    }
}