using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// シーンが空でも、Playを押した瞬間に自動でゲーム全体
/// （カメラ・自機・地面・背景・敵スポナー・各種テンプレート）を構築する。
/// Inspectorでの手動配線やPrefabアセットの用意は一切不要。
/// </summary>
public class GameRoot : MonoBehaviour
{
    private GameObject templatesContainer;

    // テンプレート（実体化のもとになる非表示オブジェクト）
    private GameObject bulletTemplate;
    private GameObject bulletDiagonalTemplate;
    private GameObject laserTemplate;
    private GameObject missileTemplate;
    private GameObject optionTemplate;
    private GameObject barrierTemplate;
    private GameObject enemyBulletTemplate;
    private GameObject capsuleTemplate;
    private GameObject smallSwarmTemplate;
    private GameObject turretTemplate;
    private GameObject hatchTemplate;
    private GameObject walkerTemplate;
    private GameObject bossTemplate;

    private void Awake()
    {
        BuildCamera();
        BuildTemplatesContainer();
        BuildWeaponTemplates();
        BuildEnemyTemplates();
        GameObject player = BuildPlayer();
        BuildGround();
        ScrollingBackground scroller = BuildBackground();
        BuildSpawner();
        BuildGameManager(scroller);
        BuildUI();
    }

    // ----------------------------------------------------------------
    // リトライ（プレイヤー・地面・背景・敵スポナー・敵/弾を全て作り直す）
    // ----------------------------------------------------------------
    public void RestartGame()
    {
        DestroyAllGameplayObjects();

        BuildPlayer();
        BuildGround();
        ScrollingBackground newScroller = BuildBackground();
        BuildSpawner();

        GameManager gm = GameManager.Instance;
        if (gm != null) gm.scroller = newScroller;
    }

    private void DestroyAllGameplayObjects()
    {
        foreach (var e in FindObjectsOfType<EnemyBase>()) Destroy(e.gameObject);
        foreach (var b in FindObjectsOfType<PlayerBullet>()) Destroy(b.gameObject);
        foreach (var l in FindObjectsOfType<PlayerLaser>()) Destroy(l.gameObject);
        foreach (var m in FindObjectsOfType<CrawlingMissile>()) Destroy(m.gameObject);
        foreach (var eb in FindObjectsOfType<EnemyBullet>()) Destroy(eb.gameObject);
        foreach (var c in FindObjectsOfType<PowerCapsule>()) Destroy(c.gameObject);
        foreach (var o in FindObjectsOfType<OptionFollower>()) Destroy(o.gameObject);

        PlayerController existingPlayer = FindObjectOfType<PlayerController>();
        if (existingPlayer != null) Destroy(existingPlayer.gameObject);

        EnemySpawner existingSpawner = FindObjectOfType<EnemySpawner>();
        if (existingSpawner != null) Destroy(existingSpawner.gameObject);

        ScrollingBackground existingBg = FindObjectOfType<ScrollingBackground>();
        if (existingBg != null) Destroy(existingBg.gameObject);

        GroundTag existingGround = FindObjectOfType<GroundTag>();
        if (existingGround != null) Destroy(existingGround.gameObject);
    }

    // ----------------------------------------------------------------
    // カメラ
    // ----------------------------------------------------------------
    private void BuildCamera()
    {
        if (Camera.main != null) return;

        GameObject camObj = new GameObject("Main Camera");
        camObj.tag = "MainCamera";
        Camera cam = camObj.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 5f;
        cam.backgroundColor = new Color(0.02f, 0.02f, 0.08f);
        cam.transform.position = new Vector3(0, 0, -10);
        camObj.AddComponent<AudioListener>();
    }

    // ----------------------------------------------------------------
    // テンプレート格納用の非アクティブな親
    // ----------------------------------------------------------------
    private void BuildTemplatesContainer()
    {
        templatesContainer = new GameObject("_Templates");
        // 親を非アクティブにすることで、この中の子は動作しない。
        // Instantiate()でルートとして複製されると自動的に有効化される。
        templatesContainer.SetActive(false);
    }

    private GameObject NewTemplate(string name)
    {
        GameObject go = new GameObject(name);
        go.transform.parent = templatesContainer.transform;
        return go;
    }

    private void AddTrigger(GameObject go, float radius)
    {
        Rigidbody2D rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        CircleCollider2D col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = radius;
    }

    // ----------------------------------------------------------------
    // プレイヤーの武器テンプレート
    // ----------------------------------------------------------------
    private void BuildWeaponTemplates()
    {
        // 通常弾
        bulletTemplate = NewTemplate("Bullet");
        bulletTemplate.AddComponent<SpriteRenderer>().sprite = SpriteFactory.CreateSquare(Color.yellow);
        AddTrigger(bulletTemplate, 0.1f);
        PlayerBullet pb = bulletTemplate.AddComponent<PlayerBullet>();
        pb.direction = Vector2.right;

        // ダブル用の斜め弾
        bulletDiagonalTemplate = NewTemplate("BulletDiagonal");
        bulletDiagonalTemplate.AddComponent<SpriteRenderer>().sprite = SpriteFactory.CreateSquare(Color.yellow);
        AddTrigger(bulletDiagonalTemplate, 0.1f);
        PlayerBullet pbd = bulletDiagonalTemplate.AddComponent<PlayerBullet>();
        pbd.direction = new Vector2(0.7f, 0.7f);

        // レーザー
        laserTemplate = NewTemplate("Laser");
        SpriteRenderer laserSr = laserTemplate.AddComponent<SpriteRenderer>();
        laserSr.sprite = SpriteFactory.CreateBar(new Color(1f, 0.2f, 0.2f), 48, 6);
        AddTrigger(laserTemplate, 0.15f);
        laserTemplate.AddComponent<PlayerLaser>();

        // ミサイル
        missileTemplate = NewTemplate("Missile");
        missileTemplate.AddComponent<SpriteRenderer>().sprite = SpriteFactory.CreateSquare(new Color(1f, 0.5f, 0f));
        AddTrigger(missileTemplate, 0.15f);
        missileTemplate.AddComponent<CrawlingMissile>();

        // オプション（分身）
        optionTemplate = NewTemplate("Option");
        optionTemplate.AddComponent<SpriteRenderer>().sprite = SpriteFactory.CreateCircle(new Color(0.3f, 1f, 1f));
        OptionFollower optFollower = optionTemplate.AddComponent<OptionFollower>();

        GameObject optMuzzleFront = new GameObject("MuzzleFront");
        optMuzzleFront.transform.parent = optionTemplate.transform;
        optMuzzleFront.transform.localPosition = new Vector3(0.4f, 0.15f, 0);
        optFollower.muzzleFront = optMuzzleFront.transform;

        GameObject optMuzzleDiag = new GameObject("MuzzleDiagonal");
        optMuzzleDiag.transform.parent = optionTemplate.transform;
        optMuzzleDiag.transform.localPosition = new Vector3(0.25f, 0.25f, 0);
        optFollower.muzzleDiagonal = optMuzzleDiag.transform;

        // バリア
        barrierTemplate = NewTemplate("Barrier");
        SpriteRenderer barrierSr = barrierTemplate.AddComponent<SpriteRenderer>();
        barrierSr.sprite = SpriteFactory.CreateCircle(new Color(0.4f, 0.7f, 1f, 0.5f));
        barrierTemplate.transform.localScale = Vector3.one * 1.8f;
        barrierTemplate.AddComponent<Barrier>();

        // 敵弾
        enemyBulletTemplate = NewTemplate("EnemyBullet");
        enemyBulletTemplate.AddComponent<SpriteRenderer>().sprite = SpriteFactory.CreateCircle(Color.magenta);
        AddTrigger(enemyBulletTemplate, 0.12f);
        enemyBulletTemplate.AddComponent<EnemyBullet>();

        // パワーアップカプセル
        capsuleTemplate = NewTemplate("PowerCapsule");
        capsuleTemplate.AddComponent<SpriteRenderer>().sprite = SpriteFactory.CreateSquare(Color.green);
        AddTrigger(capsuleTemplate, 0.2f);
        capsuleTemplate.AddComponent<PowerCapsule>();
    }

    // ----------------------------------------------------------------
    // 敵テンプレート
    // ----------------------------------------------------------------
    private void BuildEnemyTemplates()
    {
        // 敵種1：小型機（群れ）
        smallSwarmTemplate = NewTemplate("SmallSwarmEnemy");
        smallSwarmTemplate.AddComponent<SpriteRenderer>().sprite = SpriteFactory.CreateSquare(new Color(1f, 0.3f, 0.3f));
        AddTrigger(smallSwarmTemplate, 0.3f);
        SmallSwarmEnemy swarm = smallSwarmTemplate.AddComponent<SmallSwarmEnemy>();
        swarm.maxHealth = 1;
        swarm.scoreValue = 100;
        swarm.bulletPrefab = enemyBulletTemplate;

        // 敵種2：砲台・ローパー
        turretTemplate = NewTemplate("TurretEnemy");
        turretTemplate.AddComponent<SpriteRenderer>().sprite = SpriteFactory.CreateSquare(new Color(0.6f, 0.6f, 0.6f));
        AddTrigger(turretTemplate, 0.4f);
        TurretEnemy turret = turretTemplate.AddComponent<TurretEnemy>();
        turret.maxHealth = 3;
        turret.scoreValue = 300;
        turret.bulletPrefab = enemyBulletTemplate;

        // 敵種3：ハッチ（クラブ）
        hatchTemplate = NewTemplate("HatchEnemy");
        hatchTemplate.transform.localScale = Vector3.one * 1.4f;
        hatchTemplate.AddComponent<SpriteRenderer>().sprite = SpriteFactory.CreateCircle(new Color(0.7f, 0.2f, 0.8f));
        AddTrigger(hatchTemplate, 0.45f);
        HatchEnemy hatch = hatchTemplate.AddComponent<HatchEnemy>();
        hatch.maxHealth = 5;
        hatch.scoreValue = 500;
        hatch.spawnedEnemyPrefab = smallSwarmTemplate;
        hatch.isRedVariant = true; // 赤色＝撃破でカプセルドロップ
        hatch.powerCapsulePrefab = capsuleTemplate;

        // 敵種4：歩行・移動型（ダッカー）
        walkerTemplate = NewTemplate("WalkerEnemy");
        walkerTemplate.AddComponent<SpriteRenderer>().sprite = SpriteFactory.CreateSquare(new Color(0.5f, 0.35f, 0.2f));
        AddTrigger(walkerTemplate, 0.3f);
        WalkerEnemy walker = walkerTemplate.AddComponent<WalkerEnemy>();
        walker.maxHealth = 2;
        walker.scoreValue = 200;
        walker.bulletPrefab = enemyBulletTemplate;

        // 敵種5：大型・中ボス（ゴーレム／ビッグアイ）
        bossTemplate = NewTemplate("BossEnemy");
        bossTemplate.transform.localScale = Vector3.one * 2.5f;
        bossTemplate.AddComponent<SpriteRenderer>().sprite = SpriteFactory.CreateCircle(new Color(0.8f, 0.1f, 0.1f));
        AddTrigger(bossTemplate, 0.9f);
        BossEnemy boss = bossTemplate.AddComponent<BossEnemy>();
        boss.maxHealth = 40;
        boss.scoreValue = 5000;
        boss.bulletPrefab = enemyBulletTemplate;
        boss.stopPosition = new Vector3(4f, 0f, 0f);
    }

    // ----------------------------------------------------------------
    // プレイヤー
    // ----------------------------------------------------------------
    private GameObject BuildPlayer()
    {
        GameObject player = new GameObject("Player");
        player.transform.position = new Vector3(-6f, 0f, 0f);

        Rigidbody2D rb = player.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;

        CircleCollider2D col = player.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.35f;

        SpriteRenderer sr = player.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteFactory.CreateTriangle(new Color(0.2f, 1f, 0.4f));
        sr.sortingOrder = 10;

        PowerUpManager pum = player.AddComponent<PowerUpManager>();
        PlayerController pc = player.AddComponent<PlayerController>();
        pc.rb = rb;
        // PowerUpManagerのAwakeはPlayerController追加前に実行されているため、
        // ここで明示的に参照を結び直す（nullのままになるのを防ぐ）
        pum.player = pc;

        pc.normalBulletPrefab = bulletTemplate;
        pc.doubleBulletPrefab = bulletDiagonalTemplate;
        pc.laserPrefab = laserTemplate;
        pc.missilePrefab = missileTemplate;
        pc.optionPrefab = optionTemplate;
        pc.barrierPrefab = barrierTemplate;

        GameObject muzzleFront = new GameObject("MuzzleFront");
        muzzleFront.transform.parent = player.transform;
        muzzleFront.transform.localPosition = new Vector3(0.5f, 0.15f, 0);
        pc.muzzleFront = muzzleFront.transform;

        GameObject muzzleDiag = new GameObject("MuzzleDiagonal");
        muzzleDiag.transform.parent = player.transform;
        muzzleDiag.transform.localPosition = new Vector3(0.35f, 0.3f, 0);
        pc.muzzleDiagonal = muzzleDiag.transform;

        GameObject muzzleBottom = new GameObject("MuzzleBottom");
        muzzleBottom.transform.parent = player.transform;
        muzzleBottom.transform.localPosition = new Vector3(0f, -0.3f, 0);
        pc.muzzleBottom = muzzleBottom.transform;

        return player;
    }

    // ----------------------------------------------------------------
    // 地面（ミサイルの這い移動・歩行敵の足場用）
    // ----------------------------------------------------------------
    private void BuildGround()
    {
        GameObject ground = new GameObject("Ground");
        ground.transform.position = new Vector3(0f, -4.2f, 0f);

        BoxCollider2D box = ground.AddComponent<BoxCollider2D>();
        box.size = new Vector2(40f, 1f);

        SpriteRenderer sr = ground.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteFactory.CreateBar(new Color(0.25f, 0.2f, 0.15f), 640, 16);
        sr.sortingOrder = -1;

        ground.AddComponent<GroundTag>();
    }

    // ----------------------------------------------------------------
    // 横スクロール背景
    // ----------------------------------------------------------------
    private ScrollingBackground BuildBackground()
    {
        GameObject bgRoot = new GameObject("ScrollingBackground");
        ScrollingBackground scroller = bgRoot.AddComponent<ScrollingBackground>();
        scroller.scrollSpeed = 1.5f;
        scroller.tileWidth = 20f;

        Transform[] tiles = new Transform[3];
        for (int i = 0; i < 3; i++)
        {
            GameObject tile = new GameObject("BGTile" + i);
            tile.transform.parent = bgRoot.transform;
            tile.transform.position = new Vector3(-20f + i * 20f, 0f, 5f);
            SpriteRenderer sr = tile.AddComponent<SpriteRenderer>();
            sr.sprite = SpriteFactory.CreateBar(new Color(0.05f, 0.05f, 0.15f), 320, 160);
            sr.sortingOrder = -10;
            tiles[i] = tile.transform;
        }
        scroller.tiles = tiles;

        return scroller;
    }

    // ----------------------------------------------------------------
    // 敵の出現スケジュール
    // ----------------------------------------------------------------
    private void BuildSpawner()
    {
        GameObject spawnerObj = new GameObject("EnemySpawner");
        EnemySpawner spawner = spawnerObj.AddComponent<EnemySpawner>();

        spawner.spawnEntries = new EnemySpawner.SpawnEntry[]
        {
            Entry(smallSwarmTemplate, new Vector3(9f, 3f, 0f), 1.0f),
            Entry(smallSwarmTemplate, new Vector3(9f, 3.6f, 0f), 1.3f),
            Entry(smallSwarmTemplate, new Vector3(9f, 4.2f, 0f), 1.6f),

            Entry(turretTemplate, new Vector3(6f, -3.4f, 0f), 4f),

            Entry(hatchTemplate, new Vector3(6f, 3.2f, 0f), 8f),

            Entry(walkerTemplate, new Vector3(8f, -3.6f, 0f), 10f),

            Entry(smallSwarmTemplate, new Vector3(9f, -2f, 0f), 14f),
            Entry(smallSwarmTemplate, new Vector3(9f, -1.4f, 0f), 14.3f),
            Entry(smallSwarmTemplate, new Vector3(9f, -0.8f, 0f), 14.6f),

            Entry(bossTemplate, new Vector3(10f, 0f, 0f), 22f),
        };
    }

    private EnemySpawner.SpawnEntry Entry(GameObject prefab, Vector3 pos, float delay)
    {
        return new EnemySpawner.SpawnEntry { prefab = prefab, position = pos, delay = delay };
    }

    // ----------------------------------------------------------------
    // ゲームマネージャー
    // ----------------------------------------------------------------
    private void BuildGameManager(ScrollingBackground scroller)
    {
        GameObject gmObj = new GameObject("GameManager");
        GameManager gm = gmObj.AddComponent<GameManager>();
        gm.scroller = scroller;
        gm.gameRoot = this;
    }

    // ----------------------------------------------------------------
    // HUD（スコア／残機／強化メーター／ゲームオーバー表示）
    // ----------------------------------------------------------------
    private void BuildUI()
    {
        // 既にHUDが存在するなら作り直さない（リトライ時はGameManager/HUDを再利用するため）
        if (FindObjectOfType<GameHUD>() != null) return;

        GameObject canvasObj = new GameObject("HUDCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280, 720);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObj.AddComponent<GraphicRaycaster>();

        if (FindObjectOfType<EventSystem>() == null)
        {
            GameObject esObj = new GameObject("EventSystem");
            esObj.AddComponent<EventSystem>();
            esObj.AddComponent<StandaloneInputModule>();
        }

        GameHUD hud = canvasObj.AddComponent<GameHUD>();

        // 左上：スコア／残機
        hud.scoreText = CreateText(canvasObj.transform, "ScoreText",
            new Vector2(0, 1), new Vector2(0, 1), new Vector2(160, -30),
            new Vector2(300, 40), 28, TextAnchor.UpperLeft);

        hud.livesText = CreateText(canvasObj.transform, "LivesText",
            new Vector2(0, 1), new Vector2(0, 1), new Vector2(160, -65),
            new Vector2(300, 40), 22, TextAnchor.UpperLeft);

        // 下部：強化メーター／武器状態
        hud.powerMeterText = CreateText(canvasObj.transform, "PowerMeterText",
            new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 45),
            new Vector2(-40, 30), 20, TextAnchor.MiddleLeft);

        hud.powerStatusText = CreateText(canvasObj.transform, "PowerStatusText",
            new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 15),
            new Vector2(-40, 30), 18, TextAnchor.MiddleLeft);

        // 中央：ゲームオーバー／リトライ案内
        hud.gameOverText = CreateText(canvasObj.transform, "GameOverText",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero,
            new Vector2(600, 150), 40, TextAnchor.MiddleCenter);
        hud.gameOverText.text = "GAME OVER\nPress R to Retry";
        hud.gameOverText.color = Color.red;
        hud.gameOverText.gameObject.SetActive(false);
    }

    private Text CreateText(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax,
        Vector2 anchoredPos, Vector2 size, int fontSize, TextAnchor alignment)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);

        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = anchorMin;
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = size;

        Text text = go.AddComponent<Text>();
        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (font == null) font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.font = font;
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = Color.white;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.text = string.Empty;
        return text;
    }
}
