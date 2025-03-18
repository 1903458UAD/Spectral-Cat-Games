using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    private void Start()
    {
        lights = GameObject.Find("LightsContainer");
        playerTorch = GameObject.Find("Torch");
        Invoke("tripPower", initialDelay);
    }

    private void powerOff()
    {
        machineButton.enabled = false;
        till.enabled = false;
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
    }

    private void tripPower()
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
