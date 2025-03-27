using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
//using UnityEngine.UIElements;
using UnityEngine.UI;

public class CustomerPatienceBar : MonoBehaviour
{
    public static CustomerPatienceBar Instance;

    public Slider slider;

    public Sprite happy;
    public Sprite bored;
    public Sprite angry;

    public GameObject customerMoodlet;
    SpriteRenderer sprite;

    private void Start()
    {
        sprite = customerMoodlet.GetComponent<SpriteRenderer>();
    }
    public void GetStartTime(float startTime)
    {
        slider.maxValue = startTime;
        slider.value = startTime;

        sprite = happy.GetComponent<SpriteRenderer>();
    }

    public void GetCurrentTime(float currentTime)
    {
        slider.value = currentTime;
    }

    public void setMoodlet(string mood)
    {
        switch (mood)
        {
            case "happy":
                sprite = happy.GetComponent<SpriteRenderer>();
                break;

            case "bored":
                sprite = bored.GetComponent<SpriteRenderer>();
                break;

            case "angry":
                sprite = angry.GetComponent<SpriteRenderer>();
                break;

            default:
                sprite = happy.GetComponent<SpriteRenderer>();
                break;
        }
    }    
}
