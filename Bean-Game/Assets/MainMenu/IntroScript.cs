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
    public float animLength;

    public PowerCutScript PowerCut;
    
    #endregion

    private void Start()
    {
        playerCamera = GameObject.Find("First Person Camera").GetComponent<Camera>();
        menuCamera = GameObject.Find("MenuCam").GetComponent<Camera>();
        anim = menuCamera.GetComponent<Animator>();

        playerStart = playerCamera.transform;
        menuStart = menuCamera.transform;
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

    private IEnumerator ChangeViewDelay()
    {
        yield return new WaitForSeconds(animLength);

        PowerCut.InitialShutOff();
        anim.enabled = false;
        menuCamera.enabled = false;
        playerCamera.enabled = true;
    }
}
