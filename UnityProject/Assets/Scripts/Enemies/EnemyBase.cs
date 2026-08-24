using UnityEngine;

/// <summary>
/// 全ての敵に共通する基底クラス。
/// 耐久力、被弾処理、撃破時のスコア加算・カプセルドロップを担う。
/// </summary>
public class EnemyBase : MonoBehaviour
{
    [Header("基本ステータス")]
    public int maxHealth = 1;
    protected int currentHealth;
    public int scoreValue = 100;

    [Header("撃破時の演出・ドロップ")]
    public GameObject explosionPrefab;
    public GameObject powerCapsulePrefab;
    public bool dropsCapsule = false; // ハッチ(赤)など特定の敵のみtrue

    protected virtual void Awake()
    {
        currentHealth = maxHealth;
    }

    public virtual void TakeDamage(int amount)
    {
        currentHealth -= amount;
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        if (explosionPrefab != null)
        {
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        }

        if (dropsCapsule && powerCapsulePrefab != null)
        {
            Instantiate(powerCapsulePrefab, transform.position, Quaternion.identity);
        }

        GameManager.Instance?.AddScore(scoreValue);
        Destroy(gameObject);
    }

    /// <summary>自機との接触（体当たり）ダメージ</summary>
    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // プレイヤー側のダメージ処理はPlayerController側で行う
            Die();
        }
    }
}
