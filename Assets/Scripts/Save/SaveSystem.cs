using System;
using System.IO;
using UnityEngine;

[Serializable]
public class PlayerSaveData
{
    public int saveVersion = 1;
    public int completedMissions = 0;
    public int totalXP = 0;
    public int highScore = 0;
    public string unlockedAircraft = "Vajra Mk-1";
    public float masterVolume = 1.0f;
    public float sfxVolume = 1.0f;
}

public static class SaveSystem
{
    private static string SavePath => Path.Combine(Application.persistentDataPath, "operation_sindoor_save.json");

    public static void Save(PlayerSaveData data)
    {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(SavePath, json);
    }

    public static PlayerSaveData Load()
    {
        if (!File.Exists(SavePath))
        {
            var defaultData = new PlayerSaveData();
            Save(defaultData);
            return defaultData;
        }

        string json = File.ReadAllText(SavePath);
        return JsonUtility.FromJson<PlayerSaveData>(json);
    }
}
