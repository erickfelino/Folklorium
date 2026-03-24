using UnityEngine;

public class EndTurnButton : MonoBehaviour
{
    [Header("Gerenciadores")]
    public TurnManager turnManager;
    public ManaManager playerManaManager;

    [Header("Elementos Visuais")]
    public GameObject canSelectGlow;      
    public MeshRenderer buttonSurface;    
    public ParticleSystem magicLightTurn; 

    [Header("Cores das Partículas")]
    public Color colorTurnActive = Color.green;
    public Color colorNoMana = Color.yellow;
    public Color colorOpponentTurn = Color.red;

    private Material surfaceMat;
    private Color originalSurfaceColor;

    void Start()
    {
        if (canSelectGlow != null) canSelectGlow.SetActive(false);

        if (buttonSurface != null) 
        {
            surfaceMat = buttonSurface.material;
            originalSurfaceColor = surfaceMat.color; 
        }
        
        if (playerManaManager != null)
        {
            playerManaManager.OnManaChanged += UpdateManaVisuals;
        }

        if (turnManager != null)
        {
            turnManager.OnTurnChanged += HandleTurnChanged; 
        }
    }

    void OnDestroy()
    {
        if (playerManaManager != null) playerManaManager.OnManaChanged -= UpdateManaVisuals;
        if (turnManager != null) turnManager.OnTurnChanged -= HandleTurnChanged;
    }

    private void UpdateManaVisuals(int currentMana)
    {
        if (!turnManager.isPlayerTurn) return;

        if (currentMana <= 0)
        {
            ChangeParticleColor(colorNoMana);
            if (surfaceMat != null) 
            {
                surfaceMat.color = colorNoMana;
            }
        }
        else
        {
            ChangeParticleColor(colorTurnActive);
        }
    }

    private void HandleTurnChanged(bool isPlayerTurn)
    {
        if (surfaceMat != null) 
        {
            surfaceMat.color = isPlayerTurn ? originalSurfaceColor : colorOpponentTurn;
        }
        
        ChangeParticleColor(isPlayerTurn ? colorTurnActive : colorOpponentTurn);

        if (isPlayerTurn && playerManaManager != null)
        {
            UpdateManaVisuals(playerManaManager.currentMana);
        }
    }

    void OnMouseEnter()
    {
        if (turnManager.isPlayerTurn) canSelectGlow.SetActive(true);
    }

    void OnMouseExit()
    {
        canSelectGlow.SetActive(false);
    }

    void OnMouseDown()
    {
        if (turnManager.isPlayerTurn)
        {
            canSelectGlow.SetActive(false); 
            turnManager.EndPlayerTurn();
        }
    }

    private void ChangeParticleColor(Color newColor)
    {
        if (magicLightTurn != null)
        {
            var main = magicLightTurn.main;
            if (main.startColor.color != newColor) 
            {
                main.startColor = newColor;
            }
        }
    }
}