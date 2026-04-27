using UnityEngine;
using UnityEngine.UI;

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
        EnemySpawner.Instance.NextWave();
    }
}