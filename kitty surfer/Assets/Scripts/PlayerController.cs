using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float forwardSpeed = 5f;
    [SerializeField] private float laneSpacing = 3.075f;
    [SerializeField] private float laneChangeSpeed = 10f;
    [SerializeField] private float jumpSpeed = 5.5f;
    [SerializeField] private LayerMask groundLayers;
    [SerializeField] private Transform followCamera;

    private Rigidbody body;
    private float centerLaneX;
    private int lane;
    private bool jumpPressed;

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
        centerLaneX = transform.position.x;
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

    private void FixedUpdate()
    {
        Vector3 velocity = body.linearVelocity;
        float targetX = centerLaneX + lane * laneSpacing;
        float speedToTarget = (targetX - body.position.x) / Time.fixedDeltaTime;
        velocity.x = Mathf.Clamp(speedToTarget, -laneChangeSpeed, laneChangeSpeed);
        velocity.z = forwardSpeed;

        if (jumpPressed && velocity.y <= 0f &&
            Physics.Raycast(body.position + Vector3.up * 0.15f, Vector3.down,
                0.21f, groundLayers, QueryTriggerInteraction.Ignore))
        {
            velocity.y = jumpSpeed;
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