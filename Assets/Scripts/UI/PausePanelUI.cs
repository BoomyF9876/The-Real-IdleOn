using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PausePanelUI : MonoBehaviour
{
    [Header("PausePanel")]
    [SerializeField] GameObject pausePanel;
    [SerializeField] Button resumeBtn;
    [SerializeField] Button mainMenuBtn;
    [SerializeField] Button quitBtn;

    [Header("ConfirmPanel")]
    [SerializeField] GameObject confirmPanel;
    [SerializeField] Button yesBtn;
    [SerializeField] Button noBtn;

    bool isQuitting = false;

    void Start()
    {
        HideAllPanels();
        resumeBtn.onClick.AddListener(Resume);
        mainMenuBtn.onClick.AddListener(MainMenu);
        quitBtn.onClick.AddListener(Quit);
        yesBtn.onClick.AddListener(Confirm);
        noBtn.onClick.AddListener(Return);
    }

    void Update()
    {
        if (Keyboard.current.escapeKey.wasReleasedThisFrame)
        {
            if (pausePanel.activeInHierarchy)
            {
                if (confirmPanel.activeInHierarchy) Return();
                else Resume();
            }
            else
            {
                confirmPanel.SetActive(false);
                pausePanel.SetActive(true);

                GameManager.Instance.ChangeState(GameState.Pause);
            }
        }
    }

    void HideAllPanels()
    {
        confirmPanel.SetActive(false);
        pausePanel.SetActive(false);
    }

    void Resume()
    {
        HideAllPanels();

        // Resuming from pause always returns to active gameplay.
        // (This previously restored a captured "previousState", but that state was NewGame -
        // the game never enters the Playing state - so resuming reloaded scene 1 and restarted
        // the run. Going straight to Playing just restores Time.timeScale to 1.)
        GameManager.Instance.ChangeState(GameState.Playing);
    }

    void Confirm()
    {
        if (isQuitting)
        {
            GameManager.Instance.ChangeState(GameState.Quit);
        }
        else
        {
            GameManager.Instance.ChangeState(GameState.MainMenu);
        }
    }

    void Return()
    {
        confirmPanel.SetActive(false);
    }

    void MainMenu()
    {
        isQuitting = false;
        confirmPanel.SetActive(true);
    }

    void Quit()
    {
        isQuitting = true;
        confirmPanel.SetActive(true);
    }
}
