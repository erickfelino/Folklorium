using UnityEngine;

public class TargetingArrow : MonoBehaviour
{
    [Header("Configurações Visuais")]
    public LineRenderer lineRenderer;
    public int segments = 20; 
    
    // Substituímos a "curveHeight" fixa por estas duas configurações dinâmicas:
    [Tooltip("O quanto a curva sobe baseado na distância do alvo")]
    public float curveMultiplier = 0.3f; 
    [Tooltip("A altura máxima que a curva pode chegar para não ir pro espaço")]
    public float maxCurveHeight = 3f;

    void Start()
    {
        ShowArrow(false);
        if (lineRenderer != null) lineRenderer.positionCount = segments;
    }

    public void ShowArrow(bool show)
    {
        if (lineRenderer != null) lineRenderer.enabled = show;
    }

    // Adicione esta função dentro do seu TargetingArrow.cs

    public void SetColor(Color newColor)
    {
        // Se você usa LineRenderer:
        LineRenderer line = GetComponent<LineRenderer>();
        if (line != null)
        {
            line.startColor = newColor;
            line.endColor = newColor;
            
            // Se a sua seta tem um material específico que brilha (Emission), 
            // talvez precise mudar a cor do material também:
            // line.material.color = newColor;
            // line.material.SetColor("_EmissionColor", newColor); 
        }

        // Se você tem um Sprite na ponta da seta (o triângulo), mude a cor dele também!
        // SpriteRenderer tip = transform.GetChild(0).GetComponent<SpriteRenderer>();
        // if (tip != null) tip.color = newColor;
    }

    public void UpdateArrow(Vector3 startPoint, Vector3 endPoint)
    {
        if (lineRenderer == null || !lineRenderer.enabled) return;

        // 1. Descobrimos a distância real entre a carta e o alvo
        float distance = Vector3.Distance(startPoint, endPoint);

        // 2. Achamos o centro geográfico entre os dois pontos
        Vector3 midPoint = startPoint + (endPoint - startPoint) / 2f;

        // 3. A MÁGICA AQUI: A altura é proporcional à distância!
        // Usamos Clamp para garantir que ela não passe do limite máximo nem fique reta demais (0.2f mínimo)
        float dynamicHeight = distance * curveMultiplier;
        midPoint.y += Mathf.Clamp(dynamicHeight, 0.2f, maxCurveHeight);

        // Desenha os segmentos da curva
        for (int i = 0; i < segments; i++)
        {
            float t = i / (float)(segments - 1);
            Vector3 position = CalculateQuadraticBezierPoint(t, startPoint, midPoint, endPoint);
            lineRenderer.SetPosition(i, position);
        }
    }

    private Vector3 CalculateQuadraticBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;
        
        Vector3 p = uu * p0; 
        p += 2 * u * t * p1; 
        p += tt * p2; 
        
        return p;
    }
}