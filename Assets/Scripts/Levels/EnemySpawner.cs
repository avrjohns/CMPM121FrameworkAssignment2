using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Collections;
using System.Linq;

public class EnemySpawner : MonoBehaviour
{
    public static EnemySpawner Instance;

    public Image level_selector;
    public GameObject button;
    public GameObject enemy;
    public SpawnPoint[] SpawnPoints;

    public LevelData currentLevel;
    private int currentWave = 0;
    private bool isSpawning = false;

    private int enemiesSpawnedThisWave = 0;
    private int enemiesKilledThisWave = 0;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        //
        Debug.Log($"JSONLoader.Instance.levels count: {JSONLoader.Instance.levels?.Count}");
        Debug.Log($"level_selector: {level_selector}");
        // Debug.Log($"button prefab: {button}");

        CreateDifficultyButtons();
    }

    void CreateDifficultyButtons()
    {
        // Debug.Log($"Creating buttons for {JSONLoader.Instance.levels?.Count} levels");
        foreach (Transform child in level_selector.transform)
        {
            Destroy(child.gameObject);
        }

        List<LevelData> levels = JSONLoader.Instance.levels;

        if (levels == null || levels.Count == 0)
        {
            Debug.LogError("none of the levels loaded");
            return;
        }

        float startY = 130;
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

        currentWave = 1;
        level_selector.gameObject.SetActive(false);
        GameManager.Instance.player.GetComponent<PlayerController>().StartLevel();
        StartCoroutine(SpawnWave());
    }

    public void NextWave()
    {
        if (currentLevel == null)
        {
            Debug.LogError("NextWave() called but currentLevel is null?");
            return;
        }

        if (currentLevel.waves > 0 && currentWave >= currentLevel.waves)
        {
            ShowVictory();
            return;
        }

        currentWave = currentWave + 1;
        StartCoroutine(SpawnWave());
    }

    IEnumerator SpawnWave()
    {
        if (isSpawning) yield break;
        isSpawning = true;

        enemiesSpawnedThisWave = 0;
        enemiesKilledThisWave = 0;

        GameManager.Instance.state = GameManager.GameState.COUNTDOWN;
        GameManager.Instance.countdown = 3;
        for (int i = 3; i > 0; i--)
        {
            yield return new WaitForSeconds(1);
            GameManager.Instance.countdown--;
        }

        GameManager.Instance.state = GameManager.GameState.INWAVE;


        foreach (SpawnData spawn in currentLevel.spawns)
        {
            yield return StartCoroutine(SpawnEnemyType(spawn));
        }

        Debug.Log($"All spawning done. Spawned: {enemiesSpawnedThisWave}, Killed so far: {enemiesKilledThisWave}");


        yield return new WaitUntil(() => enemiesKilledThisWave >= enemiesSpawnedThisWave);

        if (IsPlayerDead())
        {
            ShowGameOver();
            isSpawning = false;
            yield break;
        }

        GameManager.Instance.state = GameManager.GameState.WAVEEND;
        ShowWaveComplete();
        isSpawning = false;
    }

    IEnumerator SpawnEnemyType(SpawnData spawn)
    {

        Debug.Log($"Spawning {spawn.enemy} for wave {currentWave}");

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

        int sequenceInd = 0;

        while (spawned < totalCount)
        {
            int collectionSize = sequence[sequenceInd % sequence.Length];
            int actualcollectionSize = Mathf.Min(collectionSize, totalCount - spawned);

            SpawnPoint spawnPoint = GetSpawnPoint(spawn.location);

            for (int i = 0; i < actualcollectionSize; i++)
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
                enemiesSpawnedThisWave++;
                spawned++;
            }

            sequenceInd = sequenceInd + 1;


            if (spawned < totalCount)
            { yield return new WaitForSeconds(delay); }
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
        Debug.Log($"Wave {currentWave} done");
    }

    void ShowVictory()
    {
        Debug.Log("win");
        GameManager.Instance.state = GameManager.GameState.GAMEOVER;
    }

    void ShowGameOver()
    {
        Debug.Log("Game Over");
        GameManager.Instance.state = GameManager.GameState.GAMEOVER;
    }

    public void EnemyDied()
    {
        enemiesKilledThisWave++;
        Debug.Log("Enemy died");
    }
}