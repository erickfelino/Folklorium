using UnityEngine;

public class EndTurnButton : MonoBehaviour
{
    [Header("Gerenciadores")]
    public TurnManager turnManager;
    public ManaManager manaManager;

    [Header("Elementos Visuais")]
    public GameObject canSelectGlow;      // AGORA É UM GAMEOBJECT! (Para ligar e desligar a visibilidade)
    public MeshRenderer buttonSurface;    // O material de pedra/água
    public ParticleSystem magicLightTurn; // O sistema de partículas

    [Header("Cores das Partículas")]
    public Color colorTurnActive = Color.green;
    public Color colorNoMana = Color.yellow;
    public Color colorOpponentTurn = Color.red;

    private Material surfaceMat;
    private Color originalSurfaceColor;

    void Start()
    {
        // Garante que o glow comece desligado
        if (canSelectGlow != null) canSelectGlow.SetActive(false);

        if (buttonSurface != null) 
        {
            surfaceMat = buttonSurface.material;
            originalSurfaceColor = surfaceMat.color; 
        }
    }

    void Update()
    {
        // Fica vigiando a mana para mudar a cor da partícula
        if (turnManager != null && turnManager.isPlayerTurn)
        {
            if (manaManager.currentMana <= 0)
            {
                ChangeParticleColor(colorNoMana);
            }
            else
            {
                ChangeParticleColor(colorTurnActive);
            }
        }
    }

    // ===============================
    // GATILHOS DO MOUSE (HOVER E CLIQUE)
    // ===============================

    void OnMouseEnter()
    {
        // O Hover só acende o Glow se for o turno do jogador
        if (turnManager.isPlayerTurn && canSelectGlow != null)
        {
            canSelectGlow.SetActive(true);
        }
    }

    void OnMouseExit()
    {
        // Desliga o Glow quando o mouse sai de cima
        if (canSelectGlow != null)
        {
            canSelectGlow.SetActive(false);
        }
    }

    void OnMouseDown()
    {
        if (turnManager.isPlayerTurn)
        {
            if (canSelectGlow != null) canSelectGlow.SetActive(false); // Esconde pra não bugar
            turnManager.EndPlayerTurn();
        }
    }

    // ===============================
    // CONTROLE DE CORES
    // ===============================

    public void SetOpponentTurnVisuals()
    {
        // A base fica vermelha e as partículas também
        if (surfaceMat != null) surfaceMat.color = colorOpponentTurn;
        ChangeParticleColor(colorOpponentTurn);
    }

    public void SetPlayerTurnVisuals()
    {
        // A base volta pra cor de água original
        if (surfaceMat != null) surfaceMat.color = originalSurfaceColor; 
        ChangeParticleColor(colorTurnActive); 
    }

    // Método auxiliar para evitar repetição de código
    private void ChangeParticleColor(Color newColor)
    {
        if (magicLightTurn != null)
        {
            var main = magicLightTurn.main;
            // Só troca se a cor for diferente para não reescrever a cada frame
            if (main.startColor.color != newColor) 
            {
                main.startColor = newColor;
            }
        }
    }
}