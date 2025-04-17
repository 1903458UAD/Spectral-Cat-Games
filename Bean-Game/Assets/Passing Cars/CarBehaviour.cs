using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarBehaviour : MonoBehaviour
{
    private Transform node1;
    private Transform node2;
    private Transform node3;

    public Transform currentNode;
    public float carSpeed;
    private Vector3 newDirection;
    private float singleStep;

    public PassingCarsScript PCS;

    private void Start()
    {
        PCS = gameObject.GetComponentInParent<PassingCarsScript>();

        node1 = PCS.node1;
        node2 = PCS.node2;
        node3 = PCS.node3;
        carSpeed = 0.1f;

        currentNode = node1;
    }

    private void Update()
    {
        singleStep = 10f * Time.deltaTime;
        if (currentNode == node1)
        {
            transform.position = Vector3.MoveTowards(transform.position, node2.position, carSpeed);
            newDirection = Vector3.RotateTowards(transform.forward, (node2.position - transform.position), 1f, 0.0f);
            transform.rotation = Quaternion.LookRotation(newDirection);

            if (Vector3.Distance(transform.position, node2.position) < 0.001f)
            {
                currentNode = node2;
            }
        }
        else if (currentNode == node2)
        {
            transform.position = Vector3.MoveTowards(transform.position, node3.position, carSpeed);
            newDirection = Vector3.RotateTowards(transform.forward, (node3.position - transform.position), singleStep, 0.0f);
            transform.rotation = Quaternion.LookRotation(newDirection);

            if (Vector3.Distance(transform.position, node3.position) < 0.001f)
            {
                currentNode = node3;
            }
        }
        else if (currentNode == node3)
        {
            PCS.DespawnCar(gameObject);
        }
    }
}
