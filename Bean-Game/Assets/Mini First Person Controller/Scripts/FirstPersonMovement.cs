using System.Collections.Generic;
using UnityEngine;

public class FirstPersonMovement : MonoBehaviour
{
    public float speed = 5;

    [Header("Running")]
    public bool canRun = true;
    public bool IsRunning { get; private set; }
    private bool IsRunningController;
    private bool IsRunningKeyboard;
    public float runSpeed = 9;

    public KeyCode runningKeyboard = KeyCode.LeftShift;
    public KeyCode runningController = KeyCode.Joystick1Button7;

    Rigidbody rigidbody;
    /// <summary> Functions to override movement speed. Will use the last added override. </summary>
    public List<System.Func<float>> speedOverrides = new List<System.Func<float>>();

    private string horizontalInputKeyboard = "Horizontal";
    private string verticalInputKeyboard = "Vertical";
    private string horizontalInputController = "Horizontal Joystick";
    private string verticalInputController = "Vertical Joystick";
    
    void Awake()
    {
        // Get the rigidbody on this.
        rigidbody = GetComponent<Rigidbody>();

        speed = speed * StaticData.speedPassed;
        runSpeed = runSpeed * StaticData.speedPassed;
    }

    void FixedUpdate()
    {
        IsRunningKeyboard = canRun && Input.GetKey(runningKeyboard);
        IsRunningController = canRun && Input.GetKey(runningController);

        // Get target move speed
        float targetMovingSpeed;
        if (IsRunningKeyboard || IsRunningController)
        {
            IsRunning = true;
            targetMovingSpeed = runSpeed;
        }

        else
        {
            IsRunning = false;
            targetMovingSpeed = speed;
        }
       
        // Get targetVelocity from input.
        Vector2 targetVelocityKeyboard = new Vector2(Input.GetAxis(horizontalInputKeyboard) * targetMovingSpeed, Input.GetAxis(verticalInputKeyboard) * targetMovingSpeed);
        Vector2 targetVelocityController= new Vector2(Input.GetAxis(horizontalInputController) * targetMovingSpeed, Input.GetAxis(verticalInputController) * targetMovingSpeed);

        Vector2 finalVelocity = targetVelocityController != Vector2.zero ? targetVelocityController : targetVelocityKeyboard;

        // Apply movement if there's any input
        if (finalVelocity != Vector2.zero)
        {
            rigidbody.velocity = transform.rotation * new Vector3(finalVelocity.x, rigidbody.velocity.y, finalVelocity.y);
        }
    }
}