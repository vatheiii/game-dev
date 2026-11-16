using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float forwardSpeed = 5f;
    [SerializeField] private float laneSpacing = 3.075f;
    [SerializeField] private float laneChangeSpeed = 10f;
    [SerializeField] private float jumpSpeed = 5.5f;
    [SerializeField] private float fallGravityMultiplier = 1f;
    [SerializeField] private LayerMask groundLayers;
    [SerializeField] private Transform followCamera;

    private const float FullSpeedDistance = 1350f;
    private const float MaxSpeed = 12f;

    private Rigidbody body;
    private float centerLaneX;
    private float startZ;
    private int lane;
    private bool jumpPressed;

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
        centerLaneX = transform.position.x;
        startZ = body.position.z;
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
            return;

        if (keyboard.aKey.wasPressedThisFrame || keyboard.leftArrowKey.wasPressedThisFrame)
            lane = Mathf.Max(lane - 1, -1);
        else if (keyboard.dKey.wasPressedThisFrame || keyboard.rightArrowKey.wasPressedThisFrame)
            lane = Mathf.Min(lane + 1, 1);

        if (keyboard.spaceKey.wasPressedThisFrame)
            jumpPressed = true;
    }

    internal float GetLaneX(int lane) => centerLaneX + lane * laneSpacing;

    private void FixedUpdate()
    {
        Vector3 velocity = body.linearVelocity;
        float targetX = GetLaneX(lane);
        float speedToTarget = (targetX - body.position.x) / Time.fixedDeltaTime;
        velocity.x = Mathf.Clamp(speedToTarget, -laneChangeSpeed, laneChangeSpeed);

        float distanceTravelled = body.position.z - startZ;
        float speedProgress = Mathf.Clamp01(distanceTravelled / FullSpeedDistance);
        // Interpolating squared speeds gives steady acceleration over time.
        float speedSquared = Mathf.Lerp(forwardSpeed * forwardSpeed, MaxSpeed * MaxSpeed, speedProgress);
        velocity.z = Mathf.Sqrt(speedSquared);

        if (jumpPressed && velocity.y <= 0f &&
            Physics.Raycast(body.position + Vector3.up * 0.15f, Vector3.down,
                0.21f, groundLayers, QueryTriggerInteraction.Ignore))
        {
            velocity.y = jumpSpeed;
        }
        else if (velocity.y < 0f)
        {
            // The Rigidbody already applies normal gravity.
            velocity.y += Physics.gravity.y * (fallGravityMultiplier - 1f) * Time.fixedDeltaTime;
        }

        jumpPressed = false;
        body.linearVelocity = velocity;
    }

    private void LateUpdate()
    {
        if (followCamera == null)
            return;

        Vector3 cameraPosition = followCamera.position;
        cameraPosition.z = transform.position.z - 8f;
        followCamera.position = cameraPosition;
    }
}
