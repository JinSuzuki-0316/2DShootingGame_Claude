using UnityEngine;

/// <summary>
/// アセットファイルを一切使わず、実行時にコードだけでスタイリッシュな
/// ネオン風プレースホルダースプライト（発光・グラデーション・輪郭付き）を生成する。
/// </summary>
public static class SpriteFactory
{
    private const int Size = 32;
    private const float PixelsPerUnit = 32f;

    // ------------------------------------------------------------
    // 円（発光グロー付き）：弾・カプセル・バリア・オプションなどに使用
    // ------------------------------------------------------------
    public static Sprite CreateCircle(Color color)
    {
        Texture2D tex = new Texture2D(Size, Size);
        Color[] pixels = new Color[Size * Size];
        Vector2 center = new Vector2(Size / 2f, Size / 2f);
        float radius = Size / 2f;
        Color coreColor = Color.Lerp(color, Color.white, 0.55f);
        float baseAlpha = color.a;

        for (int y = 0; y < Size; y++)
        {
            for (int x = 0; x < Size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), center);
                float t = dist / radius;
                Color c;

                if (t <= 0.75f)
                {
                    c = Color.Lerp(coreColor, color, t / 0.75f);
                    c.a = baseAlpha;
                }
                else if (t <= 1f)
                {
                    c = color;
                    c.a = Mathf.Lerp(baseAlpha, baseAlpha * 0.45f, (t - 0.75f) / 0.25f);
                }
                else if (t <= 1.3f)
                {
                    c = color;
                    c.a = Mathf.Lerp(baseAlpha * 0.35f, 0f, (t - 1f) / 0.3f);
                }
                else
                {
                    c = Color.clear;
                }
                pixels[y * Size + x] = c;
            }
        }
        return Build(tex, pixels);
    }

    /// <summary>目玉のような二重リング＋瞳（中ボス「ビッグアイ」風）</summary>
    public static Sprite CreateEye(Color scleraColor, Color pupilColor)
    {
        Texture2D tex = new Texture2D(Size, Size);
        Color[] pixels = new Color[Size * Size];
        Vector2 center = new Vector2(Size / 2f, Size / 2f);
        float outerR = Size / 2f;
        float pupilR = Size / 4.5f;
        Color outline = Color.Lerp(scleraColor, Color.black, 0.65f);
        Color scleraHighlight = Color.Lerp(scleraColor, Color.white, 0.25f);
        Color pupilGlow = Color.Lerp(pupilColor, Color.white, 0.6f);

        for (int y = 0; y < Size; y++)
        {
            for (int x = 0; x < Size; x++)
            {
                Vector2 p = new Vector2(x + 0.5f, y + 0.5f);
                float dist = Vector2.Distance(p, center);
                Color c;

                if (dist <= pupilR * 0.4f)
                {
                    c = pupilGlow;
                }
                else if (dist <= pupilR)
                {
                    c = pupilColor;
                }
                else if (dist <= pupilR + 1.5f)
                {
                    c = outline;
                }
                else if (dist <= outerR - 2f)
                {
                    float highlightAmt = Mathf.Clamp01(1f - Vector2.Distance(p, center + new Vector2(-outerR * 0.35f, outerR * 0.35f)) / outerR);
                    c = Color.Lerp(scleraColor, scleraHighlight, highlightAmt);
                }
                else if (dist <= outerR)
                {
                    c = outline;
                }
                else
                {
                    c = Color.clear;
                }
                pixels[y * Size + x] = c;
            }
        }
        return Build(tex, pixels);
    }

    // ------------------------------------------------------------
    // 三角形（自機用：二層シェーディング＋アウトライン）
    // ------------------------------------------------------------
    public static Sprite CreateTriangle(Color color)
    {
        Texture2D tex = new Texture2D(Size, Size);
        Color[] pixels = new Color[Size * Size];
        Color highlight = Color.Lerp(color, Color.white, 0.55f);
        Color shadow = Color.Lerp(color, Color.black, 0.3f);
        Color outline = Color.Lerp(color, Color.black, 0.65f);

        for (int y = 0; y < Size; y++)
        {
            float halfHeight = Mathf.Abs(y - Size / 2f);
            float widthAtY = Size - halfHeight * 2f;

            for (int x = 0; x < Size; x++)
            {
                if (x <= widthAtY && widthAtY > 0f)
                {
                    bool isEdge = x > widthAtY - 2f || x < 1.5f || halfHeight > widthAtY / 2f - 1.5f;
                    if (isEdge)
                    {
                        pixels[y * Size + x] = outline;
                    }
                    else
                    {
                        pixels[y * Size + x] = y < Size / 2f ? highlight : shadow;
                    }
                }
                else
                {
                    pixels[y * Size + x] = Color.clear;
                }
            }
        }
        return Build(tex, pixels);
    }

    // ------------------------------------------------------------
    // 四角形（縁取り付き）：歩行敵などに使用
    // ------------------------------------------------------------
    public static Sprite CreateSquare(Color color)
    {
        Texture2D tex = new Texture2D(Size, Size);
        Color[] pixels = new Color[Size * Size];
        Color border = Color.Lerp(color, Color.white, 0.6f);
        Color shadow = Color.Lerp(color, Color.black, 0.25f);
        const int borderWidth = 2;

        for (int y = 0; y < Size; y++)
        {
            for (int x = 0; x < Size; x++)
            {
                bool isBorder = x < borderWidth || x >= Size - borderWidth || y < borderWidth || y >= Size - borderWidth;
                pixels[y * Size + x] = isBorder ? border : (y < Size / 2 ? color : shadow);
            }
        }
        return Build(tex, pixels);
    }

    // ------------------------------------------------------------
    // ダイヤ形（小型機用：鋭く攻撃的な印象）
    // ------------------------------------------------------------
    public static Sprite CreateDiamond(Color color)
    {
        Texture2D tex = new Texture2D(Size, Size);
        Color[] pixels = new Color[Size * Size];
        Vector2 center = new Vector2(Size / 2f, Size / 2f);
        float radius = Size / 2f;
        Color outline = Color.Lerp(color, Color.white, 0.5f);
        Color core = Color.Lerp(color, Color.white, 0.2f);

        for (int y = 0; y < Size; y++)
        {
            for (int x = 0; x < Size; x++)
            {
                float manhattan = Mathf.Abs(x + 0.5f - center.x) + Mathf.Abs(y + 0.5f - center.y);
                if (manhattan <= radius)
                {
                    bool isEdge = manhattan > radius - 2.5f;
                    pixels[y * Size + x] = isEdge ? outline : core;
                }
                else
                {
                    pixels[y * Size + x] = Color.clear;
                }
            }
        }
        return Build(tex, pixels);
    }

    // ------------------------------------------------------------
    // 八角形（砲台用：機械的でがっしりした印象）
    // ------------------------------------------------------------
    public static Sprite CreateOctagon(Color color)
    {
        Texture2D tex = new Texture2D(Size, Size);
        Color[] pixels = new Color[Size * Size];
        Vector2 center = new Vector2(Size / 2f, Size / 2f);
        float radius = Size / 2f;
        Color outline = Color.Lerp(color, Color.white, 0.5f);
        Color core = Color.Lerp(color, Color.black, 0.1f);

        for (int y = 0; y < Size; y++)
        {
            for (int x = 0; x < Size; x++)
            {
                float dx = Mathf.Abs(x + 0.5f - center.x);
                float dy = Mathf.Abs(y + 0.5f - center.y);
                float chebyshev = Mathf.Max(dx, dy);
                float manhattan = dx + dy;

                bool inside = chebyshev <= radius - 1f && manhattan <= radius * 1.45f;
                if (inside)
                {
                    bool isEdge = chebyshev > radius - 3f || manhattan > radius * 1.45f - 3f;
                    pixels[y * Size + x] = isEdge ? outline : core;
                }
                else
                {
                    pixels[y * Size + x] = Color.clear;
                }
            }
        }
        return Build(tex, pixels);
    }

    // ------------------------------------------------------------
    // ネオン発光バー（レーザー用：中心が最も明るいグラデーション）
    // ------------------------------------------------------------
    public static Sprite CreateNeonBar(Color coreColor, int width, int height)
    {
        Texture2D tex = new Texture2D(width, height);
        Color[] pixels = new Color[width * height];
        Color bright = Color.Lerp(coreColor, Color.white, 0.75f);

        for (int y = 0; y < height; y++)
        {
            float t = Mathf.Abs(y - height / 2f) / (height / 2f);
            Color c = Color.Lerp(bright, coreColor, t);
            c.a = Mathf.Lerp(1f, 0.15f, t);
            for (int x = 0; x < width; x++)
            {
                pixels[y * width + x] = c;
            }
        }

        tex.filterMode = FilterMode.Bilinear;
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), PixelsPerUnit);
    }

    // ------------------------------------------------------------
    // 単色フラットバー（地面など）：上端に軽くハイライトを付ける
    // ------------------------------------------------------------
    public static Sprite CreateBar(Color color, int width, int height)
    {
        Texture2D tex = new Texture2D(width, height);
        Color[] pixels = new Color[width * height];
        Color topHighlight = Color.Lerp(color, Color.white, 0.35f);
        int highlightRows = Mathf.Max(1, height / 12);

        for (int y = 0; y < height; y++)
        {
            Color rowColor = y >= height - highlightRows ? topHighlight : color;
            for (int x = 0; x < width; x++)
            {
                pixels[y * width + x] = rowColor;
            }
        }

        tex.filterMode = FilterMode.Point;
        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), PixelsPerUnit);
    }

    // ------------------------------------------------------------
    // 星空タイル（背景用：上下グラデーション＋ランダムな星）
    // ------------------------------------------------------------
    public static Sprite CreateStarfieldTile(int width, int height, int starCount, Color topColor, Color bottomColor)
    {
        Texture2D tex = new Texture2D(width, height);
        Color[] pixels = new Color[width * height];

        for (int y = 0; y < height; y++)
        {
            float t = (float)y / height;
            Color rowColor = Color.Lerp(bottomColor, topColor, t);
            for (int x = 0; x < width; x++)
            {
                pixels[y * width + x] = rowColor;
            }
        }

        for (int i = 0; i < starCount; i++)
        {
            int sx = Random.Range(0, width);
            int sy = Random.Range(0, height);
            float brightness = Random.Range(0.5f, 1f);
            Color starColor = new Color(brightness, brightness, Mathf.Min(1f, brightness + 0.1f), 1f);
            pixels[sy * width + sx] = starColor;

            if (Random.value < 0.12f && sx + 1 < width)
            {
                pixels[sy * width + (sx + 1)] = starColor;
            }
        }

        tex.filterMode = FilterMode.Point;
        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), PixelsPerUnit);
    }

    private static Sprite Build(Texture2D tex, Color[] pixels)
    {
        tex.filterMode = FilterMode.Bilinear;
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, Size, Size), new Vector2(0.5f, 0.5f), PixelsPerUnit);
    }
}
