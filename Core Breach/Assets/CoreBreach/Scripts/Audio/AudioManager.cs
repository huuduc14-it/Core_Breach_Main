using UnityEngine;

/// <summary>
/// Quản lý phát âm thanh hiệu ứng (SFX) trong Core Breach theo yêu cầu đồ án:
/// - Tối thiểu 5+ âm thanh khác nhau: Bắn, trúng đạn, né đòn, chết, quá nhiệt, nhặt đồ...
/// - Tự động tổng hợp âm thanh Sci-Fi trực tiếp (Procedural Audio) nếu chưa kéo file ngoài, đảm bảo 100% không bao giờ bị mất âm thanh.
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource musicSource;

    [Header("Player SFX")]
    [SerializeField] private AudioClip shootSound;
    [SerializeField] private AudioClip dashSound;
    [SerializeField] private AudioClip playerHurtSound;
    [SerializeField] private AudioClip playerDeathSound;
    [SerializeField] private AudioClip overheatWarningSound;

    [Header("Enemy & Combat SFX")]
    [SerializeField] private AudioClip enemyHitSound;
    [SerializeField] private AudioClip enemyDeathSound;
    [SerializeField] private AudioClip bossRoarSound;

    [Header("World SFX")]
    [SerializeField] private AudioClip scrapCollectSound;
    [SerializeField] private AudioClip roomClearSound;
    [SerializeField] private AudioClip portalWarpSound;

    public bool IsSFXEnabled { get; set; } = true;
    public bool IsMusicEnabled { get; set; } = true;
    public float MasterVolume { get; set; } = 1.0f;

    private const string PREF_SFX = "CoreBreach_SFXEnabled";
    private const string PREF_MUSIC = "CoreBreach_MusicEnabled";
    private const string PREF_VOLUME = "CoreBreach_MasterVolume";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;

        IsSFXEnabled = PlayerPrefs.GetInt(PREF_SFX, 1) == 1;
        IsMusicEnabled = PlayerPrefs.GetInt(PREF_MUSIC, 1) == 1;
        MasterVolume = PlayerPrefs.GetFloat(PREF_VOLUME, 1.0f);

        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
        }

        EnsureAudioClips();
    }

    public void SetSFXEnabled(bool enabled)
    {
        IsSFXEnabled = enabled;
        PlayerPrefs.SetInt(PREF_SFX, enabled ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void SetMusicEnabled(bool enabled)
    {
        IsMusicEnabled = enabled;
        PlayerPrefs.SetInt(PREF_MUSIC, enabled ? 1 : 0);
        PlayerPrefs.Save();
        if (musicSource != null)
        {
            musicSource.mute = !enabled;
        }
    }

    public void SetMasterVolume(float volume)
    {
        MasterVolume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat(PREF_VOLUME, MasterVolume);
        PlayerPrefs.Save();
        if (sfxSource != null) sfxSource.volume = MasterVolume;
        if (musicSource != null) musicSource.volume = MasterVolume;
    }

    public void PlaySFX(AudioClip clip, float volume = 1f, bool randomizePitch = true)
    {
        if (!IsSFXEnabled) return;
        if (clip != null && sfxSource != null)
        {
            if (randomizePitch)
            {
                sfxSource.pitch = UnityEngine.Random.Range(0.93f, 1.07f);
            }
            else
            {
                sfxSource.pitch = 1f;
            }
            sfxSource.PlayOneShot(clip, volume * MasterVolume);
        }
    }

    public void PlayShoot() => PlaySFX(shootSound, 0.7f);
    public void PlayDash() => PlaySFX(dashSound, 0.8f);
    public void PlayPlayerHurt() => PlaySFX(playerHurtSound, 0.9f);
    public void PlayPlayerDeath() => PlaySFX(playerDeathSound, 1f);
    public void PlayOverheatWarning() => PlaySFX(overheatWarningSound, 0.8f);
    public void PlayEnemyHit() => PlaySFX(enemyHitSound, 0.6f);
    public void PlayEnemyDeath() => PlaySFX(enemyDeathSound, 0.8f);
    public void PlayBossRoar() => PlaySFX(bossRoarSound, 1f);
    public void PlayScrapCollect() => PlaySFX(scrapCollectSound, 0.75f);
    public void PlayRoomClear() => PlaySFX(roomClearSound, 0.9f);
    public void PlayPortalWarp() => PlaySFX(portalWarpSound, 1.0f, false);

    // =========================================================================
    // PROCEDURAL AUDIO SYNTHESIZER (Tự động tạo âm thanh Sci-Fi sắc nét)
    // =========================================================================
    private void EnsureAudioClips()
    {
        if (shootSound == null) shootSound = CreateLaserSound();
        if (dashSound == null) dashSound = CreateDashSound();
        if (playerHurtSound == null) playerHurtSound = CreateHurtSound();
        if (playerDeathSound == null) playerDeathSound = CreateExplosionSound(0.6f);
        if (overheatWarningSound == null) overheatWarningSound = CreateAlarmSound();
        if (enemyHitSound == null) enemyHitSound = CreateHitSound();
        if (enemyDeathSound == null) enemyDeathSound = CreateExplosionSound(0.35f);
        if (bossRoarSound == null) bossRoarSound = CreateBossRoarSound();
        if (scrapCollectSound == null) scrapCollectSound = CreateChimeSound();
        if (roomClearSound == null) roomClearSound = CreateFanfareSound();
        if (portalWarpSound == null) portalWarpSound = CreateWarpSound();
    }

    private AudioClip CreateLaserSound()
    {
        int rate = 44100;
        float duration = 0.12f;
        int count = (int)(rate * duration);
        float[] data = new float[count];
        for (int i = 0; i < count; i++)
        {
            float t = (float)i / count;
            float freq = Mathf.Lerp(980f, 220f, t);
            float env = 1f - t;
            data[i] = Mathf.Sin(2f * Mathf.PI * freq * i / rate) * env * 0.45f;
        }
        AudioClip clip = AudioClip.Create("SFX_Laser", count, 1, rate, false);
        clip.SetData(data, 0);
        return clip;
    }

    private AudioClip CreateDashSound()
    {
        int rate = 44100;
        float duration = 0.15f;
        int count = (int)(rate * duration);
        float[] data = new float[count];
        for (int i = 0; i < count; i++)
        {
            float t = (float)i / count;
            float env = Mathf.Sin(t * Mathf.PI);
            float noise = (Random.value * 2f - 1f) * 0.5f;
            float tone = Mathf.Sin(2f * Mathf.PI * 300f * i / rate) * 0.5f;
            data[i] = (noise + tone) * env * 0.35f;
        }
        AudioClip clip = AudioClip.Create("SFX_Dash", count, 1, rate, false);
        clip.SetData(data, 0);
        return clip;
    }

    private AudioClip CreateHurtSound()
    {
        int rate = 44100;
        float duration = 0.14f;
        int count = (int)(rate * duration);
        float[] data = new float[count];
        for (int i = 0; i < count; i++)
        {
            float t = (float)i / count;
            float env = 1f - t;
            float tone = Mathf.Sin(2f * Mathf.PI * 140f * i / rate);
            float noise = (Random.value * 2f - 1f) * 0.3f;
            data[i] = (tone + noise) * env * 0.5f;
        }
        AudioClip clip = AudioClip.Create("SFX_Hurt", count, 1, rate, false);
        clip.SetData(data, 0);
        return clip;
    }

    private AudioClip CreateExplosionSound(float duration)
    {
        int rate = 44100;
        int count = (int)(rate * duration);
        float[] data = new float[count];
        for (int i = 0; i < count; i++)
        {
            float t = (float)i / count;
            float env = Mathf.Pow(1f - t, 2f);
            float noise = (Random.value * 2f - 1f);
            float sub = Mathf.Sin(2f * Mathf.PI * 80f * i / rate) * 0.5f;
            data[i] = (noise * 0.6f + sub) * env * 0.45f;
        }
        AudioClip clip = AudioClip.Create("SFX_Explosion", count, 1, rate, false);
        clip.SetData(data, 0);
        return clip;
    }

    private AudioClip CreateAlarmSound()
    {
        int rate = 44100;
        float duration = 0.22f;
        int count = (int)(rate * duration);
        float[] data = new float[count];
        for (int i = 0; i < count; i++)
        {
            float t = (float)i / count;
            float freq = t < 0.5f ? 1100f : 800f;
            data[i] = Mathf.Sign(Mathf.Sin(2f * Mathf.PI * freq * i / rate)) * 0.3f * (1f - t * 0.5f);
        }
        AudioClip clip = AudioClip.Create("SFX_Alarm", count, 1, rate, false);
        clip.SetData(data, 0);
        return clip;
    }

    private AudioClip CreateHitSound()
    {
        int rate = 44100;
        float duration = 0.06f;
        int count = (int)(rate * duration);
        float[] data = new float[count];
        for (int i = 0; i < count; i++)
        {
            float t = (float)i / count;
            float freq = Mathf.Lerp(1400f, 600f, t);
            data[i] = Mathf.Sin(2f * Mathf.PI * freq * i / rate) * (1f - t) * 0.4f;
        }
        AudioClip clip = AudioClip.Create("SFX_Hit", count, 1, rate, false);
        clip.SetData(data, 0);
        return clip;
    }

    private AudioClip CreateChimeSound()
    {
        int rate = 44100;
        float duration = 0.2f;
        int count = (int)(rate * duration);
        float[] data = new float[count];
        for (int i = 0; i < count; i++)
        {
            float t = (float)i / count;
            float freq = t < 0.5f ? 587.33f : 880f; // D5 -> A5
            data[i] = Mathf.Sin(2f * Mathf.PI * freq * i / rate) * (1f - t) * 0.4f;
        }
        AudioClip clip = AudioClip.Create("SFX_Chime", count, 1, rate, false);
        clip.SetData(data, 0);
        return clip;
    }

    private AudioClip CreateFanfareSound()
    {
        int rate = 44100;
        float duration = 0.45f;
        int count = (int)(rate * duration);
        float[] data = new float[count];
        for (int i = 0; i < count; i++)
        {
            float t = (float)i / count;
            float freq = t < 0.33f ? 523.25f : (t < 0.66f ? 659.25f : 783.99f); // C5 -> E5 -> G5
            data[i] = Mathf.Sin(2f * Mathf.PI * freq * i / rate) * (1f - t * 0.8f) * 0.4f;
        }
        AudioClip clip = AudioClip.Create("SFX_Fanfare", count, 1, rate, false);
        clip.SetData(data, 0);
        return clip;
    }

    private AudioClip CreateBossRoarSound()
    {
        int rate = 44100;
        float duration = 0.55f;
        int count = (int)(rate * duration);
        float[] data = new float[count];
        for (int i = 0; i < count; i++)
        {
            float t = (float)i / count;
            float freq = Mathf.Lerp(120f, 65f, t);
            float noise = (Random.value * 2f - 1f) * 0.4f;
            float tone = Mathf.Sin(2f * Mathf.PI * freq * i / rate);
            data[i] = (tone + noise) * (1f - t) * 0.6f;
        }
        AudioClip clip = AudioClip.Create("SFX_BossRoar", count, 1, rate, false);
        clip.SetData(data, 0);
        return clip;
    }

    private AudioClip CreateWarpSound()
    {
        int rate = 44100;
        float duration = 0.65f;
        int count = (int)(rate * duration);
        float[] data = new float[count];
        for (int i = 0; i < count; i++)
        {
            float t = (float)i / count;
            // Sweep frequency from 150Hz up to 1200Hz
            float freq = Mathf.Lerp(150f, 1200f, t * t);
            float harmonic = freq * 1.5f;
            float env = Mathf.Sin(t * Mathf.PI * 0.85f);
            float pulse = Mathf.Sin(2f * Mathf.PI * 25f * t); // 25Hz tremolo
            float signal = Mathf.Sin(2f * Mathf.PI * freq * i / rate) * 0.5f 
                         + Mathf.Sin(2f * Mathf.PI * harmonic * i / rate) * 0.25f;
            data[i] = signal * env * (0.8f + pulse * 0.2f) * 0.5f;
        }
        AudioClip clip = AudioClip.Create("SFX_PortalWarp", count, 1, rate, false);
        clip.SetData(data, 0);
        return clip;
    }
}
