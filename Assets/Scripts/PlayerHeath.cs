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

    void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthUI();
    }

    // ==========================================
    // API DE ESTADO (NÍVEL 3) - O Repositório Ignorante
    // ==========================================
    public void ApplyRawStateChange(int healthChange)
    {
        currentHealth += healthChange;
        
        // Garante os limites da vida usando a variável maxHealth (em vez de 12 fixo)
        if (currentHealth < 0) currentHealth = 0;
        if (currentHealth > maxHealth) currentHealth = maxHealth;

        UpdateHealthUI();

        // Se o número for negativo, significa que foi DANO. Então a gente treme!
        if (healthChange < 0)
        {
            transform.DOShakePosition(0.3f, 0.2f, 10, 90f).OnComplete(() => 
            {
                if (currentHealth <= 0)
                {
                    Die();
                }
            });
        }
        // Se for positivo, é CURA. No futuro você pode colocar um efeito visual (verde) aqui!
        else if (healthChange > 0)
        {
            // Efeito visual de cura futuramente...
        }
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
    }
}