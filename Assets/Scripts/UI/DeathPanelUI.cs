using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class DeathPanelUI : MonoBehaviour
{
    [Header("PausePanel")]
    [SerializeField] GameObject gameOverPanel;
    [SerializeField] Button restartBtn;
    [SerializeField] Button mainMenuBtn;
    [SerializeField] Button quitBtn;

    void Start()
    {
        gameOverPanel.SetActive(false);
        restartBtn.onClick.AddListener(Restart);
        mainMenuBtn.onClick.AddListener(MainMenu);
        quitBtn.onClick.AddListener(Quit);
        PlayerController.Instance.Stats.OnPlayerDied += ShowPanel;
    }

    void OnDisable()
    {
        PlayerController.Instance.Stats.OnPlayerDied -= ShowPanel;
    }

    void ShowPanel()
    {
        gameOverPanel.SetActive(true);
    }

    void Restart()
    {
        GameManager.Instance.ChangeState(GameState.NewGame);
    }

    void MainMenu()
    {
        GameManager.Instance.ChangeState(GameState.MainMenu);
    }

    void Quit()
    {
        GameManager.Instance.ChangeState(GameState.Quit);
    }
}
