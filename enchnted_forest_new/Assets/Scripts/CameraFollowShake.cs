using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Sylvan Rush - Camera Follow & Screen Shake Controller (Temple Run / Subway Surfers Centered Camera)
/// Keeps camera firmly centered on track X=0 so player lane shifts are clearly visible!
/// </summary>
public class CameraFollowShake : MonoBehaviour
{
    public static CameraFollowShake Instance { get; private set; }

    [Header("Camera Positioning")]
    public Transform target;
    public Vector3 offset = new Vector3(0f, 3.2f, 5.6f);
    public Vector3 lookAtOffset = new Vector3(0f, 1.35f, -14f);
    public float followSpeed = 10f;

    [Header("Dynamic FOV")]
    public float baseFOV = 54f;
    public float dashFOV = 66f;
    private Camera cam;

    [Header("Screen Shake Parameters")]
    private float shakeIntensity = 0f;
    private float shakeTimer = 0f;
    private float shakeDuration = 0.35f;

    void Awake()
    {
        Instance = this;
        cam = GetComponent<Camera>();
        if (cam != null) cam.fieldOfView = baseFOV;
    }

    void LateUpdate()
    {
        // Keep camera centered at X = 0 so the player visibly jumps between lanes!
        Vector3 targetPos = new Vector3(0f, offset.y, offset.z);
        Vector3 lookTarget = new Vector3(0f, lookAtOffset.y, lookAtOffset.z);

        if (target != null)
        {
            // Track player jump height gently
            targetPos.y = offset.y + target.position.y * 0.25f;
            lookTarget.y = lookAtOffset.y + target.position.y * 0.15f;
        }

        Vector3 currentPos = transform.position;
        currentPos = Vector3.Lerp(currentPos, targetPos, followSpeed * Time.deltaTime);

        if (cam != null && PlayerController.Instance != null)
        {
            float targetFOV = PlayerController.Instance.hasDash ? dashFOV : baseFOV;
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFOV, 6f * Time.deltaTime);
        }

        if (shakeTimer > 0f)
        {
            shakeTimer -= Time.deltaTime;
            float progress = shakeTimer / Mathf.Max(0.01f, shakeDuration);
            float envelope = progress * progress;
            float amp = shakeIntensity * envelope;

            float shakeX = (Mathf.Sin(Time.time * 58f) * 0.65f + Random.Range(-0.35f, 0.35f)) * amp;
            float shakeY = (Mathf.Cos(Time.time * 51f) * 0.65f + Random.Range(-0.35f, 0.35f)) * amp * 0.85f;
            float shakeZ = Mathf.Sin(Time.time * 43f) * amp * 0.4f;

            currentPos += new Vector3(shakeX, shakeY, shakeZ);
        }

        transform.position = currentPos;
        transform.LookAt(lookTarget);
    }

    public void TriggerShake(float intensity, float duration = 0.34f)
    {
        shakeIntensity = Mathf.Max(shakeIntensity, intensity);
        shakeDuration = Mathf.Max(0.05f, duration);
        shakeTimer = Mathf.Max(shakeTimer, duration);
    }
}
