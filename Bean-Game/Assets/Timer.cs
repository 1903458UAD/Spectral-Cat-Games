using UnityEngine;

public class Timer : MonoBehaviour
{
    public float waitingTime;
    private float timeRemaining = 0;
    public bool timerRestart;

    private void Start()
    {
        timerRestart = false;
        timeRemaining = waitingTime;
    }

    void Update()
    {

        //Debug.Log("script is running");

        if (timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;
            // Debug.Log(timeRemaining);
        }

        else
        {
            Debug.Log("Customer has run out of patience!");
            timerRestart = true;
        }

        if (timerRestart)
        {
            timeRemaining = waitingTime;
            timerRestart = false;
        }
    }

    void DriveForward()
    {

    }
    void Wait()
    {

    }

}
