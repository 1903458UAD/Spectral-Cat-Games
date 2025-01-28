using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject beanPrefab; // The prefab for the NPC AI
    [SerializeField] private int initialBeanCount = 5; // Number of beans to spawn at the start
    [SerializeField] private Transform spawnArea; // Reference to the spawn area (optional)

    private List<GameObject> beanInstances = new List<GameObject>(); // List to keep track of spawned beans

    void Start()
    {
        if (beanPrefab == null)
        {
            return;
        }

        SpawnInitialBeans();
    }

    // Spawns the initial set of beans at the start of the game
    private void SpawnInitialBeans()
    {
        for (int i = 0; i < initialBeanCount; i++)
        {
            SpawnBean();
        }
    }

    // Spawns a single bean instance and adds it to the list
    public void SpawnBean()
    {
        Vector3 spawnPosition = GetRandomNavMeshPosition();
        GameObject newBean = Instantiate(beanPrefab, spawnPosition, Quaternion.identity);
        beanInstances.Add(newBean);
    }

    // Removes a bean instance from the list and destroys it
    public void RemoveBean(GameObject bean)
    {
        if (beanInstances.Contains(bean))
        {
            beanInstances.Remove(bean);
            Destroy(bean);
        }
    }

    // Gets a random position on the NavMesh within the spawn area
    private Vector3 GetRandomNavMeshPosition()
    {
        Vector3 randomPosition;

        if (spawnArea != null)
        {
            Bounds bounds = spawnArea.GetComponent<Collider>().bounds;
            randomPosition = new Vector3(
                UnityEngine.Random.Range(bounds.min.x, bounds.max.x),
                bounds.center.y,
                UnityEngine.Random.Range(bounds.min.z, bounds.max.z)
            );
        }
        else
        {
            randomPosition = new Vector3(
                UnityEngine.Random.Range(-10f, 10f),
                0f,
                UnityEngine.Random.Range(-10f, 10f)
            );
        }

        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomPosition, out hit, 10f, NavMesh.AllAreas))
        {
            return hit.position;
        }

        // If no valid NavMesh position is found, return a default position
        return transform.position;
    }

    // Gets the list of all active beans
    public List<GameObject> GetBeans()
    {
        return beanInstances;
    }

    // Example update loop to showcase management logic
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            SpawnBean();
        }

        if (Input.GetKeyDown(KeyCode.R) && beanInstances.Count > 0)
        {
            RemoveBean(beanInstances[0]);
        }
    }
}
