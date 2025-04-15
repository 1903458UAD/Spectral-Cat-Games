using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;

public class DroppedAssetScript : MonoBehaviour
{
    [SerializeField] private EventReference fallFX;
    /*[SerializeField] private EventReference bookFallFX;
    [SerializeField] private EventReference mugFallFX;
    [SerializeField] private EventReference chairFallFX;
    [SerializeField] private EventReference cupFallFX;
    [SerializeField] private EventReference bottleFallFX;*/

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.relativeVelocity.magnitude > 1)
        {
            AudioManager.instance.PlayOneShot(fallFX, this.transform.position);
        }

        /*if (gameObject.CompareTag("Book"))
        {
            if (collision.relativeVelocity.magnitude > 1)
            {
                AudioManager.instance.PlayOneShot(bookFallFX, this.transform.position);
            }
        }

        if (gameObject.CompareTag("Mug"))
        {
            if (collision.relativeVelocity.magnitude > 1)
            {
                AudioManager.instance.PlayOneShot(mugFallFX, this.transform.position);
            }
        }

        if (gameObject.CompareTag("Chair"))
        {
            if (collision.relativeVelocity.magnitude > 1)
            {
                AudioManager.instance.PlayOneShot(chairFallFX, this.transform.position);
            }
        }

        if (gameObject.CompareTag("PaperCup"))
        {
            if (collision.relativeVelocity.magnitude > 1)
            {
                AudioManager.instance.PlayOneShot(cupFallFX, this.transform.position);
            }
        }

        if (gameObject.CompareTag("PlasticBottle"))
        {
            if (collision.relativeVelocity.magnitude > 1)
            {
                AudioManager.instance.PlayOneShot(bottleFallFX, this.transform.position);
            }
        }*/
    }
}
