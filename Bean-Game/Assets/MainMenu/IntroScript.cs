using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntroScript : MonoBehaviour
{
    #region Variables

    //Player Camera
    private Camera playerCamera;
    private Transform playerStart;

    //Menu Camera
    [SerializeField] private Camera menuCamera;
    private Transform menuStart;

    //Animator
    private Animator anim;
    private Animator lidAnim;
    public float animLength;

    //public PowerCutScript PowerCut;
    public FirstPersonLook fpLook;

    #endregion
    public bool gameplayStart = false;

    private void Start()
    {
        playerCamera = GameObject.Find("First Person Camera").GetComponent<Camera>();
        menuCamera = GameObject.Find("MenuCam").GetComponent<Camera>();
        anim = menuCamera.GetComponent<Animator>();

        gameplayStart = false;

        playerStart = playerCamera.transform;
        menuStart = menuCamera.transform;
        UIManager.Instance.HideGameplayUI();
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Alpha1))
        {
            PlayIntro();
        }
    }

    public void PlayIntro()
    {
        anim.enabled = true;
        StartCoroutine(ChangeViewDelay());
    }

    public void PlaySplosion()
    {
        lidAnim = GameObject.Find("lid").GetComponent<Animator>();
        lidAnim.enabled = true;
    }

    private IEnumerator ChangeViewDelay()
    {
        yield return new WaitForSeconds(animLength);


        //PowerCut.InitialShutOff();
        anim.enabled = false;
        menuCamera.enabled = false;
        playerCamera.enabled = true;
        fpLook.cameraControl = true;

        UIManager.Instance.ShowGameplayUI();

        gameplayStart = true;
    }
}
