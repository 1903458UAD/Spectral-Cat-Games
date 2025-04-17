using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;

public class PowerCutScript : MonoBehaviour
{
    #region Variables

    private GameObject lights;
    private GameObject playerTorch;
    [SerializeField] private Collider machineButton;
    [SerializeField] private Collider till;
    public float initialDelay;
    public float cooldown;
    public float randDelay;
    public float delayMin;
    public float delayMax;
    //public bool lightsOnly;

    #endregion
    [SerializeField] private EventReference powerCutsOff;
    [SerializeField] private EventReference powerTurnsOn;

    GameObject powerexcla;

    private void Start()
    {
        powerexcla = GameObject.Find("powerexcla");
        powerexcla.SetActive(false);
        lights = GameObject.Find("LightsContainer");
        playerTorch = GameObject.Find("Torch");
        playerTorch.SetActive(false);
    }

    public void InitialShutOff()
    {
        Invoke("tripPower", initialDelay);
    }

    private void powerOff()
    {
        machineButton.enabled = false;
        till.enabled = false;
        powerexcla.SetActive(true);
        AudioManager.instance.PlayOneShot(powerCutsOff, this.transform.position);
    }
    //No longer being used.
    private void lightsOff()
    {
        lights.SetActive(false);
        playerTorch.SetActive(true);
    }
    //No longer being used.
    private void lightsOn()
    {
        lights.SetActive(true);
        playerTorch.SetActive(false);
    }
    
    private void powerOn()
    {
        machineButton.enabled = true;
        till.enabled= true;
        powerexcla.SetActive(false);
        AudioManager.instance.PlayOneShot(powerTurnsOn, this.transform.position);
    }

    public void tripPower()
    {
        //lightsOff();

        powerOff();
    }

    public void fixPower()
    {
        //lightsOn();
        powerOn();
        StartCoroutine(powerCooldown());
    }

    private IEnumerator powerCooldown()
    {
        yield return new WaitForSeconds(cooldown);

        randDelay = Random.Range(delayMin, delayMax);
        Invoke("tripPower", randDelay);
    }
}
