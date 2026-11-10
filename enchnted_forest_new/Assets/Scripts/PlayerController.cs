using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Sylvan Rush - Player Controller (Correct Non-Inverted Lane Movement & Jump/Slide)
/// </summary>
public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }

    [Header("Lane Settings")]
    public float laneWidth = 3.0f; // 3.0m wide lanes
    public float laneChangeSpeed = 22f; // Fast, responsive lane shifting
    public float tiltAmount = 14f;

    [Header("Jump & Gravity")]
    public float jumpVelocity = 15.5f; // Jump apex reaches 3.33m to cleanly clear the rock with zero clipping!
    public float gravity = -36.0f;
    public float jumpBufferTime = 0.15f;
    public float coyoteTime = 0.1f;

    [Header("Slide / Duck Settings")]
    public float slideDuration = 0.7f;
    public bool isSliding = false;
    private float slideTimer = 0f;

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

    private CapsuleCollider capsuleCol;

    public Transform visualRoot;
    public Transform shieldBubble;

    void Awake()
    {
        Instance = this;
        capsuleCol = GetComponent<CapsuleCollider>();
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
        // 1. Support New Input System Package (Default in Unity 6)
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null)
        {
            var kb = Keyboard.current;
            // Left Movement: A key or Left Arrow -> Moves Left
            if (kb.aKey.wasPressedThisFrame || kb.leftArrowKey.wasPressedThisFrame)
            {
                MoveLane(-1);
            }
            // Right Movement: D key or Right Arrow -> Moves Right
            if (kb.dKey.wasPressedThisFrame || kb.rightArrowKey.wasPressedThisFrame)
            {
                MoveLane(1);
            }
            // Jump: W, Up Arrow, or Space
            if (kb.wKey.wasPressedThisFrame || kb.upArrowKey.wasPressedThisFrame || kb.spaceKey.wasPressedThisFrame)
            {
                RequestJump();
            }
            // Slide: S or Down Arrow
            if (kb.sKey.wasPressedThisFrame || kb.downArrowKey.wasPressedThisFrame)
            {
                RequestSlide();
            }
        }
#endif

        // 2. Support Legacy Input Manager
        try
        {
            if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow)) MoveLane(-1);
            if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)) MoveLane(1);
            if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.Space)) RequestJump();
            if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) RequestSlide();
        }
        catch (System.InvalidOperationException)
        {
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

    public void RequestJump()
    {
        if (isSliding)
        {
            EndSlide();
        }
        jumpBufferTimer = jumpBufferTime;
    }

    public void RequestSlide()
    {
        if (!isGrounded)
        {
            verticalVelocity = -24f; // Fast drop down
            return;
        }

        if (!isSliding)
        {
            isSliding = true;
            slideTimer = slideDuration;

            if (capsuleCol != null)
            {
                capsuleCol.center = new Vector3(0f, 0.35f, 0f);
                capsuleCol.height = 0.7f;
            }

            if (ProceduralAudio.Instance != null)
            {
                ProceduralAudio.Instance.PlaySlide();
            }
        }
    }

    void EndSlide()
    {
        isSliding = false;
        slideTimer = 0f;

        if (capsuleCol != null)
        {
            capsuleCol.center = new Vector3(0f, 1.0f, 0f);
            capsuleCol.height = 1.9f;
        }
    }

    void HandlePhysics()
    {
        float dt = Time.deltaTime;

        if (isSliding)
        {
            slideTimer -= dt;
            if (slideTimer <= 0f)
            {
                EndSlide();
            }
        }

        // Move towards target lane position
        float targetX = GetTargetXForLane(currentLane);
        currentX = Mathf.MoveTowards(currentX, targetX, laneChangeSpeed * dt);

        // Banking tilt when moving across lanes
        float deltaX = targetX - currentX;
        float targetTilt = Mathf.Clamp(deltaX * tiltAmount, -20f, 20f);
        currentZRotation = Mathf.Lerp(currentZRotation, targetTilt, 14f * dt);

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
            if (isSliding)
            {
                visualRoot.localPosition = new Vector3(0f, -0.3f, 0f);
                visualRoot.localRotation = Quaternion.Euler(25f, 180f, currentZRotation);
            }
            else
            {
                visualRoot.localPosition = Vector3.zero;
                visualRoot.localRotation = Quaternion.Euler(0f, 180f, currentZRotation);
            }
        }

        if (shieldBubble != null)
        {
            shieldBubble.gameObject.SetActive(hasShield);
            if (hasShield)
            {
                shieldBubble.Rotate(Vector3.up * (Time.deltaTime * 240f), Space.World);
            }
        }
    }

    /// <summary>
    /// When looking down the -Z axis from camera at +Z:
    /// - Screen LEFT is world +X
    /// - Screen RIGHT is world -X
    /// </summary>
    public float GetTargetXForLane(int lane)
    {
        switch (lane)
        {
            case 0: return laneWidth;    // Left Lane (Screen LEFT)
            case 1: return 0f;           // Center Lane
            case 2: return -laneWidth;   // Right Lane (Screen RIGHT)
            default: return 0f;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        Obstacle obstacle = other.GetComponentInParent<Obstacle>();
        if (obstacle != null)
        {
            HandleObstacleCollision(obstacle);
            return;
        }

        ManaOrb orb = other.GetComponentInParent<ManaOrb>();
        if (orb != null)
        {
            CollectOrb(orb);
            return;
        }

        PowerUpItem powerUp = other.GetComponentInParent<PowerUpItem>();
        if (powerUp != null)
        {
            CollectPowerUp(powerUp);
            return;
        }
    }

    void HandleObstacleCollision(Obstacle obs)
    {
        // Jump cleanly over both the wood stump and the stone monolith!
        if (obs.obstacleType == ObstacleType.Log || obs.obstacleType == ObstacleType.Monolith)
        {
            if (transform.position.y >= 0.55f) return; // Leaped cleanly over!
        }
        else if (obs.obstacleType == ObstacleType.OverheadBranch)
        {
            if (isSliding) return; // Slid cleanly under!
        }

        if (hasDash)
        {
            if (CameraFollowShake.Instance != null) CameraFollowShake.Instance.TriggerShake(0.25f, 0.25f);
            if (ProceduralAudio.Instance != null) ProceduralAudio.Instance.PlayShieldShatter();
            if (GameManager.Instance != null) GameManager.Instance.AddScore(50);
            obs.gameObject.SetActive(false);
            return;
        }

        if (hasShield)
        {
            hasShield = false;
            if (CameraFollowShake.Instance != null) CameraFollowShake.Instance.TriggerShake(0.38f, 0.38f);
            if (ProceduralAudio.Instance != null) ProceduralAudio.Instance.PlayShieldShatter();
            if (GameManager.Instance != null) GameManager.Instance.AddScore(50);
            obs.gameObject.SetActive(false);
            return;
        }

        // Fatal collision
        if (CameraFollowShake.Instance != null) CameraFollowShake.Instance.TriggerShake(0.52f, 0.45f);
        if (ProceduralAudio.Instance != null) ProceduralAudio.Instance.PlayGameOverSound();
        if (GameManager.Instance != null) GameManager.Instance.TriggerGameOver();
    }

    void CollectOrb(ManaOrb orb)
    {
        orb.gameObject.SetActive(false);
        if (GameManager.Instance != null) GameManager.Instance.AddOrb();
        if (ProceduralAudio.Instance != null) ProceduralAudio.Instance.PlayOrbPickup();
    }

    void CollectPowerUp(PowerUpItem item)
    {
        PowerUpType type = item.powerUpType;
        item.gameObject.SetActive(false);
        if (PowerUpManager.Instance != null) PowerUpManager.Instance.ActivatePowerUp(type);
    }
}
