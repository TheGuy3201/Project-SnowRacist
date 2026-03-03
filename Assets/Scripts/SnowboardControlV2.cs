using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class SnowboardControlV2 : MonoBehaviour
{
    [Header("Steering")]
    [SerializeField] private float fixedMaxAngle = 45f;
    [SerializeField] private float rotationSpeedDegPerSec = 120f;

    [Header("Center of Mass Markers (children of the snowboard)")]
    [SerializeField] private Transform centerOfMassLeft;
    [SerializeField] private Transform centerOfMassRight;
    [SerializeField] private bool shiftCenterOfMassWithInput = false;

    private Rigidbody rb;
    private InputAction steerAction;

    private float startYawDeg;
    private float yawOffsetDeg;

    // NEW: original center of mass
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

        // Cache the real original center of mass
        originalCenterOfMass = rb.centerOfMass;
    }

    private void FixedUpdate()
    {
        float steer = steerAction.ReadValue<float>();

        if (shiftCenterOfMassWithInput && centerOfMassLeft && centerOfMassRight)
        {
            if (steer < -0.01f)
                rb.centerOfMass = centerOfMassLeft.localPosition;
            else if (steer > 0.01f)
                rb.centerOfMass = centerOfMassRight.localPosition;
            else
                rb.centerOfMass = originalCenterOfMass; // ← restore true original
        }

        yawOffsetDeg += steer * rotationSpeedDegPerSec * Time.fixedDeltaTime;
        yawOffsetDeg = Mathf.Clamp(yawOffsetDeg, -fixedMaxAngle, fixedMaxAngle);

        Vector3 currentEuler = rb.rotation.eulerAngles;
        float targetYaw = startYawDeg + yawOffsetDeg;

        Quaternion targetRotation = Quaternion.Euler(currentEuler.x, targetYaw, currentEuler.z);
        rb.MoveRotation(targetRotation);
    }
}