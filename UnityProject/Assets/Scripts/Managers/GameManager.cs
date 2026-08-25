using UnityEngine;

/// <summary>
/// スコア・ゲームオーバー・リトライ・ボス戦演出などを一元管理するシングルトン。
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public int score = 0;
    public bool isGameOver = false;

    [Header("参照")]
    public ScrollingBackground scroller;
    public GameRoot gameRoot;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Update()
    {
        // ゲームオーバー中にRキーでリトライ（Time.timeScale=0でもInputは動作する）
        if (isGameOver && Input.GetKeyDown(KeyCode.R))
        {
            Retry();
        }
    }

    public void AddScore(int amount)
    {
        score += amount;
    }

    public void GameOver()
    {
        isGameOver = true;
        Time.timeScale = 0f;
    }

    /// <summary>ゲームを初期状態からやり直す</summary>
    public void Retry()
    {
        if (gameRoot == null) gameRoot = FindObjectOfType<GameRoot>();
        if (gameRoot == null) return;

        Time.timeScale = 1f;
        isGameOver = false;
        score = 0;
        gameRoot.RestartGame();
    }

    /// <summary>ボス出現時にスクロールを止める</summary>
    public void OnBossEncountered()
    {
        if (scroller != null) scroller.isScrolling = false;
    }

    /// <summary>ボス撃破後にスクロール再開</summary>
    public void OnBossDefeated()
    {
        if (scroller != null) scroller.isScrolling = true;
    }
}

