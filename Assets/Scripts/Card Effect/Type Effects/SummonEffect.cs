using UnityEngine;
using Folklorium;

[CreateAssetMenu(fileName = "NewSummonEffect", menuName = "Card Effects/Summon")]
public class SummonEffect : CardEffect
{
    public override System.Type GetDataType()
    {
        return typeof(SummonEffectData);
    }

    public override bool IsValidTarget(CardCombat source, CardCombat targetCard, PlayerHealth targetPlayer, EffectData rawData)
    {
        // No futuro, podemos checar se o lado do campo tem slots vazios.
        // Por enquanto, sempre podemos tentar invocar.
        return true; 
    }

    public override GameAction CreateAction(CardEffectContext context, EffectData rawData)
    {
        // Valores padrão de segurança (caso o designer esqueça de preencher)
        int atk = 1;
        int hp = 1;
        int qty = 1;
        int side = 0; // 0 = Lado de quem jogou, 1 = Lado do oponente

        // Abrindo o pacote de dados!
        if (rawData is SummonEffectData summonData)
        {
            atk = summonData.attack;
            hp = summonData.health;
            qty = summonData.Quantity > 0 ? summonData.Quantity : 1;
            side = summonData.boardSide;
        }
        else
        {
            Debug.LogWarning("O pacote de dados passado para SummonEffect não é um SummonEffectData!");
        }

        // Criamos o Ticket de Invocação, passando os atributos do Token e quem jogou a carta
        return new SummonAction(atk, hp, qty, side, context.isEnemySource);
    }
}