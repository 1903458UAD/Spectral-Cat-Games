using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PassingCarsScript : MonoBehaviour
{
    [SerializeField] private GameObject[] cars;
    public Transform node1;
    public Transform node2;
    public Transform node3;

    private bool spawnable;
    private int randomCar;
    private int carCounter;
    public int carInterval;

    private GameObject car_1;
    private GameObject car_2;

    private void Start()
    {
        cars = Resources.LoadAll<GameObject>("Cars");
        spawnable = true;
        carCounter = 0;
    }

    private void LateUpdate()
    {
        if (spawnable && carCounter < 2)
        {
            randomCar = Random.Range(0,cars.Length);
            SpawnCar(cars[randomCar]);
        }
    }

    private void SpawnCar(GameObject car)
    {
        if (car_1 == null && car_2 == null)
        {
            car_2 = Instantiate(car, node1.position, Quaternion.identity, this.transform);
            carCounter += 1;
            spawnable = false;
            StartCoroutine(SpawnCooldown());
        }
        else if (car_1 == null)
        {
            car_1 = Instantiate(car, node1.position, Quaternion.identity, this.transform);
            carCounter += 1;
            spawnable = false;
            StartCoroutine(SpawnCooldown());
        }
    }

    public void DespawnCar(GameObject car)
    {
        Destroy(car);
        carCounter -= 1;
    }

    private IEnumerator SpawnCooldown()
    {
        yield return new WaitForSeconds(carInterval);

        spawnable = true;
    }
}
