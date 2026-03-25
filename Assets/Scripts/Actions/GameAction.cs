using System.Collections;
using UnityEngine;

// O Nível 3 trabalha com o conceito de Corotina para gerenciar o tempo das ações
public abstract class GameAction
{
    // A única ordem que o ActionSystem sabe dar para o ticket
    public abstract IEnumerator Perform();
}