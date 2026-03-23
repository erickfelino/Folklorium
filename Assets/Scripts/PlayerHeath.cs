using UnityEngine;
using TMPro; // Usando TextMeshPro para o texto da vida
using DG.Tweening; // Para a nossa tremedeira de dano!

public class PlayerHealth : MonoBehaviour
{
    [Header("Configurações de Vida")]
    public int maxHealth = 12; // Vida inicial padrão
    private int currentHealth;

    [Header("Interface")]
    public TMP_Text healthText; // Arraste o texto da vida aqui no Inspector

    void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthUI();
    }

    // Método que as cartas vão chamar quando atacarem!
    public void PlayerTakeDamage(int damageAmount)
    {
        currentHealth -= damageAmount;
        
        // Garante que a vida não fique negativa
        if (currentHealth < 0) currentHealth = 0; 

        UpdateHealthUI();

        transform.DOShakePosition(0.3f, 0.2f, 10, 90f).OnComplete(() => 
        {
            if (currentHealth <= 0)
            {
                Die();
            }
        });
    }

    private void UpdateHealthUI()
    {
        if (healthText != null)
        {
            healthText.text = currentHealth.ToString();
        }
    }

    private void Die()
    {
        Debug.Log(gameObject.name + " foi Derrotado! FIM DE JOGO!");
        // Aqui você chamará a tela de Vitória/Derrota no futuro
    }
}