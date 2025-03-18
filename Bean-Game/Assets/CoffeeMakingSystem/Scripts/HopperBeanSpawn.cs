using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HopperBeanSpawn : MonoBehaviour
{
    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        StartCoroutine(explode());
    }

    private IEnumerator explode()
    {
        yield return new WaitForSeconds(1);
        rb.AddForce(Random.onUnitSphere);
        Debug.Log("Bean splode");
    }
}
