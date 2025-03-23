using System.Collections.Generic;
using UnityEngine;
using FMOD.Studio;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine.WSA;


public class FirstPersonMovement : MonoBehaviour
{
    [SerializeField] private UpgradeData upgradeData;

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

    private EventInstance playerFootsteps;

    [SerializeField] private string parameterName;
    [SerializeField] private float parameterValue;

    private void Start()
    {
        playerFootsteps = AudioManager.instance.CreateInstance(FMODEvents.instance.playerFootsteps);
    }

    void Awake()
    {
        // Get the rigidbody on this.
        rigidbody = GetComponent<Rigidbody>();
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
            targetMovingSpeed = runSpeed * upgradeData.internalBaseValue;
        }

        else
        {
            IsRunning = false;
            targetMovingSpeed = speed * upgradeData.internalBaseValue;
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
        UpdateSound();
    }
    private void UpdateSound()
    {

        if (rigidbody.velocity.x > 0.5 || rigidbody.velocity.x < -0.5 ||rigidbody.velocity.z > 0.5|| rigidbody.velocity.z < -0.5)
        {
            AudioManager.instance.SetAmbianceParameter(parameterName, parameterValue);
            PLAYBACK_STATE playbackState;
            playerFootsteps.getPlaybackState(out playbackState);
            if (playbackState.Equals(PLAYBACK_STATE.STOPPED))
            {
                playerFootsteps.start();
            }
        }
        else
        {
            playerFootsteps.stop(STOP_MODE.ALLOWFADEOUT);
            AudioManager.instance.SetAmbianceParameter("Activate Bass", 0);
        }
    }
}