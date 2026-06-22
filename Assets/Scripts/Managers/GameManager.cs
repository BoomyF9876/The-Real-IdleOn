using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public enum GameState
{
    MainMenu,
    NewGame,
    Tutorial,
    Playing,
    Pause,
    GameOver,
    GameWin,
    Quit
}


public class GameManager : MonoBehaviour
{
    private GameState currentState = GameState.MainMenu;
    public static GameManager Instance;
    public bool showTutorial = true;
    public event Action<GameState> OnGameStateChanged;
    public GameState GameState { get { return currentState; } }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Quit()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public void ChangeState(GameState newState)
    {
        currentState = newState;
        OnGameStateChanged?.Invoke(newState);
        HandleGameStateChange();
    }

    private void HandleGameStateChange()
    {
        switch (currentState)
        {
            case GameState.MainMenu:
                Time.timeScale = 1f;
                SceneManager.LoadScene(0);
                break;
            case GameState.NewGame:
                // Always start a new run unpaused. Without this, restarting from the
                // GameOver screen (which set timeScale to 0) would load a frozen game.
                Time.timeScale = 1f;
                SceneManager.LoadScene(1);
                break;
            case GameState.Playing:
                Time.timeScale = 1f;
                break;
            case GameState.Tutorial:
                showTutorial = false;
                Time.timeScale = 0f;
                break;
            case GameState.Pause:
                Time.timeScale = 0f;
                break;
            case GameState.GameOver:
                Time.timeScale = 0f;
                break;
            case GameState.GameWin:
                Time.timeScale = 0f;
                break;
            case GameState.Quit:
                Quit();
                break;
        }
    }
}
