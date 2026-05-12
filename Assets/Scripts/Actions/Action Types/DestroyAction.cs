using System.Collections;
using UnityEngine;
using Folklorium;

public class DestroyAction : GameAction
{
    private CardCombat targetCard;
    private int numberOfTargets;

    public DestroyAction(CardCombat targetCard, int numberOfTargets)
    {
        this.targetCard = targetCard;
        this.numberOfTargets = numberOfTargets;
    }

    public override IEnumerator Perform()
    {
        for (int i = 0; i < numberOfTargets; i++)
        {
            
            bool destroyedSomeone = false;

            if (targetCard != null && targetCard.currentLife > 0)
            {
                Debug.Log($"[Ação] Destruindo {targetCard.name}");
                
                // 👇 PASSAMOS 'false' NO FINAL PARA AVISAR QUE É SÓ CURA! 👇
                targetCard.Die();
                
                destroyedSomeone = true;
            }
        
            // Se destruiu alguém, dá um tempinho (Juice!)
            if (destroyedSomeone)
            {
                yield return new WaitForSeconds(0.5f);
            }
            else
            {
                Debug.LogWarning("[Ação] Destruição falhou: Alvo nulo ou já destruído.");
                yield break;
            }
        }
     
    }
}