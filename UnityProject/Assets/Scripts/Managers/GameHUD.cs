using System.Text;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 画面上部にスコアと残機、下部に強化アイテムのメーター（選択中カーソル付き）と
/// 現在の武器状態を表示する。ゲームオーバー時はリトライ案内を表示する。
/// </summary>
public class GameHUD : MonoBehaviour
{
    public Text scoreText;
    public Text livesText;
    public Text powerMeterText;
    public Text powerStatusText;
    public Text gameOverText;

    private static readonly string[] Labels = { "SPEED", "MISSILE", "DOUBLE", "LASER", "OPTION", "BARRIER" };

    private void Update()
    {
        GameManager gm = GameManager.Instance;
        if (gm != null)
        {
            scoreText.text = "SCORE " + gm.score.ToString("D6");
            if (gameOverText.gameObject.activeSelf != gm.isGameOver)
            {
                gameOverText.gameObject.SetActive(gm.isGameOver);
            }
        }

        PlayerController player = FindObjectOfType<PlayerController>();
        if (player == null)
        {
            livesText.text = "LIVES --";
            powerMeterText.text = string.Empty;
            powerStatusText.text = string.Empty;
            return;
        }

        livesText.text = "LIVES " + Mathf.Max(player.lives, 0);

        PowerUpManager pum = player.GetComponent<PowerUpManager>();
        if (pum != null)
        {
            powerMeterText.text = BuildMeterString(pum);
        }
        powerStatusText.text = BuildStatusString(player);
    }

    private string BuildMeterString(PowerUpManager pum)
    {
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < pum.meterOrder.Length; i++)
        {
            string label = i < Labels.Length ? Labels[i] : pum.meterOrder[i].ToString().ToUpper();
            sb.Append(i == pum.selectedIndex ? "[" + label + "]" : " " + label + " ");
            sb.Append(" ");
        }
        sb.Append("   CAPSULE x").Append(pum.stockedCapsules);
        return sb.ToString();
    }

    private string BuildStatusString(PlayerController player)
    {
        string weapon = player.hasLaser ? "LASER" : (player.hasDouble ? "DOUBLE" : "NORMAL");
        string missile = player.hasMissile ? "ON" : "OFF";
        string barrier = player.currentBarrier != null ? "ON" : "OFF";
        int speedLv = player.speedLevelIndex + 1;
        int optionCount = player.OptionCount;

        return string.Format(
            "SPEED Lv{0}   WEAPON:{1}   MISSILE:{2}   OPTIONS:{3}   BARRIER:{4}",
            speedLv, weapon, missile, optionCount, barrier);
    }
}
