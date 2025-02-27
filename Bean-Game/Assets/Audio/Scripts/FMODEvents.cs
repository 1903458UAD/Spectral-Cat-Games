using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;

public class FMODEvents : MonoBehaviour
{
    [field: Header("Ambiance")]
    [field: SerializeField] public EventReference ambiance {  get; private set; }
   public static FMODEvents instance {  get; private set; }

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogError("More than 1 fmod events script in scene");
        }
        instance = this;
    }
}
