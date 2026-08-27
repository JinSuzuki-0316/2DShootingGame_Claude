using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// グラディウス風のパワーアップメーター管理。
/// カプセルを取得するたびに選択カーソルが1つ進み、
/// パワーアップボタンを押すとカーソル位置の効果を発動してカプセルを1つ消費する。
/// </summary>
public class PowerUpManager : MonoBehaviour
{
    public enum PowerUpType
    {
        Speed,
        Missile,
        Double,
        Laser,
        Option,
        Barrier
    }

    [Header("メーターの並び順（左から右）")]
    public PowerUpType[] meterOrder = new PowerUpType[]
    {
        PowerUpType.Speed,
        PowerUpType.Missile,
        PowerUpType.Double,
        PowerUpType.Laser,
        PowerUpType.Option,
        PowerUpType.Barrier
    };

    [Header("現在の状態")]
    public int stockedCapsules = 0;   // 未使用のカプセル数
    public int selectedIndex = 0;     // メーター上のカーソル位置
    private int totalCollected = 0;   // 累計カプセル取得数（カーソル計算用）

    [Header("参照")]
    public PlayerController player;

    public delegate void PowerUpActivated(PowerUpType type);
    public event PowerUpActivated OnPowerUpActivated;

    private void Awake()
    {
        if (player == null) player = GetComponent<PlayerController>();
    }

    /// <summary>アイテムカプセルを取得したときに呼ぶ</summary>
    public void CollectCapsule()
    {
        stockedCapsules++;
        totalCollected++;
        // 1個目のカプセルで必ず先頭（SPEED）を指すように、取得数ベースで計算する。
        // （以前は取得のたびに単純加算していたため1つ先の項目を指してしまい、
        // 　最後の項目「BARRIER」にちょうど止めにくいバグがあった）
        selectedIndex = (totalCollected - 1) % meterOrder.Length;
    }

    /// <summary>パワーアップボタン入力を毎フレームチェック（Shiftキー／ジョイスティックボタン1）</summary>
    private void Update()
    {
        bool pressed = Input.GetKeyDown(KeyCode.LeftShift)
                       || Input.GetKeyDown(KeyCode.RightShift)
                       || Input.GetKeyDown(KeyCode.JoystickButton1);

        if (pressed)
        {
            ActivateSelected();
        }
    }

    /// <summary>現在選択中の効果を発動する</summary>
    public void ActivateSelected()
    {
        if (stockedCapsules <= 0) return;

        stockedCapsules--;
        PowerUpType type = meterOrder[selectedIndex];
        Apply(type);
        AudioManager.Instance?.PlayPowerUp();
        OnPowerUpActivated?.Invoke(type);
    }

    private void Apply(PowerUpType type)
    {
        switch (type)
        {
            case PowerUpType.Speed:
                player.IncreaseSpeedLevel();
                break;
            case PowerUpType.Missile:
                player.hasMissile = true;
                break;
            case PowerUpType.Double:
                player.hasDouble = true;
                player.hasLaser = false; // ダブルとレーザーは併用不可
                break;
            case PowerUpType.Laser:
                player.hasLaser = true;
                player.hasDouble = false; // レーザーとダブルは併用不可
                break;
            case PowerUpType.Option:
                player.AddOption();
                break;
            case PowerUpType.Barrier:
                player.ActivateBarrier();
                break;
        }
    }

    /// <summary>被弾してミスした際、パワーアップを全てリセットする（グラディウス方式）</summary>
    public void ResetAllPowerUps()
    {
        stockedCapsules = 0;
        selectedIndex = 0;
        totalCollected = 0;
    }
}
