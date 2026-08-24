using UnityEngine;

/// <summary>
/// プレイヤーの通常弾・ダブル弾に使用する直進弾。
/// direction を変えるだけでダブルの斜め弾にも流用できる。
/// </summary>
public class PlayerBullet : MonoBehaviour
{
    public float speed = 12f;
    public Vector2 direction = Vector2.right;
    public int damage = 1;
    public float lifeTime = 3f;

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        transform.position += (Vector3)(direction.normalized * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        EnemyBase enemy = other.GetComponent<EnemyBase>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}
