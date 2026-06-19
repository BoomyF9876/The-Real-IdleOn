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

public enum PlayerState
{
    Normal
}

public class GameManager : MonoBehaviour
{
    private GameState currentState = GameState.MainMenu;
    private PlayerState playerState = PlayerState.Normal;
    public static GameManager Instance;
    public event Action<GameState> OnGameStateChanged;
    public event Action<PlayerState> OnPlayerStateChanged;
    public GameState GameState { get { return currentState; } }
    public PlayerState PlayerState { get { return playerState; } }

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

    public void Quit()
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

    public void ChangeState(PlayerState newState)
    {
        playerState = newState;
        OnPlayerStateChanged?.Invoke(newState);
        HandlePlayerStateChange();
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

    private void HandlePlayerStateChange()
    {
        switch (playerState)
        {
            default:
                break;
        }
    }
}
