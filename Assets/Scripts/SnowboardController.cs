using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class SnowboardController : MonoBehaviour
{
    float playerSpeed = 1.0f;
    [SerializeField] Rigidbody rb;
    [SerializeField] float targetXRotation = 10f;
    [SerializeField] float joystickDeadZone = 0.1f;
    [SerializeField] float keyboardLookSpeed = 90f;
    [SerializeField] bool invertMoveDirection = false;
    InputDevice leftController;

    void Start()
    {
        if (rb == null)
        {
            rb = GetComponent<Rigidbody>();
        }

        leftController = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
    }

    // Update is called once per frame
    void Update()
    {
        ApplyKeyboardLook();

        bool isJoystickLeft = false;
        bool isAKeyPressed = Input.GetKey(KeyCode.A);
        bool isJoystickRight = false;
        bool isDKeyPressed = Input.GetKey(KeyCode.D);
        bool isJoystickUp = false;
        bool isWKeyPressed = Input.GetKey(KeyCode.W);
        bool isJoystickDown = false;
        bool isSKeyPressed = Input.GetKey(KeyCode.S);


        if (!leftController.isValid && InputDevices.GetDeviceAtXRNode(XRNode.LeftHand).isValid)
        {
            leftController = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
        }

        if (leftController.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 joystickInput))
        {
            isJoystickLeft = joystickInput.x < -joystickDeadZone;
            isJoystickRight = joystickInput.x > joystickDeadZone;
            isJoystickUp = joystickInput.y > joystickDeadZone;
            isJoystickDown = joystickInput.y < -joystickDeadZone;
        }

        if (isJoystickLeft || isAKeyPressed)
        {
            Vector3 euler = transform.eulerAngles;
            transform.eulerAngles = new Vector3(targetXRotation, euler.y, euler.z);
        }

        if(isJoystickRight || isDKeyPressed)
        {
            Vector3 euler = transform.eulerAngles;
            transform.eulerAngles = new Vector3(-targetXRotation, euler.y, euler.z);
        }

        if(isJoystickUp || Input.GetKey(KeyCode.W))
        {
            playerSpeed += 1f * Time.deltaTime; // Increase speed over time

            //Rotate y axis up to -10 degrees
            //Vector3 euler = transform.eulerAngles;
            //transform.eulerAngles = new Vector3(euler.x, euler.y, 0);
        }

        if(isJoystickDown || Input.GetKey(KeyCode.S))
        {
            playerSpeed -= 0.1f * Time.deltaTime; // Decrease speed over time
            if(playerSpeed < 0.1f)
            {
                playerSpeed = 0.1f;
            }
            // Rotate the snowboard to be horizontal and at a 10 degree angle to the ground
            Vector3 euler = transform.eulerAngles;
            transform.eulerAngles = new Vector3(0, euler.y, euler.z);
        }
    }

    void ApplyKeyboardLook()
    {
        float lookInput = 0f;

        if (Input.GetKey(KeyCode.LeftArrow))
        {
            lookInput = -1f;
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            lookInput = 1f;
        }

        if (Mathf.Abs(lookInput) > 0f)
        {
            transform.Rotate(0f, lookInput * keyboardLookSpeed * Time.deltaTime, 0f, Space.World);
        }
    }

    void FixedUpdate()
    {
        ApplyForwardMovement();
    }

    void ApplyForwardMovement()
    {
        if (rb == null)
        {
            return;
        }

        Vector3 modelForwardFlat = Vector3.ProjectOnPlane(transform.right, Vector3.up).normalized;
        if (invertMoveDirection)
        {
            modelForwardFlat = -modelForwardFlat;
        }

        Vector3 currentVelocity = rb.linearVelocity;
        rb.linearVelocity = new Vector3(
            modelForwardFlat.x * playerSpeed,
            currentVelocity.y,
            modelForwardFlat.z * playerSpeed
        );
    }
}
