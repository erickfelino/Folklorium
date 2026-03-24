public class CardEffectContext
{
    public CardCombat source;         // Quem está usando a carta/efeito
    public CardCombat targetCard;     // O lacaio/comandante alvo (pode ser nulo)
    public PlayerHealth targetPlayer; // O jogador alvo (pode ser nulo)

    public bool isEnemySource;        // Para sabermos de qual lado do campo veio
    
    // Podemos adicionar o TurnManager aqui no futuro se um efeito precisar comprar cartas!
}