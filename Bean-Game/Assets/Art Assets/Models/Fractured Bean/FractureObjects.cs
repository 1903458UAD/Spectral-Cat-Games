using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FractureObjects : MonoBehaviour
{
    public GameObject originalObject;
    public GameObject fracturedObject;
    public float explosionMinForce = 5;
    public float explosionMaxForce = 100;
    public float explosionForceRadius = 10;
    public float fracScalefactor = 1;

    private GameObject fractObj;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Explode();
        }
        if (Input.getKeyDown(KeyCode.R))
        {
            Reset();
        }
    }

    void Explode()
    {
        if (originalObject != null)
        {
            originalObject.SetActive(false);

            if (fracturedObject != null)
            {
                fractObj = Instantiate(fracturedObject) as GameObject;

                foreach (transform t in fractObj.transform)
                {
                    var rb = t.GetComponent<Rigidbody>();

                    if (rb != null)
                        rb.AddexplosionForce(random.Range(explosionMinForce, explosionMaxForce), originalObject.transform.position, explosionForceRadius);

                    StartCoroutine(Shrink(t, 2));
                }

                Destroy(fractObj, 5);

            }
        }
    }

    void Reset()
    {
        Destroy(fractObj);
        originalObject.SetActive(true);
    }

    IEnumerator Shrink(Transform t, float delay)
    {
        yeild return new WaitForSeconds(delay);

        Vector3 newScale = t.localScale;

        while (newScale.x >= 0)
        {
            newScale -= new Vector3(fracScalefactor, fracScalefactor, fracScalefactor);

            t.localScale = newScale;
            yeild return new WaitForSeconds(0.05f);
        }
    }
}
