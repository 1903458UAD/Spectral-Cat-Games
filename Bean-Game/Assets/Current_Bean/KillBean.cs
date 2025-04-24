using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillBean : MonoBehaviour
{
    public GameObject beanObject;
    public GameObject[] beans;
    private GameObject boomLoc;
    private Rigidbody rb;

    private void Start()
    {
        boomLoc = GameObject.Find("ExplosionArea");
    }

    public void boomBean()
    {
        foreach (GameObject bean in beans)
        {
            rb = boomLoc.GetComponent<Rigidbody>();
            rb.AddExplosionForce(500f, boomLoc.transform.position, 50f, 5f);
        }
    }

    public void killBean()
    {
        Destroy(beanObject);
    }
}
