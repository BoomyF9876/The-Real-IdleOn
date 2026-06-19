using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    void Start()
    {
        GameManager.Instance.ChangeState(GameState.Playing);
    }
}
