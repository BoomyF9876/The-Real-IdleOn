using UnityEngine;
using DG.Tweening;

public class Coin : MonoBehaviour
{
    [SerializeField] SpriteRenderer sprite;

    public void CoinFadeOut()
    {
        sprite.DOFade(0, 0.2f).OnComplete(() => Destroy(gameObject));
    }
}
