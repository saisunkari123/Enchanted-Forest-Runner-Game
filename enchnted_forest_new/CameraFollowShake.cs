using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Sylvan Rush - Camera Follow & Screen Shake Controller
/// Follows the player avatar and applies multi-axis harmonic screen-shake on impact.
/// </summary>
public class CameraFollowShake : MonoBehaviour
{
    public static CameraFollowShake Instance { get; private set; }

    [Header("Camera Positioning")]
    public Transform target;
    public Vector3 offset = new Vector3(0f, 3.65f, 6.2f);
    public Vector3 lookAtOffset = new Vector3(0f, 1.35f, -9.5f);
    public float followSpeed = 8f;

    [Header("Screen Shake Parameters")]
    private float shakeIntensity = 0f;
    private float shakeTimer = 0f;
    private float shakeDuration = 0.35f;

    void Awake()
    {
        Instance = this;
    }

    void LateUpdate()
    {
        Vector3 targetPos = offset;
        Vector3 lookTarget = lookAtOffset;

        if (target != null)
        {
            targetPos.x = target.position.x * 0.62f;
            targetPos.y = offset.y + target.position.y * 0.32f;
            lookTarget.x = target.position.x * 0.38f;
            lookTarget.y = lookAtOffset.y + target.position.y * 0.2f;
        }

        // Smooth base camera follow
        Vector3 currentPos = transform.position;
        currentPos = Vector3.Lerp(currentPos, targetPos, followSpeed * Time.deltaTime);

        // Apply screen-shake offsets
        if (shakeTimer > 0f)
        {
            shakeTimer -= Time.deltaTime;
            float progress = shakeTimer / Mathf.Max(0.01f, shakeDuration);
            float envelope = progress * progress; // Quadratic decay
            float amp = shakeIntensity * envelope;

            float shakeX = (Mathf.Sin(Time.time * 58f) * 0.65f + Random.Range(-0.35f, 0.35f)) * amp;
            float shakeY = (Mathf.Cos(Time.time * 51f) * 0.65f + Random.Range(-0.35f, 0.35f)) * amp * 0.85f;
            float shakeZ = Mathf.Sin(Time.time * 43f) * amp * 0.4f;

            currentPos += new Vector3(shakeX, shakeY, shakeZ);

            transform.position = currentPos;
            transform.LookAt(lookTarget);

            // Subtle camera roll on Z
            transform.Rotate(0f, 0f, Mathf.Sin(Time.time * 64f) * amp * 5f, Space.Self);
        }
        else
        {
            transform.position = currentPos;
            transform.LookAt(lookTarget);
        }
    }

    public void TriggerShake(float intensity, float duration = 0.34f)
    {
        shakeIntensity = Mathf.Max(shakeIntensity, intensity);
        shakeDuration = Mathf.Max(0.05f, duration);
        shakeTimer = Mathf.Max(shakeTimer, duration);
    }
}
