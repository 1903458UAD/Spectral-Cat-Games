using UnityEngine;
using Random = UnityEngine.Random;
using Debug = UnityEngine.Debug;


public class Hiding_Spots : MonoBehaviour
{
    public enum HidingType { BehindCover, InsideCover, Underneath, Shelf, Normal, Small, Medium, Trap , Cage}

    [Header("Hiding Spot Settings")]
    public HidingType hidingType = HidingType.Normal;
    public int MaxOccupancy;  // Maximum NPCs allowed in this spot
    private int occupancy = 0;    // Current number of NPCs in this spot
    public int currentOccupancy = 0;

    public int Occupancy => occupancy; // Read-only property to get current occupancy

    private Collider _collider;

    private void Awake()
    {
        _collider = GetComponent<Collider>();
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
        return occupancy < MaxOccupancy;
    }


    public void IncrementOccupancy()
    {
        if (occupancy < MaxOccupancy)
        {
            occupancy++;
            currentOccupancy = occupancy;
            //Debug.Log($"[Hiding_Spots] {gameObject.name} occupancy increased: {occupancy}/{MaxOccupancy}");

            if (_collider != null && hidingType == HidingType.Trap)
            {
                _collider.isTrigger = false;
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

            if (_collider != null && hidingType == HidingType.Trap && occupancy == 0)
            {
                _collider.isTrigger = true;
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
       
        if (occupancy < 3)
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
            }
                




        }
        else
        {
            //Debug.Log("Cannot add more beans! Cage is full.");
        }

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
