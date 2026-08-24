using UnityEngine;

/// <summary>
/// 敵種1：画面上方・前方から特定の軌道で群れをなして現れる小型機。
/// AnimationCurve でサイン波や旋回など様々な軌道を表現できる。
/// </summary>
public class SmallSwarmEnemy : EnemyBase
{
    public enum PathType { StraightLeft, SineWave, DiveCurve }

    [Header("移動設定")]
    public PathType pathType = PathType.SineWave;
    public float moveSpeed = 4f;
    public float amplitude = 1.5f;   // サイン波の振幅
    public float frequency = 2f;     // サイン波の周期

    [Header("射撃設定")]
    public bool canShoot = true;
    public GameObject bulletPrefab;
    public float shootInterval = 1.5f;
    private float shootTimer;

    private Vector3 startPos;
    private float timeAlive = 0f;
    private Transform playerTarget;

    protected override void Awake()
    {
        base.Awake();
        startPos = transform.position;
        shootTimer = shootInterval;
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) playerTarget = playerObj.transform;
    }

    private void Update()
    {
        timeAlive += Time.deltaTime;
        Move();
        HandleShooting();

        if (transform.position.x < -12f)
        {
            Destroy(gameObject);
        }
    }

    private void Move()
    {
        switch (pathType)
        {
            case PathType.StraightLeft:
                transform.position += Vector3.left * moveSpeed * Time.deltaTime;
                break;

            case PathType.SineWave:
                float x = startPos.x - moveSpeed * timeAlive;
                float y = startPos.y + Mathf.Sin(timeAlive * frequency) * amplitude;
                transform.position = new Vector3(x, y, startPos.z);
                break;

            case PathType.DiveCurve:
                // 前半は下降、後半は左へ直進する簡易な急降下軌道
                float dx = -moveSpeed * timeAlive;
                float dy = -Mathf.Min(timeAlive * 2f, amplitude * 2f);
                transform.position = startPos + new Vector3(dx, dy, 0);
                break;
        }
    }

    private void HandleShooting()
    {
        if (!canShoot || bulletPrefab == null || playerTarget == null) return;

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
