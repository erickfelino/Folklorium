using System.Collections;
using UnityEngine;
using Folklorium;

public class DrawAction : GameAction
{
    private HandManager targetHand;
    private int amountToDraw;
    private bool isEnemy;

    public DrawAction(HandManager targetHand, int amountToDraw, bool isEnemy)
    {
        this.targetHand = targetHand;
        this.amountToDraw = amountToDraw;
        this.isEnemy = isEnemy;
    }

    public override IEnumerator Perform()
    {
        // Segurança: se a mão não vier no contexto, tenta achar pela cena
        if (targetHand == null)
        {
            Debug.LogWarning("HandManager não veio no contexto. Cancelando compra de cartas.");
            yield break;
        }

        string playerType = isEnemy ? "IA" : "Jogador";
        Debug.Log($"[Ação] {playerType} comprando {amountToDraw} carta(s)...");

        for (int i = 0; i < amountToDraw; i++)
        {
            // 👇 Aqui você chama a função que já existe no seu jogo para sacar cartas!
            // Se a sua função de comprar carta fica no DeckManager, troque aqui.
            targetHand.DrawCardFromDeck(); 
            
            // Dá um respiro de 0.3s entre uma carta e outra para a animação ficar bonita
            yield return new WaitForSeconds(0.3f); 
        }
    }
}