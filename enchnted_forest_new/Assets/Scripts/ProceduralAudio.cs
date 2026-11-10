using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Sylvan Rush - Procedural Audio Synthesizer (Pure 2D Crisp Audio)
/// </summary>
public class ProceduralAudio : MonoBehaviour
{
    public static ProceduralAudio Instance { get; private set; }

    private AudioSource sfxSource;
    private AudioSource musicSource;

    private AudioClip jumpClip;
    private AudioClip laneSwitchClip;
    private AudioClip slideClip;
    private AudioClip shatterClip;
    private AudioClip gameOverClip;
    private AudioClip powerUpClip;

    private AudioClip[] orbChimeScale;
    private int orbStreak = 0;
    private float orbStreakTimer = 0f;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Configure 2D AudioSources (spatialBlend = 0 ensures full volume regardless of camera distance)
        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.spatialBlend = 0f;
        sfxSource.volume = 1.0f;
        sfxSource.playOnAwake = false;

        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.spatialBlend = 0f;
        musicSource.volume = 0.75f;
        musicSource.loop = true;
        musicSource.playOnAwake = false;

        GenerateClips();
    }

    void Start()
    {
        StartMusic();
    }

    void Update()
    {
        if (orbStreakTimer > 0f)
        {
            orbStreakTimer -= Time.deltaTime;
            if (orbStreakTimer <= 0f)
            {
                orbStreak = 0;
            }
        }
    }

    void GenerateClips()
    {
        jumpClip = CreateTone(280f, 750f, 0.22f, WaveType.Triangle);
        laneSwitchClip = CreateTone(380f, 540f, 0.12f, WaveType.Sine);
        slideClip = CreateNoiseSweep(0.38f);
        shatterClip = CreateNoiseBurst(0.32f);
        gameOverClip = CreateDefeatMelody();
        powerUpClip = CreatePowerUpChord();

        // 6-step rising pentatonic scale for Mana Orbs
        float[] scaleFreqs = { 523.25f, 587.33f, 659.25f, 783.99f, 880.00f, 1046.50f };
        orbChimeScale = new AudioClip[scaleFreqs.Length];
        for (int i = 0; i < scaleFreqs.Length; i++)
        {
            orbChimeScale[i] = CreateCoinChime(scaleFreqs[i], 0.25f);
        }
    }

    public void StartMusic()
    {
        if (musicSource == null) return;
        if (musicSource.clip == null)
        {
            musicSource.clip = CreateAmbientMusicLoop(4.0f);
        }
        if (!musicSource.isPlaying)
        {
            musicSource.volume = 0.65f;
            musicSource.Play();
        }
    }

    public void StopMusic()
    {
        if (musicSource != null) musicSource.Stop();
    }

    public void PlayJump()
    {
        if (sfxSource != null && jumpClip != null) sfxSource.PlayOneShot(jumpClip, 0.9f);
    }

    public void PlayLaneSwitch(int dir)
    {
        if (sfxSource != null && laneSwitchClip != null) sfxSource.PlayOneShot(laneSwitchClip, 0.8f);
    }

    public void PlaySlide()
    {
        if (sfxSource != null && slideClip != null) sfxSource.PlayOneShot(slideClip, 0.85f);
    }

    public void PlayOrbPickup()
    {
        if (sfxSource != null && orbChimeScale != null && orbChimeScale.Length > 0)
        {
            int index = Mathf.Clamp(orbStreak, 0, orbChimeScale.Length - 1);
            sfxSource.PlayOneShot(orbChimeScale[index], 0.95f);
            orbStreak = (orbStreak + 1) % orbChimeScale.Length;
            orbStreakTimer = 1.2f;
        }
    }

    public void PlayShieldShatter()
    {
        if (sfxSource != null && shatterClip != null) sfxSource.PlayOneShot(shatterClip, 1.0f);
    }

    public void PlayGameOverSound()
    {
        if (sfxSource != null && gameOverClip != null) sfxSource.PlayOneShot(gameOverClip, 1.0f);
    }

    public void PlayPowerUpSound(PowerUpType type)
    {
        if (sfxSource != null && powerUpClip != null) sfxSource.PlayOneShot(powerUpClip, 0.95f);
    }

    enum WaveType { Sine, Triangle, Sawtooth }

    AudioClip CreateTone(float startFreq, float endFreq, float duration, WaveType wave)
    {
        int sampleRate = 44100;
        int sampleCount = Mathf.RoundToInt(sampleRate * duration);
        float[] samples = new float[sampleCount];

        float phase = 0f;
        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleCount;
            float freq = Mathf.Lerp(startFreq, endFreq, t);
            phase += 2f * Mathf.PI * freq / sampleRate;

            float val = 0f;
            if (wave == WaveType.Sine) val = Mathf.Sin(phase);
            else if (wave == WaveType.Triangle) val = Mathf.PingPong(phase / Mathf.PI, 1f) * 2f - 1f;

            val *= (1f - t);
            samples[i] = Mathf.Clamp(val * 0.7f, -1f, 1f);
        }

        AudioClip clip = AudioClip.Create("Tone", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    AudioClip CreateNoiseSweep(float duration)
    {
        int sampleRate = 44100;
        int sampleCount = Mathf.RoundToInt(sampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleCount;
            float noise = (Random.value * 2f - 1f) * Mathf.Sin(t * Mathf.PI);
            samples[i] = Mathf.Clamp(noise * 0.6f, -1f, 1f);
        }

        AudioClip clip = AudioClip.Create("SlideSweep", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    AudioClip CreateCoinChime(float freq, float duration)
    {
        int sampleRate = 44100;
        int sampleCount = Mathf.RoundToInt(sampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            float currentFreq = t < 0.04f ? freq : freq * 1.334f;
            float val = Mathf.Sin(2f * Mathf.PI * currentFreq * t) + 0.6f * Mathf.Sin(2f * Mathf.PI * currentFreq * 2f * t);
            float env = Mathf.Exp(-t * 12f);
            samples[i] = Mathf.Clamp(val * env * 0.65f, -1f, 1f);
        }

        AudioClip clip = AudioClip.Create("Coin", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    AudioClip CreateNoiseBurst(float duration)
    {
        int sampleRate = 44100;
        int sampleCount = Mathf.RoundToInt(sampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleCount;
            samples[i] = Mathf.Clamp((Random.value * 2f - 1f) * (1f - t) * 0.7f, -1f, 1f);
        }

        AudioClip clip = AudioClip.Create("Noise", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    AudioClip CreatePowerUpChord()
    {
        int sampleRate = 44100;
        float duration = 0.52f;
        int sampleCount = Mathf.RoundToInt(sampleRate * duration);
        float[] samples = new float[sampleCount];
        float[] notes = { 440f, 554.37f, 659.25f, 880f };

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            float val = 0f;
            foreach (float f in notes)
            {
                val += Mathf.Sin(2f * Mathf.PI * f * t);
            }
            val *= Mathf.Exp(-t * 6f);
            samples[i] = Mathf.Clamp((val / notes.Length) * 0.75f, -1f, 1f);
        }

        AudioClip clip = AudioClip.Create("PowerUp", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    AudioClip CreateDefeatMelody()
    {
        int sampleRate = 44100;
        float duration = 0.95f;
        int sampleCount = Mathf.RoundToInt(sampleRate * duration);
        float[] samples = new float[sampleCount];

        float[] freqs = { 440f, 392f, 349.23f, 293.66f };
        float noteDur = duration / freqs.Length;

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            int noteIndex = Mathf.Clamp(Mathf.FloorToInt(t / noteDur), 0, freqs.Length - 1);
            float noteLocalT = t - (noteIndex * noteDur);

            float freq = freqs[noteIndex];
            float val = Mathf.Sin(2f * Mathf.PI * freq * t) * (1f - noteLocalT / noteDur);
            samples[i] = Mathf.Clamp(val * 0.7f, -1f, 1f);
        }

        AudioClip clip = AudioClip.Create("Defeat", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    AudioClip CreateAmbientMusicLoop(float duration)
    {
        int sampleRate = 44100;
        int sampleCount = Mathf.RoundToInt(sampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            float val = 0.5f * Mathf.Sin(2f * Mathf.PI * 73.42f * t) +
                        0.35f * Mathf.Sin(2f * Mathf.PI * 110.0f * t) +
                        0.30f * Mathf.Sin(2f * Mathf.PI * 174.61f * t) +
                        0.22f * Mathf.Sin(2f * Mathf.PI * 261.63f * t);
            samples[i] = Mathf.Clamp(val * 0.45f, -1f, 1f);
        }

        AudioClip clip = AudioClip.Create("MusicLoop", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }
}
