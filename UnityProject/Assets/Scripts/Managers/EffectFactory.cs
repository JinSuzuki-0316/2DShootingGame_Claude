using UnityEngine;

/// <summary>
/// 画像アセットを使わず、Unity標準のParticleSystemだけでヒット/爆発エフェクトを生成する。
/// 生成されたエフェクトは再生完了後に自動的に自身を破棄する（StopAction.Destroy）。
/// </summary>
public static class EffectFactory
{
    private static Material _particleMaterial;

    private static Material GetParticleMaterial()
    {
        if (_particleMaterial == null)
        {
            _particleMaterial = new Material(Shader.Find("Sprites/Default"));
        }
        return _particleMaterial;
    }

    /// <summary>弾が当たった瞬間の小さな火花エフェクト</summary>
    public static GameObject CreateHitSpark(Color color)
    {
        return CreateBurst("HitSpark", color, particleCount: 8, startSpeed: 3f,
            startSize: 0.12f, lifetime: 0.22f, gravity: 0.4f);
    }

    /// <summary>敵・自機が撃破された際の大きめの爆発エフェクト</summary>
    public static GameObject CreateExplosion(Color color)
    {
        return CreateBurst("Explosion", color, particleCount: 24, startSpeed: 4.5f,
            startSize: 0.24f, lifetime: 0.5f, gravity: 0.15f);
    }

    private static GameObject CreateBurst(string name, Color color, int particleCount, float startSpeed,
        float startSize, float lifetime, float gravity)
    {
        GameObject go = new GameObject(name);
        // ParticleSystemはAddComponent直後に自動再生されてしまうため、
        // テンプレートとして保持する間は非アクティブのまま構築する。
        go.SetActive(false);

        ParticleSystem ps = go.AddComponent<ParticleSystem>();

        var main = ps.main;
        main.duration = lifetime;
        main.loop = false;
        main.startLifetime = lifetime;
        main.startSpeed = startSpeed;
        main.startSize = startSize;
        main.startColor = color;
        main.gravityModifier = gravity;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.stopAction = ParticleSystemStopAction.Destroy;

        var emission = ps.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, (short)particleCount) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.05f;

        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Color bright = Color.Lerp(color, Color.white, 0.4f);
        Gradient grad = new Gradient();
        grad.SetKeys(
            new GradientColorKey[]
            {
                new GradientColorKey(bright, 0f),
                new GradientColorKey(color, 0.6f),
                new GradientColorKey(color, 1f)
            },
            new GradientAlphaKey[]
            {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(0.8f, 0.6f),
                new GradientAlphaKey(0f, 1f)
            });
        colorOverLifetime.color = grad;

        var sizeOverLifetime = ps.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        AnimationCurve sizeCurve = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 0f));
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

        ParticleSystemRenderer renderer = go.GetComponent<ParticleSystemRenderer>();
        renderer.material = GetParticleMaterial();
        renderer.sortingOrder = 15;

        return go;
    }
}
