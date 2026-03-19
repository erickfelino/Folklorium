using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Folklorium;

public class OpponentAI : MonoBehaviour
{
    [Header("Gerenciadores do Oponente")]
    public HandManager aiHand;
    public ManaManager aiMana;
    
    public IEnumerator ProcessTurn()
    {
        Debug.Log("IA: Pensando...");
        yield return new WaitForSeconds(1.5f);

        List<GameObject> cardsInHand = new List<GameObject>(aiHand.cardsInHand);

        // A IA tenta jogar todas as cartas que conseguir
        foreach (GameObject cardObj in cardsInHand)
        {
            Card cardData = cardObj.GetComponent<CardDisplay>().cardData;
            
            // Verifica se tem mana
            if (cardData.mana <= aiMana.currentMana)
            {
                // Descobre a tag certa para essa carta do inimigo
                string targetTag = GetEnemyZoneTag(cardData.cardRole);
                
                // Procura um espaço vazio
                GameObject emptyZone = FindEmptyZone(targetTag);

                if (emptyZone != null)
                {
                    Debug.Log($"IA decidiu jogar: {cardData.cardName}");
                    aiMana.SpendMana(cardData.mana);
                    aiHand.RemoveCardFromHand(cardObj);

                    CardDrag dragScript = cardObj.GetComponent<CardDrag>();
                    if (dragScript != null)
                    {
                        dragScript.TransformIntoTokenAndJump(emptyZone.transform, true);
                    }

                    yield return new WaitForSeconds(1.5f); 
                }
            }
        }

        Debug.Log("IA: Fim das jogadas. Passo o turno.");
        yield return new WaitForSeconds(1f);

    }

    private string GetEnemyZoneTag(Card.CardRole role)
    {
        switch (role)
        {
            case Card.CardRole.Soldier: return "EnemyDropZoneSoldier";
            case Card.CardRole.Commander: return "EnemyDropZoneCommander";
            case Card.CardRole.Hero: return "EnemyDropZoneHero";
            default: return "Untagged";
        }
    }

    private GameObject FindEmptyZone(string tag)
    {
        // Acha todas as zonas com essa tag e pega a primeira que não tem "filhos" (cartas) dentro
        GameObject[] zones = GameObject.FindGameObjectsWithTag(tag);
        foreach (GameObject zone in zones)
        {
            if (zone.transform.childCount == 0) return zone;
        }
        return null;
    }
}