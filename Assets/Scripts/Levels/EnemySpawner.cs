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
    public int currentWave = 0;
    private bool isSpawning = false;
    public int enemiesSpawnedThisWave = 0;
    public int enemiesKilledThisWave = 0;
    public GameObject rewardScreen;
    public GameObject gameOverScreen;
    public GameObject victoryScreen;
    public TMPro.TextMeshProUGUI waveStatsText;
    public TMPro.TextMeshProUGUI waveNumberText;
    private float waveStartTime;
    public TMPro.TextMeshProUGUI spellRewardText;
    public UnityEngine.UI.Button takeSpellButton;
    private Spell pendingRewardSpell;
    public SpellUI[] spellUISlots;


    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        Debug.Log($"JSONLoader.Instance.levels count: {JSONLoader.Instance.levels?.Count}");
        Debug.Log($"level_selector: {level_selector}");
        CreateDifficultyButtons();
    }
    //func to make the buttons appear when func is called
    void CreateDifficultyButtons()
    {
        foreach (Transform child in level_selector.transform)
        {
            Destroy(child.gameObject);
        }

        List<LevelData> levels = JSONLoader.Instance.levels;

        if (levels == null || levels.Count == 0)
        {
            //Debug.LogError("none of the levels loaded");
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

    //Called when player selects a difficulty level
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

    //Goes to the next wave or shows victory if all the waves are done
    public void NextWave()
    {
        if (currentLevel == null)
        {
            //Debug.LogError("currentLevel is null");
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

    public void ReturnToMenu()
    {
        GameObject[] units = GameObject.FindGameObjectsWithTag("unit");
        foreach (GameObject u in units)
        {
            if (u.GetComponent<EnemyController>() != null)
            {
                GameManager.Instance.RemoveEnemy(u);
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

    //manages the full wave lifecycle: countdown, spawning, and wave end detection
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
        //scale player stats with wave number
        UpdatePlayerStats();

        if (currentLevel.waves > 0)
            waveNumberText.text = $"Wave {currentWave} of {currentLevel.waves}";
        else
            waveNumberText.text = $"Wave {currentWave}";

        waveStartTime = Time.time;

        List<Coroutine> spawnCoroutines = new List<Coroutine>();
        foreach (SpawnData spawn in currentLevel.spawns)
        {
            spawnCoroutines.Add(StartCoroutine(SpawnEnemyType(spawn)));
        }

        //Wait for all the spawning to finish
        foreach (var coroutine in spawnCoroutines)
        {
            yield return coroutine;
        }

        Debug.Log($"All spawning done. Spawned: {enemiesSpawnedThisWave}, Killed so far: {enemiesKilledThisWave}");

        //all enemies dying
        yield return new WaitUntil(() => enemiesKilledThisWave >= enemiesSpawnedThisWave || IsPlayerDead());

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

    //Spawns a specific enemy type using sequence and delay from levels.json
    //Stats are calculated using RPN expressions
    IEnumerator SpawnEnemyType(SpawnData spawn)
    {
        Debug.Log($"Spawning {spawn.enemy} for wave {currentWave}");

        EnemyData baseEnemy = JSONLoader.Instance.enemies.Find(e => e.name == spawn.enemy);
        if (baseEnemy == null)
        {
            Debug.LogError($"Enemy '{spawn.enemy}' not found!");
            yield break;
        }
        //create vars for waves and enemies
        int totalCount = RPNEvaluatorWrapper.Evaluate(spawn.count,
            new Dictionary<string, int> { { "base", 0 }, { "wave", currentWave } });

        int enemyHP = RPNEvaluatorWrapper.Evaluate(spawn.hp,
            new Dictionary<string, int> { { "base", baseEnemy.hp }, { "wave", currentWave } });

        int enemySpeed = RPNEvaluatorWrapper.Evaluate(spawn.speed,
            new Dictionary<string, int> { { "base", baseEnemy.speed }, { "wave", currentWave } });

        int enemyDamage = RPNEvaluatorWrapper.Evaluate(spawn.damage,
            new Dictionary<string, int> { { "base", baseEnemy.damage }, { "wave", currentWave } });

        Debug.Log($"Wave {currentWave} | {spawn.enemy}: count={totalCount}, hp={enemyHP}, speed={enemySpeed}, damage={enemyDamage}");
        //delay spawn as per our instructions :p
        int delay = spawn.delay;
        int[] sequence = spawn.sequence;
        if (sequence == null || sequence.Length == 0)
        {
            sequence = new int[] { 1 };
        }

        int spawned = 0;
        int sequenceInd = 0;

        //spawn total count
        while (spawned < totalCount)
        {
            //group size from sequence array
            int collectionSize = sequence[sequenceInd % sequence.Length];
            int actualcollectionSize = Mathf.Min(collectionSize, totalCount - spawned);

            SpawnPoint spawnPoint = GetSpawnPoint(spawn.location);

            for (int i = 0; i < actualcollectionSize; i++)
            {
                //so enemies do not stack
                Vector2 offset = Random.insideUnitCircle * 1.8f;
                Vector3 position = spawnPoint.transform.position + new Vector3(offset.x, offset.y, 0);

                GameObject newEnemy = Instantiate(enemy, position, Quaternion.identity);

                newEnemy.GetComponent<SpriteRenderer>().sprite =
                    GameManager.Instance.enemySpriteManager.Get(baseEnemy.sprite);

                EnemyController en = newEnemy.GetComponent<EnemyController>();
                //setting tha hp and all from rpn eval exps
                en.hp = new Hittable(enemyHP, Hittable.Team.MONSTERS, newEnemy);
                en.speed = enemySpeed;
                en.attackDamage = enemyDamage;

                //track all counters
                GameManager.Instance.AddEnemy(newEnemy);
                enemiesSpawnedThisWave = enemiesSpawnedThisWave + 1;
                spawned = spawned + 1;
            }

            sequenceInd = sequenceInd + 1;

            if (spawned < totalCount)
            {
                yield return new WaitForSeconds(delay);
            }
        }
    }

    //Finds the correct spawn point based on location string
    SpawnPoint GetSpawnPoint(string location)
    {
        if (location == "random")
        {
            return SpawnPoints[Random.Range(0, SpawnPoints.Length)];
        }

        if (location.StartsWith("random "))
        {
            string type = location.Substring(7);
            //spawn set in unity
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
        if (currentLevel.waves > 0 && currentWave >= currentLevel.waves)
        {
            ShowVictory();
            return;
        }
        float timeTaken = Time.time - waveStartTime;
        waveStatsText.text = $"Wave {currentWave} Complete!\nTime: {timeTaken:F1} seconds";

        //generate reward spell
        PlayerController pc = GameManager.Instance.player.GetComponent<PlayerController>();
        SpellBuilder builder = new SpellBuilder();
        pendingRewardSpell = builder.GenerateRewardSpell(pc.spellcaster, currentWave);
        spellRewardText.text = $"Spell Reward:\n{pendingRewardSpell.GetName()}";
        takeSpellButton.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = "Take Spell";

        rewardScreen.SetActive(true);
    }

    void ShowVictory()
    {
        victoryScreen.SetActive(true);
        Debug.Log("win");
        GameManager.Instance.state = GameManager.GameState.GAMEOVER;
    }

    void ShowGameOver()
    {
        gameOverScreen.SetActive(true);
        Debug.Log("Game Over");
        GameManager.Instance.state = GameManager.GameState.GAMEOVER;
    }

    public void EnemyDied()
    {
        enemiesKilledThisWave++;
        Debug.Log("Enemy died");
    }

    void UpdatePlayerStats()
    {
        PlayerController pc = GameManager.Instance.player.GetComponent<PlayerController>();
        if (pc == null) return;

        var vars = new Dictionary<string, float> { { "wave", currentWave } };

        //update max hp preserving percentage
        int newMaxHP = Mathf.RoundToInt(RPNEvaluatorWrapper.Evaluatef("95 wave 5 * +", vars));
        pc.hp.SetMaxHP(newMaxHP);

        //update mana
        int newMana = Mathf.RoundToInt(RPNEvaluatorWrapper.Evaluatef("90 wave 10 * +", vars));
        pc.spellcaster.max_mana = newMana;
        pc.spellcaster.mana = Mathf.Min(pc.spellcaster.mana, newMana);

        //update mana regen
        pc.spellcaster.mana_reg = Mathf.RoundToInt(RPNEvaluatorWrapper.Evaluatef("10 wave +", vars));

        //update spell power
        pc.spellcaster.spellPower = RPNEvaluatorWrapper.Evaluatef("wave 10 *", vars);

        //update speed
        pc.speed = 5;
    }

    public void TakeSpell()
    {
        var pc = GameManager.Instance.player.GetComponent<PlayerController>();
        if (pc == null || pc.spellcaster == null) return;

        // Use pendingRewardSpell (not currentRewardSpell)
        if (pendingRewardSpell == null)
        {
            Debug.LogWarning("[EnemySpawner] No pending reward spell!");
            return;
        }

        // CRITICAL: Set ownerHittable before adding
        pendingRewardSpell.ownerHittable = pc.hp;

        // Add or replace spell
        if (pc.spellcaster.spells.Count < 4)
        {
            pc.spellcaster.AddSpell(pendingRewardSpell);
        }
        else
        {
            // Replace selected spell (index 0 for now)
            pc.spellcaster.ReplaceSpell(0, pendingRewardSpell);
        }

        // Update UI manually
        UpdateSpellSlots(pc);

        // Hide reward and continue
        rewardScreen.SetActive(false);
        pendingRewardSpell = null;
        NextWave();
    }

    void UpdateSpellSlots(PlayerController pc)
    {
        for (int i = 0; i < spellUISlots.Length; i++)
        {
            if (i < pc.spellcaster.spells.Count)
            {
                spellUISlots[i].gameObject.SetActive(true);
                spellUISlots[i].SetSpell(pc.spellcaster.spells[i]);
            }
            else
            {
                spellUISlots[i].gameObject.SetActive(false);
            }
        }
    }
    public void DropSpell(int index)
    {
        PlayerController pc = GameManager.Instance.player.GetComponent<PlayerController>();
        pc.spellcaster.RemoveSpell(index);

        //hide that spell slot
        if (index < spellUISlots.Length)
        {
            spellUISlots[index].gameObject.SetActive(false);
        }
    }
}