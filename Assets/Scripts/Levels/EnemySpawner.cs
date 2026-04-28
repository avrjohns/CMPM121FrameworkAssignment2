using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class EnemySpawner : MonoBehaviour
{
    public static EnemySpawner Instance;

    public Image level_selector;
    public GameObject button;
    public GameObject enemy;
    public SpawnPoint[] SpawnPoints;
    public GameObject rewardScreen;
    public GameObject gameOverScreen;
    public GameObject victoryScreen;
    public TMPro.TextMeshProUGUI waveStatsText;
    private float waveStartTime;

    private LevelData currentLevel;
    private int currentWave = 0;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        Debug.Log($"JSONLoader.Instance.levels count: {JSONLoader.Instance.levels?.Count}");
        Debug.Log($"level_selector: {level_selector}");
        Debug.Log($"button prefab: {button}");
        CreateDifficultyButtons();
    }

    void CreateDifficultyButtons()
    {
        Debug.Log($"Creating buttons for {JSONLoader.Instance.levels?.Count} levels");
        foreach (Transform child in level_selector.transform)
        {
            Destroy(child.gameObject);
        }

        List<LevelData> levels = JSONLoader.Instance.levels;

        if (levels == null || levels.Count == 0)
        {
            Debug.LogError("No levels loaded! Cannot create buttons.");
            return;
        }
        float startY = 60;
        float spacing = 60;

        for (int i = 0; i < levels.Count; i++)
        {
            GameObject selector = Instantiate(button, level_selector.transform);
            selector.transform.localPosition = new Vector3(0, startY - (i * spacing), 0);

            MenuSelectorController controller = selector.GetComponent<MenuSelectorController>();
            controller.spawner = this;
            controller.SetLevel(levels[i].name);
        }
    }

    public void StartLevel(string levelname)
    {
        currentLevel = JSONLoader.Instance.levels.Find(l => l.name == levelname);
        if (currentLevel == null)
        {
            Debug.LogError($"Level '{levelname}' not found!");
            return;
        }

        currentWave = 0;
        level_selector.gameObject.SetActive(false);
        GameManager.Instance.player.GetComponent<PlayerController>().StartLevel();
        NextWave();
    }

    public void NextWave()
    {
        Debug.Log($"NextWave called, currentLevel = {currentLevel?.name}");
        if (currentLevel == null)
        {
            Debug.LogError("No level selected!");
            return;
        }

        if (currentLevel.waves > 0 && currentWave >= currentLevel.waves)
        {
            ShowVictory();
            return;
        }

        currentWave++;
        StartCoroutine(SpawnWave());
    }

    public void ReturnToMenu()
    {
        GameObject[] units = GameObject.FindGameObjectsWithTag("unit");
        foreach (GameObject u in units)
        {
            if (u.GetComponent<EnemyController>() != null)
            {
                GameManager.Instance.RemoveEnemy(u); // remove from list first
                Destroy(u);
            }
        }

        gameOverScreen.SetActive(false);
        victoryScreen.SetActive(false);
        rewardScreen.SetActive(false);
        currentLevel = null;
        currentWave = 0;
        level_selector.gameObject.SetActive(true);
        GameManager.Instance.state = GameManager.GameState.GAMEOVER;
    }

    IEnumerator SpawnWave()
    {
        GameManager.Instance.state = GameManager.GameState.COUNTDOWN;
        GameManager.Instance.countdown = 3;
        for (int i = 3; i > 0; i--)
        {
            yield return new WaitForSeconds(1);
            GameManager.Instance.countdown--;
        }

        GameManager.Instance.state = GameManager.GameState.INWAVE;
        waveStartTime = Time.time;

        foreach (SpawnData spawn in currentLevel.spawns)
        {
            yield return StartCoroutine(SpawnEnemyType(spawn));
        }

        yield return new WaitWhile(() => GameManager.Instance.enemy_count > 0 && !IsPlayerDead());

        if (IsPlayerDead())
        {
            ShowGameOver();
            yield break;
        }

        GameManager.Instance.state = GameManager.GameState.WAVEEND;
        ShowWaveComplete();
    }

    IEnumerator SpawnEnemyType(SpawnData spawn)
    {
        EnemyData baseEnemy = JSONLoader.Instance.enemies.Find(e => e.name == spawn.enemy);
        if (baseEnemy == null)
        {
            Debug.LogError($"Enemy '{spawn.enemy}' not found!");
            yield break;
        }

        int totalCount = RPNEvaluatorWrapper.Evaluate(spawn.count,
            new Dictionary<string, int> { { "base", 0 }, { "wave", currentWave } });

        int enemyHP = RPNEvaluatorWrapper.Evaluate(spawn.hp,
            new Dictionary<string, int> { { "base", baseEnemy.hp }, { "wave", currentWave } });

        int enemySpeed = RPNEvaluatorWrapper.Evaluate(spawn.speed,
            new Dictionary<string, int> { { "base", baseEnemy.speed }, { "wave", currentWave } });

        int enemyDamage = RPNEvaluatorWrapper.Evaluate(spawn.damage,
            new Dictionary<string, int> { { "base", baseEnemy.damage }, { "wave", currentWave } });

        int delay = spawn.delay;
        int[] sequence = spawn.sequence;
        if (sequence == null || sequence.Length == 0)
        {
            sequence = new int[] { 1 };
        }

        int spawned = 0;
        int sequenceIndex = 0;

        while (spawned < totalCount)
        {
            int groupSize = sequence[sequenceIndex % sequence.Length];
            int actualGroupSize = Mathf.Min(groupSize, totalCount - spawned);

            SpawnPoint spawnPoint = GetSpawnPoint(spawn.location);

            for (int i = 0; i < actualGroupSize; i++)
            {
                Vector2 offset = Random.insideUnitCircle * 1.8f;
                Vector3 position = spawnPoint.transform.position + new Vector3(offset.x, offset.y, 0);

                GameObject newEnemy = Instantiate(enemy, position, Quaternion.identity);

                newEnemy.GetComponent<SpriteRenderer>().sprite =
                    GameManager.Instance.enemySpriteManager.Get(baseEnemy.sprite);

                EnemyController en = newEnemy.GetComponent<EnemyController>();
                en.hp = new Hittable(enemyHP, Hittable.Team.MONSTERS, newEnemy);
                en.speed = enemySpeed;
                en.attackDamage = enemyDamage;

                GameManager.Instance.AddEnemy(newEnemy);
                spawned++;
            }

            sequenceIndex++;

            if (spawned < totalCount)
            {
                yield return new WaitForSeconds(delay);
            }
        }
    }

    SpawnPoint GetSpawnPoint(string location)
    {
        if (location == "random")
        {
            return SpawnPoints[Random.Range(0, SpawnPoints.Length)];
        }

        if (location.StartsWith("random "))
        {
            string type = location.Substring(7);
            SpawnPoint.SpawnName spawnName = SpawnPoint.SpawnName.RED;
            if (type == "green") spawnName = SpawnPoint.SpawnName.GREEN;
            else if (type == "bone") spawnName = SpawnPoint.SpawnName.BONE;

            SpawnPoint[] matching = System.Array.FindAll(SpawnPoints, sp => sp.kind == spawnName);
            if (matching.Length > 0)
            {
                return matching[Random.Range(0, matching.Length)];
            }
        }

        return SpawnPoints[Random.Range(0, SpawnPoints.Length)];
    }

    bool IsPlayerDead()
    {
        if (GameManager.Instance.player == null) return true;
        PlayerController pc = GameManager.Instance.player.GetComponent<PlayerController>();
        return pc == null || pc.hp.hp <= 0;
    }

    void ShowWaveComplete()
    {
        // if this was the last wave show victory instead
        if (currentLevel.waves > 0 && currentWave >= currentLevel.waves)
        {
            ShowVictory();
            return;
        }

        float timeTaken = Time.time - waveStartTime;
        waveStatsText.text = $"Wave {currentWave} Complete!\nTime: {timeTaken:F1} seconds";

        rewardScreen.SetActive(true);
        Debug.Log($"Wave {currentWave} complete! Click Continue.");
    }

    void ShowVictory()
    {
        victoryScreen.SetActive(true);
        Debug.Log("Victory! All waves survived!");
        GameManager.Instance.state = GameManager.GameState.GAMEOVER;
    }

    void ShowGameOver()
    {
        gameOverScreen.SetActive(true);
        Debug.Log("Game Over! You died!");
        GameManager.Instance.state = GameManager.GameState.GAMEOVER;
    }

    public void EnemyDied()
    {
        Debug.Log("Enemy died notification received");
    }
}