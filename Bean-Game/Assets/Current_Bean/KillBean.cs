using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillBean : MonoBehaviour
{
    public GameObject beanObject;

    public void killBean()
    {
        Destroy(beanObject);
    }
}
