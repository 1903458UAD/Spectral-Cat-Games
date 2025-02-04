using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FacialExpression : MonoBehaviour
{
    public Material dollarFace;
    public Material starFace;
    private Material currentMaterial;
    private Renderer renderer;

    // Start is called before the first frame update
    void Start()
    {
        renderer = GetComponent<Renderer>();
        currentMaterial = dollarFace;
       
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKey(KeyCode.T))
        {
            if (currentMaterial == dollarFace) { currentMaterial = starFace; }
            else { currentMaterial = dollarFace; }
        }

       renderer.material = currentMaterial;
    }
}
