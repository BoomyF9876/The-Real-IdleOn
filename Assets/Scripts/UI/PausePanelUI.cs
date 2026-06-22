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
    private GameState previousState;

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
                previousState = GameManager.Instance.GameState;
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

        GameManager.Instance.ChangeState(previousState);
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
