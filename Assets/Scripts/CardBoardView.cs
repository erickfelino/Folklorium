using System;
using UnityEngine;
using DG.Tweening;
using Folklorium;
public class CardBoardView : MonoBehaviour
{
    [Header("Token Collider")]
    public Vector3 tokenColliderCenter = new Vector3(0f, -0.012f, 0f);
    public Vector3 tokenColliderSize = new Vector3(0.056f, 0.04f, 0.002f);
    
    [Header("Visuais no Board")]
    [SerializeField] private GameObject[] objectsToHideOnBoard;
    [SerializeField] private GameObject[] objectsToShowOnBoard;

    [Header("Glow")]
    [SerializeField] private GameObject dragGlow;

    public void PlayPlacementAnimation(BoardSlot targetSlot, bool isEnemy, Action onComplete = null)
    {
        if (targetSlot == null)
            return;

        CardDrag drag = GetComponent<CardDrag>();
        if (drag != null)
        {
            drag.MarkAsPlayed();
        }

        transform.SetParent(targetSlot.transform);

        BoxCollider boxCol = GetComponent<BoxCollider>();
        if (boxCol != null)
        {
            boxCol.enabled = true;
            boxCol.center = tokenColliderCenter;
            boxCol.size = tokenColliderSize;
        }

        foreach (GameObject obj in objectsToHideOnBoard)
            if (obj) obj.SetActive(false);

        foreach (GameObject obj in objectsToShowOnBoard)
            if (obj) obj.SetActive(true);

        MeshRenderer mesh = GetComponent<MeshRenderer>();
        if (mesh != null)
            mesh.enabled = false;

        Vector3 finalPos = new Vector3(
            targetSlot.transform.position.x,
            targetSlot.transform.position.y + 0.1f,
            targetSlot.transform.position.z - 0.15f
        );

        transform.localRotation = isEnemy
            ? Quaternion.Euler(-90f, 0f, 180f)
            : Quaternion.Euler(-90f, 0f, 0f);

        Vector3 targetScale = targetSlot.transform.localScale;
        float fatorDeCompensacao = 50f;
        float espessuraValue = 0.5f;

        Vector3 finalScale = new Vector3(
            targetScale.x * fatorDeCompensacao,
            targetScale.z * fatorDeCompensacao,
            espessuraValue
        );

        transform.DOKill();

        Sequence seq = DOTween.Sequence();
        seq.Join(transform.DOScale(finalScale, 1.2f).SetEase(Ease.OutQuad));
        seq.Join(transform.DOJump(finalPos, jumpPower: 0.7f, numJumps: 1, duration: 1f).SetEase(Ease.OutQuad));
        seq.OnComplete(() => onComplete?.Invoke());
    }

    public void SetupBoardVisuals()
    {
        AdjustCollider();
        AdjustGlow();
    }

    void AdjustCollider()
    {
        BoxCollider boxCol = GetComponent<BoxCollider>();
        if (boxCol == null) return;

        boxCol.enabled = true;
        boxCol.center = tokenColliderCenter;
        boxCol.size = tokenColliderSize;
    }

    void AdjustGlow()
    {
        if (dragGlow == null) return;

        dragGlow.transform.localPosition = tokenColliderCenter;

        dragGlow.transform.localScale = new Vector3(
            tokenColliderSize.x * 1.05f,
            tokenColliderSize.y * 1.05f,
            tokenColliderSize.z
        );
    }
}