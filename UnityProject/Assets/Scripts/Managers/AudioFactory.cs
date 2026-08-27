using UnityEngine;

/// <summary>
/// 音声ファイルを一切使わず、AudioClip.Createで波形データを直接書き込んで
/// レトロ風の効果音（SE）を実行時に合成生成するヘルパー。
/// </summary>
public static class AudioFactory
{
    private const int SampleRate = 44100;

    /// <summary>ピコッとした発射音（短い減衰つきの高音・下降ピッチ）</summary>
    public static AudioClip CreateShootSound(float frequency = 900f, float duration = 0.08f)
    {
        int samples = Mathf.CeilToInt(SampleRate * duration);
        float[] data = new float[samples];
        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / SampleRate;
            float envelope = Mathf.Exp(-t * 30f);
            float freq = frequency * (1f - (t / duration) * 0.4f);
            data[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * envelope * 0.5f;
        }
        return BuildClip("Shoot", data);
    }

    /// <summary>レーザー発射音（矩形波によるブザー感のある音）</summary>
    public static AudioClip CreateLaserSound()
    {
        float duration = 0.15f;
        int samples = Mathf.CeilToInt(SampleRate * duration);
        float[] data = new float[samples];
        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / SampleRate;
            float envelope = Mathf.Exp(-t * 12f);
            float freq = 500f + Mathf.Sin(t * 40f) * 80f;
            data[i] = Mathf.Sign(Mathf.Sin(2f * Mathf.PI * freq * t)) * envelope * 0.3f;
        }
        return BuildClip("LaserShoot", data);
    }

    /// <summary>被弾/ヒット音（短いノイズバースト）</summary>
    public static AudioClip CreateHitSound()
    {
        float duration = 0.08f;
        int samples = Mathf.CeilToInt(SampleRate * duration);
        float[] data = new float[samples];
        System.Random rng = new System.Random(12345);
        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / SampleRate;
            float envelope = Mathf.Exp(-t * 40f);
            data[i] = ((float)rng.NextDouble() * 2f - 1f) * envelope * 0.4f;
        }
        return BuildClip("Hit", data);
    }

    /// <summary>爆発音（ノイズ＋低音ランブルの減衰）</summary>
    public static AudioClip CreateExplosionSound(float duration = 0.4f)
    {
        int samples = Mathf.CeilToInt(SampleRate * duration);
        float[] data = new float[samples];
        System.Random rng = new System.Random(54321);
        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / SampleRate;
            float envelope = Mathf.Exp(-t * 6f);
            float noise = (float)rng.NextDouble() * 2f - 1f;
            float rumble = Mathf.Sin(2f * Mathf.PI * 60f * t);
            data[i] = (noise * 0.6f + rumble * 0.4f) * envelope * 0.6f;
        }
        return BuildClip("Explosion", data);
    }

    /// <summary>パワーアップ取得音（上昇するピッチ）</summary>
    public static AudioClip CreatePowerUpSound()
    {
        float duration = 0.25f;
        int samples = Mathf.CeilToInt(SampleRate * duration);
        float[] data = new float[samples];
        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / SampleRate;
            float progress = t / duration;
            float envelope = Mathf.Exp(-t * 5f) * (1f - progress * 0.3f);
            float freq = Mathf.Lerp(400f, 1400f, progress);
            data[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * envelope * 0.5f;
        }
        return BuildClip("PowerUp", data);
    }

    /// <summary>被ダメージ警告音（下降する低いブザー）</summary>
    public static AudioClip CreateDamageSound()
    {
        float duration = 0.3f;
        int samples = Mathf.CeilToInt(SampleRate * duration);
        float[] data = new float[samples];
        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / SampleRate;
            float progress = t / duration;
            float envelope = Mathf.Exp(-t * 8f);
            float freq = Mathf.Lerp(500f, 100f, progress);
            data[i] = Mathf.Sign(Mathf.Sin(2f * Mathf.PI * freq * t)) * envelope * 0.4f;
        }
        return BuildClip("Damage", data);
    }

    /// <summary>ゲームオーバー音（長めの下降トーン）</summary>
    public static AudioClip CreateGameOverSound()
    {
        float duration = 0.8f;
        int samples = Mathf.CeilToInt(SampleRate * duration);
        float[] data = new float[samples];
        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / SampleRate;
            float progress = t / duration;
            float envelope = Mathf.Exp(-t * 2.5f);
            float freq = Mathf.Lerp(300f, 60f, progress);
            data[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * envelope * 0.5f;
        }
        return BuildClip("GameOver", data);
    }

    private static AudioClip BuildClip(string name, float[] data)
    {
        AudioClip clip = AudioClip.Create(name, data.Length, 1, SampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }
}
