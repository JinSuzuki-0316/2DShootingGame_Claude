using System.Text;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 画面上部にスコアと残機、下部に強化アイテムのメーター（選択中カーソル付き・色分け）と
/// 現在の武器状態を表示する。ゲームオーバー時は点滅するリトライ案内を表示する。
/// </summary>
public class GameHUD : MonoBehaviour
{
    public Text scoreText;
    public Text livesText;
    public Text powerMeterText;
    public Text powerStatusText;
    public Text gameOverText;

    private static readonly string[] Labels = { "SPEED", "MISSILE", "DOUBLE", "LASER", "OPTION", "BARRIER" };

    private const string ColorSelected = "#FFEB3B"; // 選択中：イエロー
    private const string ColorIdle = "#7A88A8";      // 未選択：くすんだブルーグレー
    private const string ColorCapsule = "#4FC3F7";   // カプセル数：スカイブルー
    private const string ColorOn = "#66FF99";        // 状態ON：グリーン
    private const string ColorOff = "#556070";       // 状態OFF：グレー
    private const string ColorWeaponLaser = "#FF4D6D";
    private const string ColorWeaponDouble = "#FFA94D";
    private const string ColorWeaponNormal = "#E0E0E0";

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

            if (gm.isGameOver)
            {
                // Time.timeScale=0でも進むunscaledTimeで明滅させる
                float pulse = 0.55f + 0.45f * ((Mathf.Sin(Time.unscaledTime * 3.2f) + 1f) / 2f);
                Color c = gameOverText.color;
                c.a = pulse;
                gameOverText.color = c;
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

        int lives = Mathf.Max(player.lives, 0);
        livesText.text = "LIVES " + lives;
        livesText.color = lives <= 1 ? new Color(1f, 0.4f, 0.35f) : new Color(0.6f, 1f, 0.6f);

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

            if (i == pum.selectedIndex)
            {
                sb.Append("<color=").Append(ColorSelected).Append("><b>[ ").Append(label).Append(" ]</b></color>");
            }
            else
            {
                sb.Append("<color=").Append(ColorIdle).Append(">").Append(label).Append("</color>");
            }
            sb.Append("   ");
        }
        sb.Append("<color=").Append(ColorCapsule).Append(">CAPSULE x").Append(pum.stockedCapsules).Append("</color>");
        return sb.ToString();
    }

    private string BuildStatusString(PlayerController player)
    {
        string weaponColor = player.hasLaser ? ColorWeaponLaser : (player.hasDouble ? ColorWeaponDouble : ColorWeaponNormal);
        string weaponName = player.hasLaser ? "LASER" : (player.hasDouble ? "DOUBLE" : "NORMAL");

        string missileColor = player.hasMissile ? ColorOn : ColorOff;
        string missileState = player.hasMissile ? "ON" : "OFF";

        string barrierColor = player.currentBarrier != null ? ColorOn : ColorOff;
        string barrierState = player.currentBarrier != null ? "ON" : "OFF";

        int speedLv = player.speedLevelIndex + 1;
        int optionCount = player.OptionCount;

        return string.Format(
            "SPEED Lv{0}   WEAPON:<color={1}>{2}</color>   MISSILE:<color={3}>{4}</color>   OPTIONS:{5}   BARRIER:<color={6}>{7}</color>",
            speedLv, weaponColor, weaponName, missileColor, missileState, optionCount, barrierColor, barrierState);
    }
}
