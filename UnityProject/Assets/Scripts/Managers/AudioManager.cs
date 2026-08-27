using UnityEngine;

/// <summary>
/// 効果音（SE）を一元管理するシングルトン。
/// 各SEは起動時にAudioFactoryで合成生成され、単一のAudioSourceから再生される。
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    private AudioSource source;

    private AudioClip shootClip;
    private AudioClip laserClip;
    private AudioClip missileClip;
    private AudioClip hitClip;
    private AudioClip explosionClip;
    private AudioClip powerUpClip;
    private AudioClip damageClip;
    private AudioClip gameOverClip;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        source = gameObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.spatialBlend = 0f; // 2Dサウンド（距離による減衰なし）

        shootClip = AudioFactory.CreateShootSound();
        laserClip = AudioFactory.CreateLaserSound();
        missileClip = AudioFactory.CreateShootSound(400f, 0.1f);
        hitClip = AudioFactory.CreateHitSound();
        explosionClip = AudioFactory.CreateExplosionSound();
        powerUpClip = AudioFactory.CreatePowerUpSound();
        damageClip = AudioFactory.CreateDamageSound();
        gameOverClip = AudioFactory.CreateGameOverSound();
    }

    public void PlayShoot() => PlayWithPitchVariance(shootClip, 0.5f);
    public void PlayLaser() => PlayWithPitchVariance(laserClip, 0.5f);
    public void PlayMissile() => PlayWithPitchVariance(missileClip, 0.5f);
    public void PlayHit() => PlayWithPitchVariance(hitClip, 0.4f);
    public void PlayExplosion() => PlayWithPitchVariance(explosionClip, 0.7f);
    public void PlayPowerUp() => Play(powerUpClip, 0.6f);
    public void PlayDamage() => Play(damageClip, 0.7f);
    public void PlayGameOver() => Play(gameOverClip, 0.8f);

    private void PlayWithPitchVariance(AudioClip clip, float volume)
    {
        if (clip == null) return;
        source.pitch = Random.Range(0.92f, 1.08f);
        source.PlayOneShot(clip, volume);
    }

    private void Play(AudioClip clip, float volume)
    {
        if (clip == null) return;
        source.pitch = 1f;
        source.PlayOneShot(clip, volume);
    }
}
