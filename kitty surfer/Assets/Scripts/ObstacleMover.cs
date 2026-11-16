using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ObstacleMover : MonoBehaviour
{
    [SerializeField] private float travelDistance = 3.075f;
    [SerializeField] private float slideSpeed = 2f;

    private Rigidbody body;
    private Transform trackSection;
    private Vector3 startLocalPosition;
    private float elapsed;

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
        trackSection = transform.parent;
        startLocalPosition = transform.localPosition;
    }

    private void FixedUpdate()
    {
        elapsed += Time.fixedDeltaTime;
        Vector3 target = startLocalPosition;
        target.x += Mathf.PingPong(elapsed * slideSpeed, travelDistance);
        body.MovePosition(trackSection.TransformPoint(target));
    }
}
