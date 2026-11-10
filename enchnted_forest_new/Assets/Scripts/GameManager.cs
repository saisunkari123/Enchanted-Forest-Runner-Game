using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public enum GameState
{
    Title,
    Playing,
    Paused,
    GameOver
}

/// <summary>
/// Sylvan Rush - Central Game Manager (Title Screen Card & Dynamic Progression)
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game State")]
    public GameState currentState = GameState.Title;

    [Header("Speed & Distance")]
    public float baseSpeed = 20.0f; // Comfortable starting speed
    public float maxSpeed = 38.0f;  // Top sprint speed
    public float currentSpeed = 20.0f;
    public float distance = 0f;

    [Header("Score & Orbs")]
    public int score = 0;
    public int orbsCollected = 0;
    public int comboMultiplier = 1;
    public float comboTimer = 0f;

    private int personalBestScore = 0;
    private float personalBestDistance = 0f;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            baseSpeed = 20.0f;
            currentSpeed = baseSpeed;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        personalBestScore = PlayerPrefs.GetInt("SylvanBestScore", 0);
        personalBestDistance = PlayerPrefs.GetFloat("SylvanBestDist", 0f);
        currentState = GameState.Title; // Start on Pre-Game Title Card for screen recording!
        currentSpeed = baseSpeed;
    }

    void Update()
    {
        if (currentState == GameState.Title)
        {
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null)
            {
                var kb = Keyboard.current;
                if (kb.spaceKey.wasPressedThisFrame || kb.enterKey.wasPressedThisFrame)
                {
                    StartNewRun();
                    return;
                }
            }
#endif
            try
            {
                if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
                {
                    StartNewRun();
                    return;
                }
            }
            catch (System.InvalidOperationException)
            {
            }
            return;
        }

        try
        {
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
            {
                if (currentState == GameState.Playing)
                {
                    PauseGame();
                }
                else if (currentState == GameState.Paused)
                {
                    ResumeGame();
                }
            }
        }
        catch (System.InvalidOperationException)
        {
        }

        if (currentState != GameState.Playing)
            return;

        float dt = Time.deltaTime;

        // Dynamic continuous speed progression according to distance covered!
        float speedBonus = distance * 0.022f;
        float targetSpeed = Mathf.Min(maxSpeed, baseSpeed + speedBonus);

        if (PlayerController.Instance != null && PlayerController.Instance.hasDash)
        {
            targetSpeed *= 1.35f;
        }

        currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, 5f * dt);

        float deltaDist = currentSpeed * dt;
        distance += deltaDist;

        // Balanced distance score: ~1 point per 2 meters, multiplied by active combo
        score += Mathf.RoundToInt(deltaDist * 0.45f * comboMultiplier);

        if (comboMultiplier > 1)
        {
            comboTimer -= dt;
            if (comboTimer <= 0f)
            {
                comboMultiplier = 1;
            }
        }
    }

    public void StartNewRun()
    {
        score = 0;
        distance = 0f;
        orbsCollected = 0;
        comboMultiplier = 1;
        comboTimer = 0f;
        baseSpeed = 20.0f;
        currentSpeed = baseSpeed;
        Time.timeScale = 1.0f;

        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.currentLane = 1;
            PlayerController.Instance.hasShield = false;
            PlayerController.Instance.hasDash = false;
            PlayerController.Instance.hasMagnet = false;
            PlayerController.Instance.isSliding = false;
            PlayerController.Instance.transform.position = new Vector3(0f, 0f, 0f);
        }

        if (ObstacleSpawner.Instance != null)
        {
            ObstacleSpawner.Instance.ResetSpawner();
        }

        if (PowerUpManager.Instance != null)
        {
            PowerUpManager.Instance.ResetTimers();
        }

        currentState = GameState.Playing;

        if (ProceduralAudio.Instance != null)
        {
            ProceduralAudio.Instance.StartMusic();
        }
    }

    public void PauseGame()
    {
        currentState = GameState.Paused;
        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        currentState = GameState.Playing;
        Time.timeScale = 1.0f;
    }

    public void AddOrb()
    {
        orbsCollected++;
        comboMultiplier = Mathf.Min(4, comboMultiplier + 1);
        comboTimer = 2.8f;
        score += 10 * comboMultiplier;
    }

    public void AddScore(int points)
    {
        score += points;
    }

    public void TriggerGameOver()
    {
        currentState = GameState.GameOver;

        if (score > personalBestScore)
        {
            personalBestScore = score;
            PlayerPrefs.SetInt("SylvanBestScore", personalBestScore);
        }
        if (distance > personalBestDistance)
        {
            personalBestDistance = distance;
            PlayerPrefs.SetFloat("SylvanBestDist", personalBestDistance);
        }
        PlayerPrefs.Save();

        if (ProceduralAudio.Instance != null)
        {
            ProceduralAudio.Instance.StopMusic();
        }
    }

    public int GetPersonalBestScore() => personalBestScore;
    public float GetPersonalBestDistance() => personalBestDistance;
}
