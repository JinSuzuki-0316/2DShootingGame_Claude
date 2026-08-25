using UnityEngine;

/// <summary>
/// 敵種4：歩行・移動型（ダッカーなど）。
/// 地形の上を歩き、自機を追うように移動して攻撃してくる陸上敵。
/// </summary>
public class WalkerEnemy : EnemyBase
{
    [Header("移動設定")]
    public float walkSpeed = 2f;
    public float groundCheckDistance = 1f;

    [Header("追跡・攻撃設定")]
    public float trackRange = 6f;
    public GameObject bulletPrefab;
    public float shootInterval = 1.8f;
    private float shootTimer;

    private Transform playerTarget;
    private float direction = -1f; // 基本は左向きに進行（プレイヤーがスクロールで右から来る想定）

    protected override void Awake()
    {
        base.Awake();
        shootTimer = shootInterval;
        PlayerController playerObj = Object.FindObjectOfType<PlayerController>();
        if (playerObj != null) playerTarget = playerObj.transform;
    }

    private void Update()
    {
        FollowTerrain();
        TrackPlayer();
        HandleShooting();
    }

    /// <summary>地形の上を歩くため、下方向にRayを飛ばして高さを合わせながら進む</summary>
    private void FollowTerrain()
    {
        Vector3 nextPos = transform.position + Vector3.right * direction * walkSpeed * Time.deltaTime;

        RaycastHit2D hit = Physics2D.Raycast(nextPos + Vector3.up * 0.5f, Vector2.down, groundCheckDistance + 0.5f);
        if (hit.collider != null && hit.collider.GetComponent<GroundTag>() != null)
        {
            nextPos.y = hit.point.y;
            transform.position = nextPos;
        }
        else
        {
            // 足元が地形から外れたら反転（崖・端で折り返す）
            direction *= -1f;
        }
    }

    /// <summary>プレイヤーが近ければ向きをプレイヤー側に合わせる</summary>
    private void TrackPlayer()
    {
        if (playerTarget == null) return;

        float dist = Vector2.Distance(transform.position, playerTarget.position);
        if (dist <= trackRange)
        {
            direction = playerTarget.position.x > transform.position.x ? 1f : -1f;
        }
    }

    private void HandleShooting()
    {
        if (bulletPrefab == null || playerTarget == null) return;

        float dist = Vector2.Distance(transform.position, playerTarget.position);
        if (dist > trackRange) return;

        shootTimer -= Time.deltaTime;
        if (shootTimer <= 0f)
        {
            Vector2 dir = (playerTarget.position - transform.position).normalized;
            GameObject b = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
            EnemyBullet eb = b.GetComponent<EnemyBullet>();
            if (eb != null) eb.direction = dir;
            shootTimer = shootInterval;
        }
    }
}
