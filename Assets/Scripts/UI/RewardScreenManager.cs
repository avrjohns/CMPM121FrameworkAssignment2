using UnityEngine;
using UnityEngine.UI;
using TMPro;

// attempting to fix what the text displays
public class WaveInfoDisplay : MonoBehaviour
{
    public TextMeshProUGUI waveText;

    void Update()
    {
        if (GameManager.Instance.state == GameManager.GameState.WAVEEND)
        {
            waveText.text = $"Wave {EnemySpawner.Instance.currentWave} Complete!\nEnemies killed: {EnemySpawner.Instance.enemiesKilledThisWave}\nClick Continue";
        }}}

public class RewardScreenManager : MonoBehaviour
{
    public GameObject rewardUI;
    public Button continueButton; 

    void Start()
    {
        if (continueButton != null) { continueButton.onClick.AddListener(OnContinueClicked);}
    }

    void Update()
    {
        //
        if (GameManager.Instance == null) return;
        if (GameManager.Instance.state == GameManager.GameState.WAVEEND)
        {
            rewardUI.SetActive(true);
        }
        else
        {
            rewardUI.SetActive(false);
        }
    }

    void OnContinueClicked()
    {
        rewardUI.SetActive(false);
        if (EnemySpawner.Instance == null)
        {
            Debug.LogError("EnemySpawner instance null");
            return;
        }

        if (EnemySpawner.Instance.currentLevel == null)
        {
            Debug.LogError("currentLevel is null! Cannot continue");
            return;
        }
        EnemySpawner.Instance.NextWave();
    }
}