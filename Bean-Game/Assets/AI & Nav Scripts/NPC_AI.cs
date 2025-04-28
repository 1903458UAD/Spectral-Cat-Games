using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;
using Debug = UnityEngine.Debug;
using FMOD.Studio;
using Unity.VisualScripting;

public class NPC_AI : MonoBehaviour
{
    [SerializeField] private UpgradeData inflateUpgrade;
    [SerializeField] private UpgradeData exclamationUpgrade;

    public GameObject spotted;

    public NavMeshAgent navMeshAgent;
    private Hiding_Spots currentHidingSpot;

    public float runRange = 2f;
    private bool isPickedUp = false;


    public bool escaping = false;

    public Hiding_Spots currentHSpot;

    private Hiding_Spots lastHidingSpot; // Track last used spot
    public Hiding_Spots lastHSpot;

    private bool isRunning = false;
    public bool RunTest = false;

    private bool isHidden = false;
    public bool hidTest = false;

    public float distanceToSpot = 0;

    public bool hasEscapedOnce = false;

    public bool initialHidingAssigned = false;

    //public bool hasHidingAssignment = false;

    public bool hasReachedRouteEnd = false;


    public AnimatorStateInfo stateInfo;

    public Animator animator;

    public AnimationClip loopingClip;
    public AnimationClip nonLoopingClip;


    public string looping = "Take 001";
    public string nonLooping = "Take 002";

    public bool playedOnce = false;

    public enum NPCState { Idle, Hiding, Running }
    public NPCState state = NPCState.Idle;

    public bool inflated = false;
    public bool alerted = false;

    public float maxVerticalDifference = 0.2f;

    public bool isFrozen = false;


    // [SerializeField] private EventReference beanMoveSound;
    public EventInstance beanFootsteps;

    private void Start()
    {
        beanFootsteps = AudioManager.instance.CreateInstance(FMODEvents.instance.beanFootsteps);
        FMODUnity.RuntimeManager.AttachInstanceToGameObject(beanFootsteps, transform, this);
        animator = GetComponent<Animator>();

        AIManager.Instance.AssignNewHidingSpot(this, false);
    }

    private void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        AIManager.Instance.RegisterNPC(this);
    }

    private void Update()
    {
        if (isPickedUp)
        { 
            return;
        }

        if (navMeshAgent == null)
        {
            return;
        }

        if(inflateUpgrade.internalUpgradeEnabled == true && inflated == false)
        {
            transform.localScale *= inflateUpgrade.baseValue;
            inflated = true;
        }

        if (exclamationUpgrade.internalUpgradeEnabled)
        {
            float distToPlayer = Vector3.Distance(transform.position,AIManager.Instance.GetPlayerPosition());
            bool inRange = distToPlayer <= runRange * 2f;

            if (inRange && !alerted)
            {
                spotted.SetActive(true);
                alerted = true;
            }
            else if (!inRange && alerted)
            {
                spotted.SetActive(false);
                alerted = false;
            }
        }


        distanceToSpot = Vector3.Distance(transform.position, GetHidingSpotPosition());

        stateInfo = animator.GetCurrentAnimatorStateInfo(0);
    }


    public void Freeze(float duration)
    {
        if (isFrozen) return;

        StartCoroutine(FreezeCoroutine(duration));
    }

    private IEnumerator FreezeCoroutine(float duration)
    {
        isFrozen = true;


        if (navMeshAgent != null)
        {
            navMeshAgent.isStopped = true;
        }
        if (animator != null)
        {
            animator.enabled = false;
        }

        yield return new WaitForSeconds(duration);

        // restore
        if (animator != null)
        {
            animator.enabled = true;
        }

        if (navMeshAgent != null)
        {
            navMeshAgent.isStopped = false;
        }
        // set to idle so they can pick a new action
        state = NPCState.Idle;

        isFrozen = false;
    }

    public void MoveTo(Vector3 destination)
    {
        if (navMeshAgent == null)
        {
            return;
        }
        //if (this.transform.position.x == this.transform.position.x)
        //{
        //    PLAYBACK_STATE playbackState;
        //    beanFootsteps.getPlaybackState(out playbackState);
        //    if (playbackState.Equals(PLAYBACK_STATE.STOPPED))
        //    {
        //        beanFootsteps.start();
        //    }
        //}
        //else
        //{
        //    beanFootsteps.stop(STOP_MODE.ALLOWFADEOUT);
        //}

        if (navMeshAgent.isOnNavMesh)
        {
            NavMeshHit hit;


            float targetY = (currentHidingSpot != null) ? currentHidingSpot.transform.position.y : destination.y;
           
            Vector3 samplePosition = new Vector3(destination.x, targetY, destination.z);
            
            float maxVerticalDifference = 0.5f;  // adjust as needed

            //if (NavMesh.SamplePosition(samplePosition, out hit, 2.0f, NavMesh.AllAreas))
            //{
            //    if (Mathf.Abs(hit.position.y - samplePosition.y) <= maxVerticalDifference)
            //    {
            //        navMeshAgent.SetDestination(hit.position);
            //    }
            //    else
            //    {
            //        Vector3 adjustedTarget = new Vector3(hit.position.x, samplePosition.y, hit.position.z);
            //        navMeshAgent.SetDestination(adjustedTarget);
            //    }
            //}
            navMeshAgent.SetDestination(samplePosition);
        }
       
    }

    public void SetLastHidingSpot(Hiding_Spots spot)
    {

        lastHidingSpot = spot;
        lastHSpot = spot;
    }

    public Hiding_Spots GetLastHidingSpot()
    {

        return lastHidingSpot;
    }

    public Hiding_Spots GetHidingSpot()
    {

        return currentHidingSpot;
    }

    public void SetHidingSpot(Hiding_Spots spot)
    {
        if (navMeshAgent == null)
        {
            return;
        }
        currentHidingSpot = spot;
        currentHSpot = spot;
    }

    public Vector3 GetHidingSpotPosition()
    {

        return currentHidingSpot != null ? currentHidingSpot.transform.position : transform.position;
    }

    public void OnReachedHidingSpot()
    {
        if (navMeshAgent == null)
        {
            return;
        }

        if (currentHidingSpot != null)
        {
            state = NPCState.Hiding;
            //Debug.Log($"[NPC_AI] {gameObject.name} reached hiding spot {currentHidingSpot.name} and is now hidden.");
            // Reset or pause the hiding timer once the bean is truly hidden.
            AIManager.Instance.ResetHidingTimerForNPC(this);
        }
        else
        {
           // Debug.Log($"[NPC_AI] {gameObject.name} did not have a valid hiding spot on arrival! Reassigning...");
            state = NPCState.Idle;
            AIManager.Instance.AssignNewHidingSpot(this, false);
        }
    }

    public void PlayBeanMoveSound(bool isMoving)
    {
        if (navMeshAgent == null)
        {
            return;
        }
        if (isMoving)
        {
            PLAYBACK_STATE playbackState;
            beanFootsteps.getPlaybackState(out playbackState);
            if (playbackState.Equals(PLAYBACK_STATE.STOPPED))
            {
                //beanFootsteps.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(transform.GetComponent<Rigidbody>().velocity));
                beanFootsteps.start();
                //Debug.Log("Starting Bean Move");
            }
        }
        else 
        {
            beanFootsteps.stop(STOP_MODE.IMMEDIATE);
        }
    }

    public void OnPickedUp()
    {
        isPickedUp = true;

        PlayBeanMoveSound(false);

        animator.enabled = false;

        if (currentHidingSpot != null)
        {
            currentHidingSpot.DecrementOccupancy();  //-1 from spot if the bean was hiding
            //Debug.Log($"[NPC_AI] {gameObject.name} was picked up and left hiding spot {currentHidingSpot.name}");
            SetHidingSpot(null); // Remove reference to the hiding spot
        }

        if (navMeshAgent != null)
        {
            navMeshAgent.isStopped = true;
            navMeshAgent.enabled = false; // Properly disable the agent to prevent errors
        }
    }

    public void OnDropped()
    {
        isPickedUp = false;



        if (navMeshAgent == null)
        {
            return;
        }
        // Try to find a valid position on the NavMesh
        if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 2.0f, NavMesh.AllAreas))
        {
           //Debug.Log($"[NPC_AI] {gameObject.name} repositioned to valid NavMesh position: {hit.position}");

            transform.position = hit.position; // Move to valid NavMesh point
            navMeshAgent.Warp(hit.position);  // Instantly corrects position
            navMeshAgent.isStopped = false;   // Resume movement
            navMeshAgent.enabled = true;
            animator.enabled = true;
        }
        else
        {
            //Debug.LogError($"[NPC_AI] {gameObject.name} could not find a valid NavMesh position nearby! Disabling movement.");
            navMeshAgent.enabled = false; // Prevent errors if no valid NavMesh position
        }

        state = NPCState.Idle;
    }

    public bool IsPickedUp()
    {
        return isPickedUp;
    }


    // method to check if the bean is on a shelf and to get an off-shelf target.
    public bool IsOnShelf()
    {
        // Check if a small sphere around the bean's position overlaps any collider tagged "Shelf"
        Collider[] hits = Physics.OverlapSphere(transform.position, 0.5f);
        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("Shelf"))
                return true;
        }
        return false;
    }

    public Vector3 GetOffShelfTarget()
    {
        RaycastHit hit;
        // Cast a ray downward from the bean's position.
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 5f))
        {
            return hit.point;
        }
        return transform.position;
    }


    private void OnDestroy()
    {
        if (AIManager.Instance != null)
        {
            AIManager.Instance.UnregisterNPC(this);
            //Debug.Log($"[NPC_AI] {gameObject.name} removed from AIManager before destruction.");
        }

        if (currentHidingSpot != null)
        {
            currentHidingSpot.DecrementOccupancy(); 
            //Debug.Log($"[NPC_AI] {gameObject.name} was destroyed and left hiding spot {currentHidingSpot.name}");
        }
    }
}