using UnityEngine;
using Random = UnityEngine.Random;
using Debug = UnityEngine.Debug;
using System.Collections.Generic;
using System.Collections;


public class Hiding_Spots : MonoBehaviour
{
    public enum HidingType { BehindCover, InsideCover, Underneath, Shelf, Normal, Small, Medium, Trap , Cage}

    [Header("Hiding Spot Settings")]
    public HidingType hidingType = HidingType.Normal;
    public int MaxOccupancy;  // Maximum NPCs allowed in this spot
    private int occupancy = 0;    // Current number of NPCs in this spot
    public int currentOccupancy = 0;


    //Reset the hiding spot pos after drop
    private Quaternion originalRotation;

    private Vector3 originalPosition;

    private bool isValidSpot = true;

    private bool isResetting = false;

    private List<BeanInteraction> assignedBeans = new List<BeanInteraction>();

    private List<BeanInteraction> beansInCage = new List<BeanInteraction>();

    //Reset after player looks away, spooooky
    [SerializeField, Range(0f, 90f)]
    private float viewAngleThreshold = 30f;


    public int Occupancy => occupancy; // Read-only property to get current occupancy

    private Collider collider;

    //private void Update()
    //{
       
    //    if (hidingType != HidingType.Trap && isValidSpot)
    //    {
    //        if (Vector3.Distance(transform.position, originalPosition) > 0.01f)
    //        {
    //            InvalidateSpot();
    //        }
    //    }
    //}




    private void Awake()
    {
        collider = GetComponent<Collider>();

        originalPosition = transform.position;
        originalRotation = transform.rotation;
    }



    public void ReserveSpot()
    {
        if (occupancy < MaxOccupancy)
        {
            occupancy++;  //Reserve the spot immediately
            currentOccupancy = occupancy;
        }
    }

    public void ReleaseSpot()
    {
        if (occupancy > 0)
        {
            occupancy--;  //Release the spot if an NPC leaves
            currentOccupancy = occupancy;
        }
    }

    public bool IsAvailable()
    {
        return isValidSpot && occupancy < MaxOccupancy;
    }


    private void InvalidateSpot()
    {
        isValidSpot = false;


        foreach (var bean in assignedBeans)
        {
            var beanInst = bean.GetComponent<NPC_AI>();
            if (beanInst != null)
            {
                beanInst.SetHidingSpot(null);
                beanInst.state = NPC_AI.NPCState.Idle;

                beanInst.Freeze(3f);
            }
            bean.GetComponent<InteractableObject>()?.ReleaseObject();
        }
        assignedBeans.Clear();


        //ResetHidingSpot();

        //if (collider != null)
        //{
        //    collider.isTrigger = true;
        //}

        //if (!isResetting)
        //{
        //    StartCoroutine(WatchForReset());
        //}
    }

    //private IEnumerator WatchForReset()
    //{
    //    isResetting = true;
    //    var cam = Camera.main;

    //    bool LookingAt(Vector3 worldPos)
    //    {
    //        var dir = (worldPos - cam.transform.position).normalized;
    //        return Vector3.Angle(cam.transform.forward, dir) < viewAngleThreshold;
    //    }

    //    while (true)
    //    {  
    //        if (!LookingAt(transform.position) && !LookingAt(originalPosition))
    //        {
    //            transform.rotation = originalRotation;
    //            transform.position = originalPosition;
    //            isValidSpot = true;
    //            isResetting = false;

    //            // restore collider to solid
    //            if (collider != null)
    //                collider.isTrigger = false;

    //            yield break;
    //        }
    //        yield return null;
    //    }
    //}



    public void IncrementOccupancy()
    {
        if (occupancy < MaxOccupancy)
        {
            occupancy++;
            currentOccupancy = occupancy;
            //Debug.Log($"[Hiding_Spots] {gameObject.name} occupancy increased: {occupancy}/{MaxOccupancy}");

            if (collider != null && hidingType == HidingType.Trap)
            {
                collider.isTrigger = false;
            }
        }
        else
        {
           // Debug.LogWarning($"[Hiding_Spots] {gameObject.name} is already at max occupancy!");
        }
    }


    public void DecrementOccupancy()
    {
        if (occupancy > 0)
        {
            occupancy--;
            currentOccupancy = occupancy;
            //Debug.Log($"[Hiding_Spots] {gameObject.name} occupancy decreased: {occupancy}/{MaxOccupancy}");

            if (collider != null && hidingType == HidingType.Trap && occupancy == 0)
            {
                collider.isTrigger = true;
            }

        }
        else
        {
            //Debug.LogWarning($"[Hiding_Spots] {gameObject.name} occupancy is already zero!");      
        }
    }



    public bool IsTrap()
    {
        if(hidingType == HidingType.Trap)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool IsCage()
    {
        if (hidingType == HidingType.Cage)
        {
            return true;
        }
        else
        {
            return false;
        }
    }


    public void AddBean(BeanInteraction bean)
    {
       

        if (occupancy < MaxOccupancy)
        {
          
            bean.gameObject.transform.position = this.transform.position;
            InteractableObject interactable = bean.GetComponent<InteractableObject>();




            NPC_AI beanNPC = bean.GetComponent<NPC_AI>();

            if (interactable != null)
            {
                //fully drop the bean.
                
            }

            


            if (beanNPC != null)
            {
                // Clear the picked-up state.
                interactable.ReleaseObject();

                // Set the bean's hiding spot to this cage.
                beanNPC.SetHidingSpot(this);
            }

            if (beanNPC != null)
            {
                beanNPC.state = NPC_AI.NPCState.Hiding;

                IncrementOccupancy();
                assignedBeans.Add(bean);
            }
                




        }
        else
        {
            //Debug.Log("Cannot add more beans! Cage is full.");
        }

    }

    public bool AddBeanToCage(BeanInteraction bean)
    {
        if (!IsCage() || beansInCage.Count >= MaxOccupancy)
        {
            return false;
        }

     
        var io = bean.GetComponent<InteractableObject>();

        if (io != null && io.GetIsHeld())
        {
            io.ReleaseObject();
        }


        bean.gameObject.layer = LayerMask.NameToLayer("Default");

 
        if (io != null)
        {
            io.enabled = false;
        }


        bean.transform.SetParent(transform, worldPositionStays: true);
        var b = GetComponent<Collider>().bounds;

        bean.transform.position = new Vector3(
            Random.Range(b.min.x, b.max.x),
            transform.position.y,
            Random.Range(b.min.z, b.max.z));


        var beanNPC = bean.GetComponent<NPC_AI>();

        if (beanNPC != null)
        {
            beanNPC.navMeshAgent.Warp(bean.transform.position);
            beanNPC.SetHidingSpot(this);
            beanNPC.state = NPC_AI.NPCState.Hiding;
            AIManager.Instance.ResetHidingTimerForNPC(beanNPC);
        }

        beansInCage.Add(bean);
        IncrementOccupancy();
        assignedBeans.Add(bean);
        return true;
    }

    public BeanInteraction ExtractRandomBean()
    {
        if (!IsCage() || beansInCage.Count == 0)
        {
            return null;
        }

        int idx = Random.Range(0, beansInCage.Count);
        var bean = beansInCage[idx];
        beansInCage.RemoveAt(idx);


        bean.transform.SetParent(null, true);


        bean.gameObject.layer = LayerMask.NameToLayer("Interactable");


        var io = bean.GetComponent<InteractableObject>();
        if (io != null)
        {
            io.enabled = true;
        }

        DecrementOccupancy();
        assignedBeans.Remove(bean);
        return bean;
    }



    public void SetOccupancy(int count)
    {
        occupancy = count;
        currentOccupancy = occupancy;
        //Debug.Log($"[Hiding_Spots] {gameObject.name} occupancy set to: {occupancy}/{MaxOccupancy}");
    }

    public void ResetHidingSpot()
    {
        occupancy = 0;  // Clear any NPCs marked inside
        currentOccupancy = occupancy;
        //Debug.Log($"[Hiding_Spots] {gameObject.name} reset to empty.");
    }

    public bool IsOnSameShelf(NPC_AI bean)
    {
       
        Collider shelfCol = GetComponent<Collider>();
        if (shelfCol == null) return false;

        Collider[] hits = Physics.OverlapSphere(bean.transform.position, 0.5f);
        foreach (Collider hit in hits)
        {
            if (hit == shelfCol)
                return true;
        }
        return false;
    }


}
