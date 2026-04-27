using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameOverUIManager : MonoBehaviour
{
    public GameObject victoryPanel;
    public GameObject gameOverPanel;
    public Button returnToMenuButton;

    void Update()
    {
        if (GameManager.Instance.state != GameManager.GameState.GAMEOVER)
        {
            victoryPanel.SetActive(false);
            gameOverPanel.SetActive(false);
            return;
        }

        if (IsPlayerDead())
        {
            gameOverPanel.SetActive(true);
            victoryPanel.SetActive(false);
        }
        else
        {
            victoryPanel.SetActive(true);
            gameOverPanel.SetActive(false);
        }
    }

    bool IsPlayerDead()
    {
        if (GameManager.Instance.player == null) return true;
        var pc = GameManager.Instance.player.GetComponent<PlayerController>();
        return pc == null || pc.hp.hp <= 0;
    }

    public void ReturnToMenu()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}