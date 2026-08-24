using UnityEngine;

/// <summary>
/// 敵種3：ハッチ（クラブ）。時間差で小型機を無限に吐き出す難敵。
/// 赤色（isRedVariant）を倒すとパワーアップカプセルを落とす。
/// </summary>
public class HatchEnemy : EnemyBase
{
    [Header("湧き出し設定")]
    public GameObject spawnedEnemyPrefab;
    public Transform spawnPoint;
    public float spawnInterval = 2.5f;
    public int maxAlive = 3; // 同時に存在できる子機の上限（無制限に増えすぎないように）

    [Header("色バリエーション")]
    public bool isRedVariant = false; // 赤色個体はカプセルを落とす

    private float spawnTimer;
    private int currentAliveCount = 0;

    protected override void Awake()
    {
        base.Awake();
        spawnTimer = spawnInterval;
        dropsCapsule = isRedVariant; // EnemyBase側のドロップ判定に反映
        if (spawnPoint == null) spawnPoint = transform;
    }

    private void Update()
    {
        spawnTimer -= Time.deltaTime;
        if (spawnTimer <= 0f && currentAliveCount < maxAlive)
        {
            SpawnChild();
            spawnTimer = spawnInterval;
        }
    }

    private void SpawnChild()
    {
        if (spawnedEnemyPrefab == null) return;

        GameObject child = Instantiate(spawnedEnemyPrefab, spawnPoint.position, Quaternion.identity);
        currentAliveCount++;

        // 子機が破壊されたときにカウントを減らすためのフック
        HatchSpawnTracker tracker = child.AddComponent<HatchSpawnTracker>();
        tracker.parentHatch = this;
    }

    public void OnChildDestroyed()
    {
        currentAliveCount = Mathf.Max(0, currentAliveCount - 1);
    }
}

/// <summary>
/// ハッチが生成した子機が破壊された際に、親ハッチへ通知するための補助コンポーネント。
/// </summary>
public class HatchSpawnTracker : MonoBehaviour
{
    public HatchEnemy parentHatch;

    private void OnDestroy()
    {
        if (parentHatch != null)
        {
            parentHatch.OnChildDestroyed();
        }
    }
}
