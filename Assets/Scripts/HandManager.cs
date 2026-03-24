using UnityEngine;
using DG.Tweening;
using Folklorium;
using System.Collections.Generic;

public class HandManager : MonoBehaviour
{
    public DeckManager deckManager;
    public GameObject cardPrefab;
    public Transform handTransform;

    [Header("Configurações 3D da Mão")]
    public float fanSpread = -7.5f;
    public float cardSpacing = 8f;
    public float verticalSpacing = 2f;
    public float depthSpacing = 0.5f;

    [Header("Configurações do DOTween")]
    public float animDuration = 0.3f; // Nova variável que substitui a MoveSpeed
    public bool isEnemyHand = false;
    public ManaManager myManaManager;
    public List<GameObject> cardsInHand = new List<GameObject>();

    private List<Vector3> targetPositions = new List<Vector3>();
    private List<Quaternion> targetRotations = new List<Quaternion>();

    public void DrawCardFromDeck()
    {
        if (deckManager != null)
        {
            Card drawnCard = deckManager.DrawCard();
            
            if (drawnCard != null)
            {
                AddCardToHand(drawnCard);
            }
        }
    }

    public void AddCardToHand(Card cardData)
    {
        GameObject newCard = Instantiate(cardPrefab, handTransform.position, Quaternion.identity, handTransform);
        newCard.transform.localScale = new Vector3(10f, 10f, 10f);
        
        CardDrag drag = newCard.GetComponent<CardDrag>();
        if (drag != null)
        {
            drag.SetManagers(this, myManaManager); 
            
            if (isEnemyHand)
            {
                drag.enabled = false;
        
                Collider col = newCard.GetComponent<Collider>();
                if (col != null) col.enabled = false;

                CardCombat cardCombat = newCard.GetComponent<CardCombat>();
                cardCombat.isEnemy = true;
            }
        }
        
        cardsInHand.Add(newCard);
        targetPositions.Add(Vector3.zero);
        targetRotations.Add(Quaternion.identity);

        CardDisplay display = newCard.GetComponent<CardDisplay>();
        display.cardData = cardData;
        display.UpdateCardDisplay();

        UpdateHandVisuals();
    } 

    public void RemoveCardFromHand(GameObject cardToRemove)
    {
        int index = cardsInHand.IndexOf(cardToRemove);
        if (index != -1)
        {
            cardsInHand.RemoveAt(index);
            targetPositions.RemoveAt(index);
            targetRotations.RemoveAt(index);
            UpdateHandVisuals();
        }
    }

    public void UpdateHandVisuals()
    {
        int cardCount = cardsInHand.Count;
        if (cardCount == 0) return;

        for (int i = 0; i < cardCount; i++)
        {
            float offsetFromCenter = i - (cardCount - 1) / 2f; 
            float normalizedPosition = (cardCount == 1) ? 0f : offsetFromCenter / ((cardCount - 1) / 2f);

            float horizontalOffset = cardSpacing * offsetFromCenter;
            float backwardOffset = verticalSpacing * (normalizedPosition * normalizedPosition);
            float heightOffset = i * depthSpacing; 

            targetPositions[i] = new Vector3(horizontalOffset, heightOffset, -backwardOffset);
            
            float rotationAngle = fanSpread * normalizedPosition;

            if (isEnemyHand)
            {
                targetRotations[i] = Quaternion.Euler(-89.98f, 0f, 180f + rotationAngle); 
            }
            else
            {
                targetRotations[i] = Quaternion.Euler(-89.98f, 0f, rotationAngle);
            }

            CardDrag dragScript = cardsInHand[i].GetComponent<CardDrag>();
            if (dragScript != null && !dragScript.isDragging && !dragScript.isHovering)
            {
                SendCardToHandPosition(i);
            }
        }
    }
    public void SendCardToHandPosition(int index)
    {
        GameObject card = cardsInHand[index];

        card.transform.DOKill(); // Mata animações anteriores para não bugar

        //SetEase(Ease.OutBack) faz a carta dar um pulinho elástico
        card.transform.DOLocalMove(targetPositions[index], animDuration).SetEase(Ease.OutBack);
        card.transform.DOLocalRotateQuaternion(targetRotations[index], animDuration).SetEase(Ease.OutBack);
        card.transform.DOScale(new Vector3(10f, 10f, 10f), animDuration);
    }

    public void TriggerHover(GameObject card)
    {
        int index = cardsInHand.IndexOf(card);
        if (index == -1) return;

        card.transform.DOKill(); 

        Vector3 hoverPos = targetPositions[index] + new Vector3(0f, 0.1f, 0.75f);
        Quaternion hoverRot = Quaternion.Euler(-90f, 0f, 0f);
        Vector3 hoverScale = new Vector3(15f, 15f, 15f);

        card.transform.DOLocalMove(hoverPos, 0.2f).SetEase(Ease.OutCubic); // SetEase(Ease.OutCubic) deixa o movimento mais suave
        card.transform.DOLocalRotateQuaternion(hoverRot, 0.2f);
        card.transform.DOScale(hoverScale, 0.2f).SetEase(Ease.OutBack);
    }

    public void CancelHoverOrDrag(GameObject card)
    {
        int index = cardsInHand.IndexOf(card);
        if (index == -1) return;

        SendCardToHandPosition(index);
    }

    //Encolhe a carta de volta ao tamanho 10 quando clica para arrastar
    public void TriggerDrag(GameObject card)
    {
        card.transform.DOKill();
        Quaternion hoverRot = Quaternion.Euler(-90f, 0f, 0f);
        card.transform.DOLocalRotateQuaternion(hoverRot, 0.1f);
        card.transform.DOScale(new Vector3(10f, 10f, 10f), 0.1f);
    }
}