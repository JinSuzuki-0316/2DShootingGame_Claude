using UnityEngine;

/// <summary>
/// 敵種2：地表・天井設置型（砲台、ローパー）。
/// 固定砲座やミサイル発射基地。自機を狙って弾を撃ち返してくるため
/// 早めの破壊が必要な、地形に固定されたタイプ。
/// </summary>
public class TurretEnemy : EnemyBase
{
    [Header("設置タイプ")]
    public bool isCeilingMounted = false; // trueなら天井設置（下向きに攻撃）

    [Header("索敵・射撃設定")]
    public float detectRange = 8f;
    public GameObject bulletPrefab;
    public float shootInterval = 1.2f;
    public bool aimAtPlayer = true; // falseなら固定方向へ発射

    private float shootTimer;
    private Transform playerTarget;
    public Transform muzzle;

    protected override void Awake()
    {
        base.Awake();
        shootTimer = shootInterval;
        PlayerController playerObj = Object.FindObjectOfType<PlayerController>();
        if (playerObj != null) playerTarget = playerObj.transform;
        if (muzzle == null) muzzle = transform;
    }

    private void Update()
    {
        if (playerTarget == null) return;

        float dist = Vector2.Distance(transform.position, playerTarget.position);
        if (dist <= detectRange)
        {
            shootTimer -= Time.deltaTime;
            if (shootTimer <= 0f)
            {
                Shoot();
                shootTimer = shootInterval;
            }
        }
    }

    private void Shoot()
    {
        if (bulletPrefab == null) return;

        Vector2 dir;
        if (aimAtPlayer && playerTarget != null)
        {
            dir = (playerTarget.position - muzzle.position).normalized;
        }
        else
        {
            dir = isCeilingMounted ? Vector2.down : Vector2.up;
        }

        GameObject b = Instantiate(bulletPrefab, muzzle.position, Quaternion.identity);
        EnemyBullet eb = b.GetComponent<EnemyBullet>();
        if (eb != null) eb.direction = dir;
    }

    // 砲台は地形に固定されているため、通常弾との接触判定のみ有効
    // （体当たりによる自機ダメージは基本的に発生させない設計にしてもよい）
}
