using UnityEngine;

/// <summary>
/// 横スクロール背景をループさせる。ボス戦中は GameManager から停止指示を受ける。
/// 同じ幅の背景タイルを2〜3枚並べて子オブジェクトとして登録して使用する。
/// </summary>
public class ScrollingBackground : MonoBehaviour
{
    public Transform[] tiles;
    public float scrollSpeed = 2f;
    public float tileWidth = 20f;

    public bool isScrolling = true;

    private void Update()
    {
        if (!isScrolling) return;

        foreach (Transform tile in tiles)
        {
            tile.position += Vector3.left * scrollSpeed * Time.deltaTime;

            if (tile.position.x <= -tileWidth)
            {
                float rightMostX = GetRightMostX();
                tile.position = new Vector3(rightMostX + tileWidth, tile.position.y, tile.position.z);
            }
        }
    }

    private float GetRightMostX()
    {
        float max = float.MinValue;
        foreach (Transform tile in tiles)
        {
            if (tile.position.x > max) max = tile.position.x;
        }
        return max;
    }
}
