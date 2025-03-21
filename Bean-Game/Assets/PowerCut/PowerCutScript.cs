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
    public bool lightsOnly;

    #endregion
    [SerializeField] private EventReference powerCutsOff;
    [SerializeField] private EventReference powerTurnsOn;

    private void Start()
    {
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
        AudioManager.instance.PlayOneShot(powerCutsOff, this.transform.position);
    }

    private void lightsOff()
    {
        lights.SetActive(false);
        playerTorch.SetActive(true);
    }

    private void lightsOn()
    {
        lights.SetActive(true);
        playerTorch.SetActive(false);
    }

    private void powerOn()
    {
        machineButton.enabled = true;
        till.enabled= true;
        AudioManager.instance.PlayOneShot(powerTurnsOn, this.transform.position);
    }

    public void tripPower()
    {
        lightsOff();

        if (!lightsOnly)
        {
            powerOff();
        }
    }

    public void fixPower()
    {
        lightsOn();
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
