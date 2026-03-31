using UnityEngine;
using Folklorium;

public class BoardSummonAuraListener : MonoBehaviour
{
    private CardCombat owner;
    private BoardCardPlacedEventChannelSO placedChannel;

    private int attackBonus = 1;
    private int healthBonus = 1;
    private bool ownSideOnly = true;
    private bool isSubscribed = false;

    private void Awake()
    {
        owner = GetComponent<CardCombat>();
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    public void Initialize(BoardCardPlacedEventChannelSO channel, int atk, int hp, bool onlyOwnSide)
    {
        placedChannel = channel;
        attackBonus = atk;
        healthBonus = hp;
        ownSideOnly = onlyOwnSide;

        Subscribe();
    }

    private void Subscribe()
    {
        if (isSubscribed || placedChannel == null)
            return;

        placedChannel.OnEventRaised += HandleBoardCardPlaced;
        isSubscribed = true;
    }

    private void Unsubscribe()
    {
        if (!isSubscribed || placedChannel == null)
            return;

        placedChannel.OnEventRaised -= HandleBoardCardPlaced;
        isSubscribed = false;
    }

    private void HandleBoardCardPlaced(BoardCardPlacedEventPayload payload)
    {
        Debug.Log("chamou o buff 1");
        if (owner == null || owner.isDead)
            return;
        Debug.Log("chamou o buff 2");
        if (payload.card == null)
            return;
        Debug.Log("chamou o buff 3");
        if (payload.card == owner)
            return;
        Debug.Log("chamou o buff 4");
        if (ownSideOnly && payload.card.isEnemy != owner.isEnemy)
            return;
        Debug.Log("chamou o buff 5");
        if (ActionSystem.Instance == null)
            return;
        Debug.Log("chamou o buff 6");       
        ActionSystem.Instance.AddAction(new BuffAction(owner, owner, attackBonus, healthBonus));
        Debug.Log("chamou o buff fim");
    }
}