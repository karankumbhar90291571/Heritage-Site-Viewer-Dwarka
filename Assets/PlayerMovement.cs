using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float diveDepth = 5f;
    public float diveTime = 3f;

    private Rigidbody rb;
    private bool isDiving = true;

    [Header("Camera Settings")]
    public Camera mainCamera;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // Enable phone gyro for looking around
        if (SystemInfo.supportsGyroscope)
        {
            Input.gyro.enabled = true;
        }

        // Start the diving animation
        StartCoroutine(DiveIn());
    }

    IEnumerator DiveIn()
    {
        Vector3 startPos = transform.position;
        Vector3 targetPos = transform.position + Vector3.down * diveDepth;
        float elapsed = 0;

        while (elapsed < diveTime)
        {
            transform.position = Vector3.Lerp(startPos, targetPos, elapsed / diveTime);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPos;
        isDiving = false; // enable control after diving
    }

    void Update()
    {
        // Handle camera rotation using phone gyroscope
        if (mainCamera != null && SystemInfo.supportsGyroscope)
        {
            Quaternion deviceRotation = Input.gyro.attitude;
            deviceRotation = Quaternion.Euler(90f, 0f, 0f) * (new Quaternion(-deviceRotation.x, -deviceRotation.y, deviceRotation.z, deviceRotation.w));
            mainCamera.transform.localRotation = deviceRotation;
        }

        // Movement with touch input after dive completes
        if (!isDiving && Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Moved)
            {
                Vector2 delta = touch.deltaPosition;
                Vector3 move = new Vector3(delta.x, 0, delta.y) * Time.deltaTime * moveSpeed;

                // Move relative to camera direction
                Vector3 cameraForward = mainCamera.transform.forward;
                cameraForward.y = 0;
                cameraForward.Normalize();

                Vector3 cameraRight = mainCamera.transform.right;
                cameraRight.y = 0;
                cameraRight.Normalize();

                Vector3 moveDir = (cameraRight * delta.x + cameraForward * delta.y).normalized;

                rb.MovePosition(transform.position + moveDir * moveSpeed * Time.deltaTime);
            }
        }
    }
}