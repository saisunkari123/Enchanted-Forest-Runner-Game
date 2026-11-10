using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Sylvan Rush - Player Controller
/// Handles 3-lane kinematic movement, articulated jumping, coyote time, jump buffering, and banking tilt.
/// </summary>
public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }

    [Header("Lane Settings")]
    public float laneWidth = 2.65f;
    public float laneChangeSpeed = 16f;
    public float tiltAmount = 14f;

    [Header("Jump & Gravity")]
    public float jumpVelocity = 14.6f;
    public float gravity = -35.0f;
    public float jumpBufferTime = 0.15f;
    public float coyoteTime = 0.095f;

    [Header("State")]
    public int currentLane = 1; // 0 = Left, 1 = Center, 2 = Right
    public bool isGrounded = true;
    public bool hasShield = false;
    public bool hasDash = false;
    public bool hasMagnet = false;

    private float verticalVelocity = 0f;
    private float jumpBufferTimer = 0f;
    private float coyoteTimer = 0f;
    private float currentX = 0f;
    private float currentZRotation = 0f;

    // Visual references (built procedurally or assigned)
    public Transform visualRoot;
    public Transform shieldBubble;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        currentX = GetTargetXForLane(currentLane);
        transform.position = new Vector3(currentX, 0f, 0f);
    }

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.currentState != GameState.Playing)
            return;

        HandleInput();
        HandlePhysics();
        HandleVisuals();
    }

    void HandleInput()
    {
        // Lane Movement (A/D or Left/Right Arrow or Swipe)
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            MoveLane(-1);
        }
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            MoveLane(1);
        }

        // Jump (W, UpArrow, Space)
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.Space))
        {
            jumpBufferTimer = jumpBufferTime;
        }

        // Mobile touch swipe handling
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                // Touch start registered
            }
            else if (touch.phase == TouchPhase.Ended)
            {
                Vector2 delta = touch.deltaPosition;
                if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y) && Mathf.Abs(delta.x) > 30f)
                {
                    MoveLane(delta.x > 0 ? 1 : -1);
                }
                else if (delta.y > 30f)
                {
                    jumpBufferTimer = jumpBufferTime;
                }
            }
        }
    }

    public void MoveLane(int direction)
    {
        int nextLane = Mathf.Clamp(currentLane + direction, 0, 2);
        if (nextLane != currentLane)
        {
            currentLane = nextLane;
            if (ProceduralAudio.Instance != null)
            {
                ProceduralAudio.Instance.PlayLaneSwitch(direction);
            }
        }
    }

    void HandlePhysics()
    {
        float dt = Time.deltaTime;

        // X-axis smooth lane interpolation
        float targetX = GetTargetXForLane(currentLane);
        currentX = Mathf.Lerp(currentX, targetX, laneChangeSpeed * dt);

        // Z-axis tilt into turns
        float targetTilt = -(currentX - targetX) * tiltAmount;
        currentZRotation = Mathf.Lerp(currentZRotation, targetTilt, 12f * dt);

        // Jump timers
        if (jumpBufferTimer > 0) jumpBufferTimer -= dt;
        if (isGrounded) coyoteTimer = coyoteTime;
        else if (coyoteTimer > 0) coyoteTimer -= dt;

        // Execute jump
        if (jumpBufferTimer > 0 && (isGrounded || coyoteTimer > 0))
        {
            verticalVelocity = jumpVelocity;
            isGrounded = false;
            jumpBufferTimer = 0f;
            coyoteTimer = 0f;

            if (ProceduralAudio.Instance != null)
            {
                ProceduralAudio.Instance.PlayJump();
            }
        }

        // Apply Gravity
        Vector3 pos = transform.position;
        pos.x = currentX;

        if (!isGrounded)
        {
            pos.y += verticalVelocity * dt;
            verticalVelocity += gravity * dt;

            if (pos.y <= 0f)
            {
                pos.y = 0f;
                verticalVelocity = 0f;
                isGrounded = true;
            }
        }

        transform.position = pos;
    }

    void HandleVisuals()
    {
        if (visualRoot != null)
        {
            // Running bounce when grounded
            float bounce = isGrounded ? Mathf.Abs(Mathf.Sin(Time.time * 16f)) * 0.12f : 0f;
            visualRoot.localPosition = new Vector3(0f, bounce, 0f);
            visualRoot.localRotation = Quaternion.Euler(0f, 0f, currentZRotation);
        }

        if (shieldBubble != null)
        {
            shieldBubble.gameObject.SetActive(hasShield);
            if (hasShield)
            {
                shieldBubble.Rotate(Vector3.up * (Time.deltaTime * 90f));
            }
        }
    }

    public float GetTargetXForLane(int lane)
    {
        switch (lane)
        {
            case 0: return -laneWidth;
            case 1: return 0f;
            case 2: return laneWidth;
            default: return 0f;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Obstacle"))
        {
            HandleObstacleCollision(other.gameObject);
        }
        else if (other.CompareTag("Orb"))
        {
            CollectOrb(other.gameObject);
        }
        else if (other.CompareTag("PowerUp"))
        {
            CollectPowerUp(other.gameObject);
        }
    }

    void HandleObstacleCollision(GameObject obstacle)
    {
        Obstacle obs = obstacle.GetComponent<Obstacle>();
        float obsHeight = obs != null ? obs.obstacleHeight : 1.2f;

        // Check if player vaulted over cleanly
        if (transform.position.y > obsHeight)
        {
            return; // Successfully vaulted over jumpable fallen log!
        }

        if (hasDash)
        {
            // Invulnerable - smash obstacle!
            if (CameraFollowShake.Instance != null) CameraFollowShake.Instance.TriggerShake(0.24f, 0.28f);
            if (ProceduralAudio.Instance != null) ProceduralAudio.Instance.PlayShieldShatter();
            if (GameManager.Instance != null) GameManager.Instance.AddScore(150);
            obstacle.SetActive(false);
        }
        else if (hasShield)
        {
            // Shield absorbs fatal impact
            hasShield = false;
            if (CameraFollowShake.Instance != null) CameraFollowShake.Instance.TriggerShake(0.36f, 0.36f);
            if (ProceduralAudio.Instance != null) ProceduralAudio.Instance.PlayShieldShatter();
            if (GameManager.Instance != null) GameManager.Instance.AddScore(250);
            obstacle.SetActive(false);
        }
        else
        {
            // Fatal impact
            if (CameraFollowShake.Instance != null) CameraFollowShake.Instance.TriggerShake(0.48f, 0.42f);
            if (ProceduralAudio.Instance != null) ProceduralAudio.Instance.PlayGameOverSound();
            if (GameManager.Instance != null) GameManager.Instance.TriggerGameOver();
        }
    }

    void CollectOrb(GameObject orb)
    {
        orb.SetActive(false);
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddOrb();
        }
        if (ProceduralAudio.Instance != null)
        {
            ProceduralAudio.Instance.PlayOrbPickup();
        }
    }

    void CollectPowerUp(GameObject powerUpObj)
    {
        PowerUpItem item = powerUpObj.GetComponent<PowerUpItem>();
        PowerUpType type = item != null ? item.powerUpType : PowerUpType.Shield;
        powerUpObj.SetActive(false);

        if (PowerUpManager.Instance != null)
        {
            PowerUpManager.Instance.ActivatePowerUp(type);
        }
    }
}
