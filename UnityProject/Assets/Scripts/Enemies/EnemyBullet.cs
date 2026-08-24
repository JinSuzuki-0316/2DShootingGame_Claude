using UnityEngine;

/// <summary>
/// 敵側が発射する弾の共通スクリプト。
/// タグ "EnemyBullet" を付けたプレハブにアタッチして使用する。
/// </summary>
public class EnemyBullet : MonoBehaviour
{
    public float speed = 6f;
    public Vector2 direction = Vector2.left;
    public float lifeTime = 5f;

    private void Start()
    {
        Destroy(gameObject, lifeTime);

        // 進行方向にスプライトを向ける（任意）
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    private void Update()
    {
        transform.position += (Vector3)(direction.normalized * speed * Time.deltaTime);
    }
}
