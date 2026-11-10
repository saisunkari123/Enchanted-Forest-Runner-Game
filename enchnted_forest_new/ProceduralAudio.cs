using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Sylvan Rush - Procedural Audio Synthesizer
/// Generates real-time sound effects and music clips directly in Unity memory without requiring external audio files.
/// </summary>
public class ProceduralAudio : MonoBehaviour
{
    public static ProceduralAudio Instance { get; private set; }

    private AudioSource sfxSource;
    private AudioSource musicSource;

    private AudioClip jumpClip;
    private AudioClip laneSwitchClip;
    private AudioClip orbClip;
    private AudioClip shatterClip;
    private AudioClip gameOverClip;
    private AudioClip powerUpClip;

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

        sfxSource = gameObject.AddComponent<AudioSource>();
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = true;

        GenerateClips();
    }

    void GenerateClips()
    {
        jumpClip = CreateTone(260f, 720f, 0.18f, WaveType.Triangle);
        laneSwitchClip = CreateTone(340f, 520f, 0.09f, WaveType.Sine);
        orbClip = CreateCoinChime(659.25f, 0.24f);
        shatterClip = CreateNoiseBurst(0.28f);
        gameOverClip = CreateDefeatMelody();
        powerUpClip = CreatePowerUpChord();
    }

    public void StartMusic()
    {
        // Simple ambient pulse loop
        if (musicSource.clip == null)
        {
            musicSource.clip = CreateAmbientMusicLoop(4.0f);
        }
        musicSource.volume = 0.4f;
        musicSource.Play();
    }

    public void StopMusic()
    {
        musicSource.Stop();
    }

    public void PlayJump() => sfxSource.PlayOneShot(jumpClip, 0.6f);
    public void PlayLaneSwitch(int dir) => sfxSource.PlayOneShot(laneSwitchClip, 0.45f);
    public void PlayOrbPickup() => sfxSource.PlayOneShot(orbClip, 0.5f);
    public void PlayShieldShatter() => sfxSource.PlayOneShot(shatterClip, 0.7f);
    public void PlayGameOverSound() => sfxSource.PlayOneShot(gameOverClip, 0.85f);
    public void PlayPowerUpSound(PowerUpType type) => sfxSource.PlayOneShot(powerUpClip, 0.7f);

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

            // Amplitude envelope: fade out
            val *= (1f - t);
            samples[i] = val * 0.45f;
        }

        AudioClip clip = AudioClip.Create("Tone", sampleCount, 1, sampleRate, false);
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
            float currentFreq = t < 0.05f ? freq : freq * 1.334f;
            float val = Mathf.Sin(2f * Mathf.PI * currentFreq * t) + 0.5f * Mathf.Sin(2f * Mathf.PI * currentFreq * 2f * t);
            float env = Mathf.Exp(-t * 12f);
            samples[i] = val * env * 0.35f;
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
            samples[i] = (Random.value * 2f - 1f) * (1f - t) * 0.4f;
        }

        AudioClip clip = AudioClip.Create("Noise", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    AudioClip CreatePowerUpChord()
    {
        int sampleRate = 44100;
        float duration = 0.45f;
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
            samples[i] = (val / notes.Length) * 0.4f;
        }

        AudioClip clip = AudioClip.Create("PowerUp", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    AudioClip CreateDefeatMelody()
    {
        int sampleRate = 44100;
        float duration = 0.85f;
        int sampleCount = Mathf.RoundToInt(sampleRate * duration);
        float[] samples = new float[sampleCount];

        // 4 descending notes (A4, G4, F4, D4)
        float[] freqs = { 440f, 392f, 349.23f, 293.66f };
        float noteDur = duration / freqs.Length;

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            int noteIndex = Mathf.Clamp(Mathf.FloorToInt(t / noteDur), 0, freqs.Length - 1);
            float noteLocalT = t - (noteIndex * noteDur);

            float freq = freqs[noteIndex];
            float val = Mathf.Sin(2f * Mathf.PI * freq * t) * (1f - noteLocalT / noteDur);
            samples[i] = val * 0.4f;
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
            // D Dorian chord drone (D2, A2, F3)
            float val = 0.5f * Mathf.Sin(2f * Mathf.PI * 73.42f * t) +
                        0.3f * Mathf.Sin(2f * Mathf.PI * 110.0f * t) +
                        0.2f * Mathf.Sin(2f * Mathf.PI * 174.61f * t);
            samples[i] = val * 0.25f;
        }

        AudioClip clip = AudioClip.Create("MusicLoop", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }
}
