using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShatteredBeanSwirl : MonoBehaviour
{
    public Transform swirlCenter; 
    public float swirlSpeed = 100f;
    public float inwardForce = 5f;

    private Rigidbody[] rbs;

    void Awake()
    {
        rbs = GetComponentsInChildren<Rigidbody>();
    }

    void FixedUpdate()
    {
        if (swirlCenter == null || rbs == null) return;

        foreach (Rigidbody rb in rbs)
        {
            if (rb == null) continue;

            Vector3 dirToCenter = (swirlCenter.position - rb.position).normalized;
            Vector3 swirlDir = Vector3.Cross(Vector3.up, dirToCenter);

            Vector3 swirlForce = swirlDir * swirlSpeed + dirToCenter * inwardForce;
            rb.AddForce(swirlForce * Time.fixedDeltaTime, ForceMode.VelocityChange);
        }
    }
}
