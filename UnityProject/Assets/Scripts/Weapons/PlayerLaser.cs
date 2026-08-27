using UnityEngine;

/// <summary>
/// 貫通力のある直線状レーザー。
/// 複数の敵を貫通してダメージを与える（Destroyしない）。
/// </summary>
public class PlayerLaser : MonoBehaviour
{
    public float speed = 20f;
    public int damage = 1;
    public float lifeTime = 1.2f;
    public GameObject hitEffectPrefab;

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        transform.position += transform.right * speed * Time.deltaTime;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        EnemyBase enemy = other.GetComponent<EnemyBase>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
            if (hitEffectPrefab != null) Instantiate(hitEffectPrefab, other.transform.position, Quaternion.identity);
            AudioManager.Instance?.PlayHit();
            // 貫通するのでDestroyしない
        }
    }
}
