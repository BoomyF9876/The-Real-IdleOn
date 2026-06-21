using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public enum GameState
{
    MainMenu,
    NewGame,
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
                SceneManager.LoadScene(1);
                break;
            case GameState.Playing:
                Time.timeScale = 1f;
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
