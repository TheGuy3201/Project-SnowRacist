using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class SnowboardControlV2 : MonoBehaviour
{
    [Header("Steering")]
    [SerializeField] private float fixedMaxAngle = 45f;
    [SerializeField] private float rotationSpeedDegPerSec = 120f;
    [SerializeField] private float baseDrag = 0f;      // drag when straight
    [SerializeField] private float maxDragWhenAngled = 3f; // drag at fixedMaxAngle (tweak)
    [SerializeField] private float returnToCenterDegPerSec = 40f;
    
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

        // Yaw control (relative to start, clamped) + RETURN TO CENTER when no input
        if (Mathf.Abs(steer) > 0.01f)
        {
            yawOffsetDeg += steer * rotationSpeedDegPerSec * Time.fixedDeltaTime;
        }
        else
        {
            yawOffsetDeg = Mathf.MoveTowards(yawOffsetDeg, 0f, returnToCenterDegPerSec * Time.fixedDeltaTime);
        }

        yawOffsetDeg = Mathf.Clamp(yawOffsetDeg, -fixedMaxAngle, fixedMaxAngle);

        // Preserve current physics tilt (X/Z), only override yaw (Y)
        Vector3 currentEuler = rb.rotation.eulerAngles;
        float targetYaw = startYawDeg + yawOffsetDeg;
        rb.MoveRotation(Quaternion.Euler(currentEuler.x, targetYaw, currentEuler.z));

        ApplySideMovementFromYaw();
    }
    
    private void ApplySideMovementFromYaw()
    {
        float currentYaw = rb.rotation.eulerAngles.y;
        float yawDelta = Mathf.DeltaAngle(startYawDeg, currentYaw);

        float multiplier = Mathf.InverseLerp(0f, fixedMaxAngle, Mathf.Abs(yawDelta));

        float dir = -Mathf.Sign(yawDelta);
        float targetZVelocity = dir * sideMovementSpeed * multiplier;

        // 1) Drag increases with angle (slows overall motion, including downhill)
        rb.linearDamping = Mathf.Lerp(baseDrag, maxDragWhenAngled, multiplier)/2;

        // 2) But we force Z back to the desired side speed so drag won’t kill it
        Vector3 v = rb.linearVelocity;
        v.z = targetZVelocity;
        rb.linearVelocity = v;
    }
}