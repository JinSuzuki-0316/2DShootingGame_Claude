using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PowerUpManager))]
public class PlayerController : MonoBehaviour
{
    [Header("移動")]
    public float[] speedLevels = new float[] { 3f, 4.5f, 6f, 7.5f, 9f }; // 最大5段階
    public int speedLevelIndex = 0;
    public Rigidbody2D rb;
    public Vector2 screenMin = new Vector2(-8.5f, -4.5f);
    public Vector2 screenMax = new Vector2(8.5f, 4.5f);

    [Header("武器プレハブ")]
    public GameObject normalBulletPrefab;
    public GameObject doubleBulletPrefab; // 斜め上用
    public GameObject laserPrefab;
    public GameObject missilePrefab;
    public GameObject optionPrefab;
    public GameObject barrierPrefab;

    [Header("発射位置")]
    public Transform muzzleFront;
    public Transform muzzleDiagonal; // ダブル用の斜め上
    public Transform muzzleBottom;   // ミサイル用

    [Header("武器状態（併用不可の組はPowerUpManagerが管理）")]
    public bool hasDouble = false;
    public bool hasLaser = false;
    public bool hasMissile = false;

    [Header("発射レート")]
    public float fireCooldown = 0.2f;
    public float missileCooldown = 0.5f;
    private float fireTimer = 0f;
    private float missileTimer = 0f;

    [Header("オプション")]
    public int maxOptions = 4;
    private List<OptionFollower> options = new List<OptionFollower>();

    [Header("バリア")]
    public GameObject currentBarrier;

    [Header("ライフ")]
    public int lives = 3;
    public bool isInvincible = false;
    public float respawnInvincibleTime = 2f;

    // オプション追従用に自分の移動履歴を保持
    [HideInInspector] public List<Vector3> positionHistory = new List<Vector3>();
    public int historyMaxLength = 300;

    private PowerUpManager powerUpManager;

    private void Awake()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        powerUpManager = GetComponent<PowerUpManager>();
    }

    private void Update()
    {
        HandleMovement();
        HandleShooting();
        RecordHistory();
    }

    private void HandleMovement()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector2 dir = new Vector2(h, v).normalized;
        float speed = speedLevels[Mathf.Clamp(speedLevelIndex, 0, speedLevels.Length - 1)];

        Vector3 pos = transform.position + (Vector3)(dir * speed * Time.deltaTime);
        pos.x = Mathf.Clamp(pos.x, screenMin.x, screenMax.x);
        pos.y = Mathf.Clamp(pos.y, screenMin.y, screenMax.y);
        transform.position = pos;
    }

    private void HandleShooting()
    {
        fireTimer -= Time.deltaTime;
        missileTimer -= Time.deltaTime;

        bool firing = Input.GetButton("Fire1");

        if (firing && fireTimer <= 0f)
        {
            FireMainWeapon();
            fireTimer = fireCooldown;
        }

        if (firing && hasMissile && missileTimer <= 0f)
        {
            FireMissile();
            missileTimer = missileCooldown;
        }
    }

    /// <summary>メイン武器を発射する。レーザー＞ダブル＞ノーマルの優先順位（併用不可のため）</summary>
    private void FireMainWeapon()
    {
        if (hasLaser)
        {
            Instantiate(laserPrefab, muzzleFront.position, muzzleFront.rotation);
        }
        else if (hasDouble)
        {
            Instantiate(normalBulletPrefab, muzzleFront.position, muzzleFront.rotation);
            Instantiate(doubleBulletPrefab, muzzleDiagonal.position, muzzleDiagonal.rotation);
        }
        else
        {
            Instantiate(normalBulletPrefab, muzzleFront.position, muzzleFront.rotation);
        }

        // オプションも同じ武器で追従射撃
        foreach (var opt in options)
        {
            opt.Fire(hasLaser, hasDouble, normalBulletPrefab, doubleBulletPrefab, laserPrefab);
        }
    }

    private void FireMissile()
    {
        if (missilePrefab == null || muzzleBottom == null) return;
        Instantiate(missilePrefab, muzzleBottom.position, Quaternion.identity);
    }

    public void IncreaseSpeedLevel()
    {
        // ミスするまで減速不可。最大段階まで上げるのみ。
        speedLevelIndex = Mathf.Min(speedLevelIndex + 1, speedLevels.Length - 1);
    }

    public void AddOption()
    {
        if (options.Count >= maxOptions) return;
        GameObject go = Instantiate(optionPrefab);
        OptionFollower follower = go.GetComponent<OptionFollower>();
        // 先頭オプションはプレイヤーを追従、以降は1つ前のオプションを追従してもよいが
        // ここでは全て「プレイヤーの過去位置」を参照するシンプルな実装にする
        int delayIndex = (options.Count + 1) * 20; // フレーム遅延
        follower.Init(this, delayIndex);
        options.Add(follower);
    }

    public void ActivateBarrier()
    {
        if (currentBarrier != null) return; // 既に展開中なら何もしない
        currentBarrier = Instantiate(barrierPrefab, transform.position, Quaternion.identity, transform);
    }

    private void RecordHistory()
    {
        positionHistory.Insert(0, transform.position);
        if (positionHistory.Count > historyMaxLength)
        {
            positionHistory.RemoveAt(positionHistory.Count - 1);
        }
    }

    /// <summary>敵弾・敵機との接触処理</summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isInvincible) return;

        if (other.CompareTag("EnemyBullet") || other.CompareTag("Enemy"))
        {
            if (currentBarrier != null)
            {
                Barrier barrier = currentBarrier.GetComponent<Barrier>();
                barrier.TakeHit();
                Destroy(other.gameObject.CompareTag("EnemyBullet") ? other.gameObject : null);
                return;
            }

            Die();
            Destroy(other.gameObject);
        }
    }

    private void Die()
    {
        lives--;

        // グラディウス方式：被弾で全パワーアップを喪失
        hasDouble = false;
        hasLaser = false;
        hasMissile = false;
        speedLevelIndex = 0;
        powerUpManager.ResetAllPowerUps();

        foreach (var opt in options)
        {
            if (opt != null) Destroy(opt.gameObject);
        }
        options.Clear();

        if (currentBarrier != null)
        {
            Destroy(currentBarrier);
            currentBarrier = null;
        }

        if (lives > 0)
        {
            StartCoroutine(RespawnRoutine());
        }
        else
        {
            GameManager.Instance?.GameOver();
            gameObject.SetActive(false);
        }
    }

    private IEnumerator RespawnRoutine()
    {
        isInvincible = true;
        transform.position = new Vector3(-6f, 0f, 0f); // 復帰地点
        yield return new WaitForSeconds(respawnInvincibleTime);
        isInvincible = false;
    }
}
