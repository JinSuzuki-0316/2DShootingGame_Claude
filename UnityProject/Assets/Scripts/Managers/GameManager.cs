using UnityEngine;

/// <summary>
/// スコア・ゲームオーバー・ボス戦演出などを一元管理するシングルトン。
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public int score = 0;
    public bool isGameOver = false;

    [Header("ボス戦演出用")]
    public ScrollingBackground scroller;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void AddScore(int amount)
    {
        score += amount;
    }

    public void GameOver()
    {
        isGameOver = true;
        Time.timeScale = 0f;
        // ここにゲームオーバーUI表示処理などを追加
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
