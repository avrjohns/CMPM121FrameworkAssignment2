using UnityEngine;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.IO;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Collections;
using System.Linq;

public class EnemySpawner : MonoBehaviour
{
    public Image level_selector;
    public GameObject button;
    public GameObject enemy;
    public SpawnPoint[] SpawnPoints;

    //creating some private variables to manage levels/waves
    private LevelData currentLevel;
    private int currentWave = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameObject selector = Instantiate(button, level_selector.transform);
        selector.transform.localPosition = new Vector3(0, 130);
        selector.GetComponent<MenuSelectorController>().spawner = this;
        selector.GetComponent<MenuSelectorController>().SetLevel("Start");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartLevel(string levelname)
    {
        //uppdating which level it is (0 since it's the start)
        currentLevel = JSONLoader.Instance.levels[0];
        currentWave = 0;

        level_selector.gameObject.SetActive(false);
        // this is not nice: we should not have to be required to tell the player directly that the level is starting
        GameManager.Instance.player.GetComponent<PlayerController>().StartLevel();
        StartCoroutine(SpawnWave());
    }

    public void NextWave()
    {
        StartCoroutine(SpawnWave());
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

        //Updated to change enemy based on current player level
        foreach (SpawnData spawn in currentLevel.spawns)
        {
            StartCoroutine(SpawnEnemyType(spawn));
        }

        yield return new WaitWhile(() => GameManager.Instance.enemy_count > 0);
        GameManager.Instance.state = GameManager.GameState.WAVEEND;
    }

    IEnumerator SpawnEnemyType(SpawnData spawn)
    {
        //Finds the enemy from the json file
        EnemyData baseEnemy = JSONLoader.Instance.enemies.Find(e => e.name == spawn.enemy);

        //error if it doesn't exist
        if (baseEnemy == null)
        {
            Debug.LogError($"Enemy type {spawn.enemy} not found!");
            yield break;
        }

        int totalCount = 3;
        int spawned = 0;
        int sequenceIndex = 0;

        //Keep spawning until all enemies are spawned
        while (spawned < totalCount)
        {
            //Cycles through a size array to make sure group sizes vary (pattern)
            int groupSize = spawn.sequence[sequenceIndex % spawn.sequence.Length];

            //Makes sure not to overspawn
            int actualGroupSize = Mathf.Min(groupSize, totalCount - spawned);

            //spawns groups at once
            for (int i = 0; i < actualGroupSize; i++)
            {
                SpawnPoint spawnPoint = SpawnPoints[Random.Range(0, SpawnPoints.Length)];
                Vector2 offset = Random.insideUnitCircle * 1.8f;
                Vector3 position = spawnPoint.transform.position + new Vector3(offset.x, offset.y, 0);

                GameObject newEnemy = Instantiate(enemy, position, Quaternion.identity);
                newEnemy.GetComponent<SpriteRenderer>().sprite =
                    GameManager.Instance.enemySpriteManager.Get(baseEnemy.sprite);

                EnemyController en = newEnemy.GetComponent<EnemyController>();
                en.hp = new Hittable(baseEnemy.hp, Hittable.Team.MONSTERS, newEnemy);
                en.speed = baseEnemy.speed;

                GameManager.Instance.AddEnemy(newEnemy);
                spawned++;
            }

            sequenceIndex++;

            //Waits before spawning in next group
            if (spawned < totalCount)
                yield return new WaitForSeconds(spawn.delay);
        }
    }
}
