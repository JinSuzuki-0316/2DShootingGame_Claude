using UnityEngine;

/// <summary>
/// アセットファイルを一切使わず、実行時にコードだけでプレースホルダー用の
/// スプライト（四角・円・三角）を生成するヘルパークラス。
/// これにより、画像素材やPrefabアセットを用意しなくてもゲームが動作する。
/// </summary>
public static class SpriteFactory
{
    private const int Size = 16;
    private const float PixelsPerUnit = 16f;

    public static Sprite CreateSquare(Color color)
    {
        Texture2D tex = new Texture2D(Size, Size);
        Color[] pixels = new Color[Size * Size];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = color;
        return Build(tex, pixels);
    }

    public static Sprite CreateCircle(Color color)
    {
        Texture2D tex = new Texture2D(Size, Size);
        Color[] pixels = new Color[Size * Size];
        Vector2 center = new Vector2(Size / 2f, Size / 2f);
        float radius = Size / 2f;

        for (int y = 0; y < Size; y++)
        {
            for (int x = 0; x < Size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), center);
                pixels[y * Size + x] = dist <= radius ? color : Color.clear;
            }
        }
        return Build(tex, pixels);
    }

    /// <summary>右向きの三角形（自機の見た目用）</summary>
    public static Sprite CreateTriangle(Color color)
    {
        Texture2D tex = new Texture2D(Size, Size);
        Color[] pixels = new Color[Size * Size];

        for (int y = 0; y < Size; y++)
        {
            // 中央(高さ半分)を頂点として右に伸びる三角形
            float halfHeight = Mathf.Abs(y - Size / 2f);
            float widthAtY = Size - halfHeight * 2f;

            for (int x = 0; x < Size; x++)
            {
                pixels[y * Size + x] = x <= widthAtY ? color : Color.clear;
            }
        }
        return Build(tex, pixels);
    }

    /// <summary>横長の矩形（レーザー・地面用）</summary>
    public static Sprite CreateBar(Color color, int width, int height)
    {
        Texture2D tex = new Texture2D(width, height);
        Color[] pixels = new Color[width * height];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = color;

        tex.filterMode = FilterMode.Point;
        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), PixelsPerUnit);
    }

    private static Sprite Build(Texture2D tex, Color[] pixels)
    {
        tex.filterMode = FilterMode.Point;
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, Size, Size), new Vector2(0.5f, 0.5f), PixelsPerUnit);
    }
}
