using UnityEngine;

/// <summary>
/// プレイヤーの過去の移動履歴を辿って追従する「オプション」。
/// 敵弾・敵機と衝突しても無敵（ダメージを受けない）。
/// プレイヤーと同じ攻撃を行う。
/// </summary>
public class OptionFollower : MonoBehaviour
{
    private PlayerController owner;
    private int delayFrames;

    public Transform muzzleFront;
    public Transform muzzleDiagonal;

    public void Init(PlayerController player, int delay)
    {
        owner = player;
        delayFrames = delay;
    }

    private void Update()
    {
        if (owner == null) return;

        int index = Mathf.Min(delayFrames, owner.positionHistory.Count - 1);
        if (index >= 0)
        {
            transform.position = owner.positionHistory[index];
        }
    }

    /// <summary>プレイヤーと同じ武器種でオプションからも発射する</summary>
    public void Fire(bool useLaser, bool useDouble, GameObject normalBulletPrefab, GameObject doubleBulletPrefab, GameObject laserPrefab)
    {
        if (muzzleFront == null) return;

        if (useLaser)
        {
            Instantiate(laserPrefab, muzzleFront.position, muzzleFront.rotation);
        }
        else if (useDouble)
        {
            Instantiate(normalBulletPrefab, muzzleFront.position, muzzleFront.rotation);
            if (muzzleDiagonal != null)
                Instantiate(doubleBulletPrefab, muzzleDiagonal.position, muzzleDiagonal.rotation);
        }
        else
        {
            Instantiate(normalBulletPrefab, muzzleFront.position, muzzleFront.rotation);
        }
    }

    // オプションは無敵のため、敵弾・敵機とのトリガー判定はあえて実装しない
    // （コライダーは Trigger のままにし、ダメージ処理を持たせないことで無敵を表現）
}
