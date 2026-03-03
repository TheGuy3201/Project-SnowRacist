using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class SnowboardControlV2 : MonoBehaviour
{
    [Header("Steering")]
    [SerializeField] private float fixedMaxAngle = 45f;
    [SerializeField] private float rotationSpeedDegPerSec = 120f;

    [Header("Side Movement")]
    [SerializeField] private float sideMovementSpeed = 2f; // <-- requested (private serialized)

    [Header("Center of Mass Markers (children of the snowboard)")]
    [SerializeField] private Transform centerOfMassLeft;
    [SerializeField] private Transform centerOfMassRight;
    [SerializeField] private bool shiftCenterOfMassWithInput = false;

    private Rigidbody rb;
    private InputAction steerAction;

    private float startYawDeg;
    private float yawOffsetDeg;

    private Vector3 originalCenterOfMass;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        steerAction = new InputAction("Steer", InputActionType.Value, expectedControlType: "Axis");
        steerAction.AddCompositeBinding("1DAxis")
            .With("Negative", "<Keyboard>/a")
            .With("Positive", "<Keyboard>/d");
    }

    private void OnEnable() => steerAction.Enable();
    private void OnDisable() => steerAction.Disable();

    private void Start()
    {
        startYawDeg = rb.rotation.eulerAngles.y;
        yawOffsetDeg = 0f;

        originalCenterOfMass = rb.centerOfMass;
    }

    private void FixedUpdate()
    {
        float steer = steerAction.ReadValue<float>(); // -1..+1

        // Center of mass shifting (optional)
        if (shiftCenterOfMassWithInput && centerOfMassLeft && centerOfMassRight)
        {
            if (steer < -0.01f) rb.centerOfMass = centerOfMassLeft.localPosition;
            else if (steer > 0.01f) rb.centerOfMass = centerOfMassRight.localPosition;
            else rb.centerOfMass = originalCenterOfMass;
        }

        // Yaw control (relative to start, clamped)
        yawOffsetDeg += steer * rotationSpeedDegPerSec * Time.fixedDeltaTime;
        yawOffsetDeg = Mathf.Clamp(yawOffsetDeg, -fixedMaxAngle, fixedMaxAngle);

        // Preserve current physics tilt (X/Z), only override yaw (Y)
        Vector3 currentEuler = rb.rotation.eulerAngles;
        float targetYaw = startYawDeg + yawOffsetDeg;
        rb.MoveRotation(Quaternion.Euler(currentEuler.x, targetYaw, currentEuler.z));

        // NEW: add sideways motion on WORLD Z using yaw angle
        ApplySideMovementFromYaw();
    }

    /// <summary>
    /// Uses current yaw (relative to start) to push the board sideways on WORLD Z.
    /// 0° => multiplier 0 (no side movement)
    /// ±fixedMaxAngle => multiplier 1 (full side movement)
    /// Left moves -Z, Right moves +Z.
    /// </summary>
    private void ApplySideMovementFromYaw()
    {
        // Signed yaw difference from start (in degrees)
        float currentYaw = rb.rotation.eulerAngles.y;
        float yawDelta = Mathf.DeltaAngle(startYawDeg, currentYaw); // [-180, 180], left negative, right positive

        // Convert angle magnitude to 0..1 multiplier
        float multiplier = Mathf.InverseLerp(0f, fixedMaxAngle, Mathf.Abs(yawDelta));

        // Direction: left = -1, right = +1
        float dir = -Mathf.Sign(yawDelta);

        // Target sideways velocity on world Z
        float targetZVelocity = dir * sideMovementSpeed * multiplier;

        // Apply ONLY the Z component; keep X/Y from physics (gravity/slope)
        Vector3 v = rb.linearVelocity;
        v.z = targetZVelocity;
        rb.linearVelocity = v;
    }
}