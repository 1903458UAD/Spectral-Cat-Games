using System.Collections.Generic;
using UnityEngine;

public class FirstPersonMovement : MonoBehaviour
{
    public float speed = 5;

    [Header("Running")]
    public bool canRun = true;
    public bool IsRunning { get; private set; }
    public float runSpeed = 9;
    public KeyCode runningKey = KeyCode.LeftShift;

    Rigidbody rigidbody;
    /// <summary> Functions to override movement speed. Will use the last added override. </summary>
    public List<System.Func<float>> speedOverrides = new List<System.Func<float>>();

    private string horizontalInput = "";
    private string verticalInput = "";
    
    private enum InputType { Controller, Keyboard};
    private InputType currentInput;

    void Awake()
    {
        // Get the rigidbody on this.
        rigidbody = GetComponent<Rigidbody>();

        currentInput = InputType.Keyboard;
    }

    InputType DetectInput()
    {
        string[] joysticks = Input.GetJoystickNames();

        // Check if any joystick has a valid (non-empty) name
        foreach (string joystick in joysticks)
        {
            if (!string.IsNullOrEmpty(joystick))
            {
                return InputType.Controller;
            }
        }

        return InputType.Keyboard;
    }

    void Update()
    {
        currentInput = DetectInput(); // Check input type in Update instead of FixedUpdate

        if (currentInput == InputType.Controller)
        {
            horizontalInput = "Horizontal Joystick";
            verticalInput = "Vertical Joystick";
            runningKey = KeyCode.Joystick1Button7;
        }

        else
        {
            horizontalInput = "Horizontal";
            verticalInput = "Vertical";
            runningKey = KeyCode.LeftShift;
        }

        // Update IsRunning from input.
    }


    void FixedUpdate()
    {
        IsRunning = canRun && Input.GetKey(runningKey);

        // Get targetMovingSpeed.
        float targetMovingSpeed = IsRunning ? runSpeed : speed;
        if (speedOverrides.Count > 0)
        {
            targetMovingSpeed = speedOverrides[speedOverrides.Count - 1]();
        }

        // Get targetVelocity from input.
        Vector2 targetVelocity =new Vector2( Input.GetAxis(horizontalInput) * targetMovingSpeed, Input.GetAxis(verticalInput) * targetMovingSpeed);

        // Apply movement.
        rigidbody.velocity = transform.rotation * new Vector3(targetVelocity.x, rigidbody.velocity.y, targetVelocity.y);
    }
}