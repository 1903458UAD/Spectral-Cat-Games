using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class AudioManager : MonoBehaviour
{
    private List<EventInstance> eventInstances;
    private List<StudioEventEmitter> eventEmitters;

    //public bool STOPLOOPINGMYMUSIC;

    private EventInstance ambianceEventInstance;
    public static AudioManager instance {  get; private set; }

    public int restarting = 0;

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogError("More than 1 audio manager in scene.");
        }
        instance = this;
        //STOPLOOPINGMYMUSIC = false;
    }

    private void Start()
    {
        
        
            InitializeAmbiance(FMODEvents.instance.ambiance);
            InitializeAmbiance(FMODEvents.instance.musicFMOD);
        
    }

    private void InitializeAmbiance (EventReference ambianceEventReference)
    {
        
        ambianceEventInstance = CreateInstance(ambianceEventReference);
        ambianceEventInstance.start();

        /*if (STOPLOOPINGMYMUSIC == true)
        {
            ambianceEventInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        }
        STOPLOOPINGMYMUSIC = true;*/
    }

    public void PlayOneShot(EventReference sound, Vector3 worldPos)
    {
        RuntimeManager.PlayOneShot(sound, worldPos);
    }

    public EventInstance CreateInstance(EventReference eventReference)
    {
        EventInstance eventInstance = RuntimeManager.CreateInstance(eventReference);
        return eventInstance;
    }

    public void SetAmbianceParameter(string parameterName, float parameterValue)
    {
        ambianceEventInstance.setParameterByName(parameterName, parameterValue);
    }

    private void Update()
    {
        
    }
}
