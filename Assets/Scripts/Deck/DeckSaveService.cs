using System.IO;
using UnityEngine;

public static class DeckSaveService
{
    private const string SaveFileName = "folklorium_player_deck.json";

    public static string SavePath => Path.Combine(Application.persistentDataPath, SaveFileName);

    public static void Save(DeckSaveData data)
    {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(SavePath, json);
    }

    public static DeckSaveData Load()
    {
        if (!File.Exists(SavePath))
            return new DeckSaveData();

        string json = File.ReadAllText(SavePath);
        DeckSaveData data = JsonUtility.FromJson<DeckSaveData>(json);
        return data ?? new DeckSaveData();
    }
}