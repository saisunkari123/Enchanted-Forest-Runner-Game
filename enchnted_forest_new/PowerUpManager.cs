using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PowerUpType
{
    Magnet, // Draws nearby Mana Orbs
    Shield, // Absorbs one fatal obstacle impact
    Dash    // Invulnerability + Turbo Sprint
}

public class PowerUpItem : MonoBehaviour
{
    public PowerUpType powerUpType = PowerUpType.Shield;

    public void ApplyColor()
    {
        var rend = GetComponent<Renderer>();
        if (rend != null)
        {
            switch (powerUpType)
            {
                case PowerUpType.Magnet:
                    rend.material.color = new Color(0.78f, 0.49f, 1.0f); // Purple
                    break;
                case PowerUpType.Shield:
                    rend.material.color = new Color(0.22f, 0.94f, 0.49f); // Emerald
                    break;
                case PowerUpType.Dash:
                    rend.material.color = new Color(0.0f, 0.96f, 0.83f); // Cyan
                    break;
            }
        }
    }
}

/// <summary>
/// Manages active power-ups, timers, and buff effects.
/// </summary>
public class PowerUpManager : MonoBehaviour
{
    public static PowerUpManager Instance { get; private set; }

    [Header("Timers")]
    public float magnetTimer = 0f;
    public float shieldTimer = 0f;
    public float dashTimer = 0f;

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.currentState != GameState.Playing)
            return;

        float dt = Time.deltaTime;

        // Magnet countdown
        if (magnetTimer > 0f)
        {
            magnetTimer -= dt;
            if (magnetTimer <= 0f && PlayerController.Instance != null)
            {
                PlayerController.Instance.hasMagnet = false;
            }
        }

        // Dash countdown
        if (dashTimer > 0f)
        {
            dashTimer -= dt;
            if (dashTimer <= 0f && PlayerController.Instance != null)
            {
                PlayerController.Instance.hasDash = false;
            }
        }
    }

    public void ActivatePowerUp(PowerUpType type)
    {
        if (PlayerController.Instance == null) return;

        switch (type)
        {
            case PowerUpType.Magnet:
                magnetTimer = 8.5f;
                PlayerController.Instance.hasMagnet = true;
                if (ProceduralAudio.Instance != null) ProceduralAudio.Instance.PlayPowerUpSound(PowerUpType.Magnet);
                break;

            case PowerUpType.Shield:
                shieldTimer = 14.0f;
                PlayerController.Instance.hasShield = true;
                if (ProceduralAudio.Instance != null) ProceduralAudio.Instance.PlayPowerUpSound(PowerUpType.Shield);
                break;

            case PowerUpType.Dash:
                dashTimer = 5.0f;
                PlayerController.Instance.hasDash = true;
                if (ProceduralAudio.Instance != null) ProceduralAudio.Instance.PlayPowerUpSound(PowerUpType.Dash);
                break;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddScore(150);
        }
    }
}
