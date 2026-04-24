using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EnemyData
{
    public string name;
    public int sprite;
    public int hp;
    public int speed;
    public int damage;
}

[System.Serializable]
public class SpawnData
{
    public string enemy;
    public string count;
    public int delay = 2;
    public int[] sequence = new int[] { 1 };
    public string location = "random";
    public string hp = "base";
    public string speed = "base";
    public string damage = "base";
}

[System.Serializable]
public class LevelData
{
    public string name;
    public int waves;
    public SpawnData[] spawns;
}

[System.Serializable]
public class EnemyListWrapper
{
    public List<EnemyData> enemies;
}

[System.Serializable]
public class LevelListWrapper
{
    public List<LevelData> levels;
}

public class JSONLoader : MonoBehaviour
{
    public static JSONLoader Instance;

    public List<EnemyData> enemies;
    public List<LevelData> levels;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        LoadData();
    }

    public void LoadData()
    {
        TextAsset enemiesText = Resources.Load<TextAsset>("enemies");
        TextAsset levelsText = Resources.Load<TextAsset>("levels");

        if (enemiesText != null)
        {
            EnemyListWrapper wrapper = JsonUtility.FromJson<EnemyListWrapper>(enemiesText.text);
            enemies = wrapper.enemies;
            Debug.Log($"Loaded {enemies.Count} enemy types");
        }
        else
        {
            Debug.LogError("enemies.json not found in Resources!");
        }

        if (levelsText != null)
        {
            LevelListWrapper wrapper = JsonUtility.FromJson<LevelListWrapper>(levelsText.text);
            levels = wrapper.levels;
            Debug.Log($"Loaded {levels.Count} difficulty levels");
        }
        else
        {
            Debug.LogError("levels.json not found in Resources!");
        }
    }
}