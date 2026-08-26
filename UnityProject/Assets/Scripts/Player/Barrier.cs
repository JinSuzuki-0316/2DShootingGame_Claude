using UnityEngine;

/// <summary>
/// 自機の周囲を覆い、敵弾・敵機からのダメージを数回防ぐバリア。
/// 耐久回数がなくなると自動的に消滅する。
/// </summary>
public class Barrier : MonoBehaviour
{
    public int maxHits = 5;
    private int remainingHits;

    [Header("見た目のフェード演出用")]
    public SpriteRenderer sprite;

    // 破壊時に自機側の参照を確実にクリアするための持ち主参照
    public PlayerController owner;

    private void Awake()
    {
        remainingHits = maxHits;
        if (sprite == null) sprite = GetComponent<SpriteRenderer>();
    }

    /// <summary>1回分のダメージを吸収する</summary>
    public void TakeHit()
    {
        remainingHits--;
        UpdateVisual();

        if (remainingHits <= 0)
        {
            Break();
        }
    }

    private void UpdateVisual()
    {
        if (sprite == null) return;
        float ratio = (float)remainingHits / maxHits;
        Color c = sprite.color;
        c.a = Mathf.Lerp(0.3f, 1f, ratio);
        sprite.color = c;
    }

    private void Break()
    {
        // 破損エフェクトなどをここに追加可能
        if (owner != null)
        {
            owner.NotifyBarrierDestroyed(this);
        }
        Destroy(gameObject);
    }
}
