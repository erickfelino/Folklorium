using System;
using System.Collections.Generic;

[Serializable]
public class DeckSaveData
{
    public List<string> mainDeckCards = new List<string>();
    public List<string> selectedSpells = new List<string>(); // máximo 2
}