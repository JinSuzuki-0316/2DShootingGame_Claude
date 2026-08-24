using System.Collections;
using UnityEngine;

/// <summary>
/// 敵種5：大型・中ボス（ゴーレム、ビッグアイ）。
/// 耐久力が高く、ステージの中盤や要所で進路を塞ぐように立ちはだかる。
/// 複数の攻撃パターンをローテーションする簡易ステートマシン。
/// </summary>
public class BossEnemy : EnemyBase
{
    public enum AttackPattern { SingleShot, SpreadShot, Beam }

    [Header("配置・進路ブロック")]
    public bool blocksPath = true;   // trueの間、背景スクロールを止める演出などに利用可能
    public float entrySpeed = 2f;
    public Vector3 stopPosition;     // ここまで進んで停止する

    [Header("攻撃設定")]
    public GameObject bulletPrefab;
    public GameObject beamPrefab;
    public float patternInterval = 3f;
    public int spreadCount = 5;
    public float spreadAngle = 60f;

    [Header("弱点演出")]
    public int phaseThreshold; // このHPを下回るとパターンが激化する（0なら未使用）

    private bool hasStopped = false;
    private float patternTimer;
    private Transform playerTarget;

    protected override void Awake()
    {
        base.Awake();
        patternTimer = patternInterval;
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) playerTarget = playerObj.transform;
    }

    private void Update()
    {
        if (!hasStopped)
        {
            EnterScene();
        }
        else
        {
            patternTimer -= Time.deltaTime;
            if (patternTimer <= 0f)
            {
                StartCoroutine(ExecuteRandomPattern());
                patternTimer = patternInterval;
            }
        }
    }

    private void EnterScene()
    {
        transform.position = Vector3.MoveTowards(transform.position, stopPosition, entrySpeed * Time.deltaTime);
        if (Vector3.Distance(transform.position, stopPosition) < 0.05f)
        {
            hasStopped = true;
            if (blocksPath)
            {
                GameManager.Instance?.OnBossEncountered();
            }
        }
    }

    private IEnumerator ExecuteRandomPattern()
    {
        AttackPattern pattern = (AttackPattern)Random.Range(0, 3);

        switch (pattern)
        {
            case AttackPattern.SingleShot:
                FireSingleShot();
                break;
            case AttackPattern.SpreadShot:
                FireSpreadShot();
                break;
            case AttackPattern.Beam:
                FireBeam();
                break;
        }

        yield return null;
    }

    private void FireSingleShot()
    {
        if (bulletPrefab == null || playerTarget == null) return;
        Vector2 dir = (playerTarget.position - transform.position).normalized;
        GameObject b = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
        EnemyBullet eb = b.GetComponent<EnemyBullet>();
        if (eb != null) eb.direction = dir;
    }

    private void FireSpreadShot()
    {
        if (bulletPrefab == null) return;

        float startAngle = -spreadAngle / 2f;
        float step = spreadCount > 1 ? spreadAngle / (spreadCount - 1) : 0f;

        for (int i = 0; i < spreadCount; i++)
        {
            float angle = startAngle + step * i;
            Vector2 dir = Quaternion.Euler(0, 0, angle) * Vector2.left;
            GameObject b = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
            EnemyBullet eb = b.GetComponent<EnemyBullet>();
            if (eb != null) eb.direction = dir;
        }
    }

    private void FireBeam()
    {
        if (beamPrefab == null) return;
        Instantiate(beamPrefab, transform.position, transform.rotation);
    }

    protected override void Die()
    {
        if (blocksPath)
        {
            GameManager.Instance?.OnBossDefeated();
        }
        base.Die();
    }
}
