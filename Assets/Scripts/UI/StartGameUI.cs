using UnityEngine;
using UnityEngine.UI;

public class StartGameUI : MonoBehaviour
{
    [SerializeField] Button startBtn;
    [SerializeField] Button quitBtn;

    void Start()
    {
        startBtn.onClick.AddListener(StartNewGame);
        quitBtn.onClick.AddListener(QuitGame);
    }

    void StartNewGame()
    {
        GameManager.Instance.ChangeState(GameState.NewGame);
    }

    void QuitGame()
    {
        GameManager.Instance.ChangeState(GameState.Quit);
    }
}
