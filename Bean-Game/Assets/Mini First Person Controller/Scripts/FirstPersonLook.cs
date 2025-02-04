using UnityEngine;

public class FirstPersonLook : MonoBehaviour
{
    [SerializeField]
    Transform character;
    public float sensitivity = 2;
    public float smoothing = 1.5f;

    Vector2 velocity;
    Vector2 frameVelocity;

    private string horizontalInput = "";
    private string verticalInput = "";

    private enum InputType { Controller, Keyboard };
    private InputType currentInput;

    void Reset()
    {
        // Get the character from the FirstPersonMovement in parents.
        character = GetComponentInParent<FirstPersonMovement>().transform;
    }

    void Start()
    {
        // Lock the mouse cursor to the game screen.
        Cursor.lockState = CursorLockMode.Locked;
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
            horizontalInput = "Joystick X";
            verticalInput = "Joystick Y";
        }

        else
        {
            horizontalInput = "Mouse X";
            verticalInput = "Mouse Y";
        }

        // Get smooth velocity.
        Vector2 mouseDelta = new Vector2(Input.GetAxisRaw(horizontalInput), Input.GetAxisRaw(verticalInput));
        Vector2 rawFrameVelocity = Vector2.Scale(mouseDelta, Vector2.one * sensitivity);
        frameVelocity = Vector2.Lerp(frameVelocity, rawFrameVelocity, 1 / smoothing);
        velocity += frameVelocity;
        velocity.y = Mathf.Clamp(velocity.y, -90, 90);

        // Rotate camera up-down and controller left-right from velocity.
        transform.localRotation = Quaternion.AngleAxis(-velocity.y, Vector3.right);
        character.localRotation = Quaternion.AngleAxis(velocity.x, Vector3.up);
    }
}
