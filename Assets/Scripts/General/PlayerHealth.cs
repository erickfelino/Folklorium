using UnityEngine;
using TMPro;
using DG.Tweening;

public class PlayerHealth : MonoBehaviour
{
    [Header("Configurações de Vida")]
    public int maxHealth = 12;
    public int currentHealth;

    [Header("Interface")]
    public TMP_Text healthText;

    private bool isDead = false;

    void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthUI();
    }

    public void ApplyRawStateChange(int healthChange)
    {
        if (isDead) return;

        currentHealth += healthChange;

        if (currentHealth < 0) currentHealth = 0;
        if (currentHealth > maxHealth) currentHealth = maxHealth;

        UpdateHealthUI();

        if (healthChange < 0)
        {
            transform.DOShakePosition(0.3f, 0.2f, 10, 90f).OnComplete(() =>
            {
                CheckDeath();
            });
        }
        else
        {
            CheckDeath();
        }
    }

    private void CheckDeath()
    {
        if (isDead) return;
        if (currentHealth > 0) return;

        isDead = true;
        Debug.Log(gameObject.name + " foi derrotado! FIM DE JOGO!");

        if (GameFlowManager.Instance != null)
        {
            if (CompareTag("EnemyHealth"))
            {
                GameFlowManager.Instance.TriggerVictory();
            }
            else
            {
                GameFlowManager.Instance.TriggerDefeat();
            }
        }
    }

    private void UpdateHealthUI()
    {
        if (healthText != null)
        {
            healthText.text = currentHealth.ToString();
        }
    }
}