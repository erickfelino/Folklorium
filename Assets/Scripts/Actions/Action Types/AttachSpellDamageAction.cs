using System.Collections;
using UnityEngine;
using Folklorium;

public class AttachSpellDamageAuraAction : GameAction
{
    private readonly CardCombat sourceCard;
    private readonly int bonusDamage;

    public AttachSpellDamageAuraAction(CardCombat sourceCard, int bonusDamage)
    {
        this.sourceCard = sourceCard;
        this.bonusDamage = bonusDamage;
    }

    public override IEnumerator Perform()
    {
        if (sourceCard == null || sourceCard.isDead)
            yield break;

        SpellDamageAuraProvider provider = sourceCard.GetComponent<SpellDamageAuraProvider>();
        if (provider == null)
            provider = sourceCard.gameObject.AddComponent<SpellDamageAuraProvider>();

        provider.SetBonus(bonusDamage);
        yield break;
    }
}