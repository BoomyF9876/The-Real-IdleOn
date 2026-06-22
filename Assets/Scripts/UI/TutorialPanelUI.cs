using UnityEngine;
using UnityEngine.UI;

public class TutorialPanelUI : MonoBehaviour
{
    [SerializeField] GameObject tutorialPanel;
    [SerializeField] Button continueBtn;

    void Start()
    {
        HidePanel();
        continueBtn.onClick.AddListener(StartGame);
        if (GameManager.Instance.showTutorial)
        {
            GameManager.Instance.ChangeState(GameState.Tutorial);
            tutorialPanel.SetActive(true);
        }
    }

    void HidePanel()
    {
        tutorialPanel.SetActive(false);
    }

    void StartGame()
    {
        HidePanel();
        GameManager.Instance.ChangeState(GameState.Playing);
    }
}
