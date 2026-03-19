using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Folklorium;

public class OpponentAI : MonoBehaviour
{
    [Header("Gerenciadores do Oponente")]
    public HandManager aiHand;
    public ManaManager aiMana;
    
    [Header("Gerenciadores do Jogo")]
    public TurnManager turnManager;

    public IEnumerator ProcessTurn()
    {
        Debug.Log("IA: Pensando...");
        yield return new WaitForSeconds(1.5f); // Pausa dramática

        // CRIAMOS UMA CÓPIA DA LISTA DA MÃO! 
        // Se lermos a mão original e removermos uma carta no meio do loop, a Unity dá erro.
        List<GameObject> cartasNaMao = new List<GameObject>(aiHand.cardsInHand);

        // A IA tenta jogar todas as cartas que conseguir
        foreach (GameObject cardObj in cartasNaMao)
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
                    
                    // 1. Paga a mana
                    aiMana.SpendMana(cardData.mana);
                    
                    // 2. Tira a carta da mão (isso atualiza o leque do inimigo)
                    aiHand.RemoveCardFromHand(cardObj);

                    // 3. Joga a carta na mesa com animação!
                    PlayCardOnBoard(cardObj, emptyZone.transform);

                    // Espera a animação terminar antes de pensar na próxima carta
                    yield return new WaitForSeconds(1.5f); 
                }
            }
        }

        Debug.Log("IA: Fim das jogadas. Passo o turno.");
        yield return new WaitForSeconds(1f);
        turnManager.StartPlayerTurn(); // Devolve o turno para você!
    }

    // ==========================================
    // MÉTODOS AUXILIARES DA IA
    // ==========================================

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
        return null; // Retorna nulo se a mesa estiver cheia para essa classe
    }

    private void PlayCardOnBoard(GameObject cardObj, Transform zone)
    {
        // Prende a carta na DropZone
        cardObj.transform.SetParent(zone);
        
        // ==========================================
        // CORREÇÃO 1: ROTAÇÃO
        // Colocamos 180f no eixo Z para girar a carta de frente para você!
        // (Se por acaso ela ficar de lado, troque para ( -89.98f, 180f, 0f ) )
        // ==========================================
        cardObj.transform.localRotation = Quaternion.Euler(-89.98f, 0f, 180f); 

        // Ativa/Desativa as partes visuais 3D da carta
        CardDrag dragScript = cardObj.GetComponent<CardDrag>();
        if(dragScript != null)
        {
            foreach(GameObject obj in dragScript.objectsToHideOnBoard) if(obj) obj.SetActive(false);
            foreach(GameObject obj in dragScript.objectsToShowOnBoard) if(obj) obj.SetActive(true);
        }
        
        MeshRenderer mesh = cardObj.GetComponent<MeshRenderer>();
        if(mesh != null) mesh.enabled = false;

        // ==========================================
        // CORREÇÃO 2: POSIÇÃO
        // Removi o "- 0.15f" do zone.position.z. Agora ela vai cravar no centro exato da DropZone!
        // ==========================================
        Vector3 finalPos = new Vector3(zone.position.x, zone.position.y + 0.1f, zone.position.z - 0.15f);
        
        cardObj.transform.DOKill();
        cardObj.transform.DOJump(finalPos, jumpPower: 0.7f, numJumps: 1, duration: 1.2f).SetEase(Ease.OutQuad);
    }
}