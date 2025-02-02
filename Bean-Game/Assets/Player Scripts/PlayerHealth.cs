using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int maxLives = 3; // Lives the player starts with (Likely needing to adjust for balancing)
    private int currentLives;

    private void Start()
    {
        currentLives = maxLives; // Player starts with full lives

    }

   
    public void LoseLife() // Reduce player health (Called when get an order wrong)
    {
        currentLives--;
        Debug.Log("Player lost a life! Remaining lives: " + currentLives);

        if (currentLives <= 0)
        {
            GameOver();
        }
    }

    private void GameOver()
    {
        Debug.Log("Game Over! Player ran out of lives.");
        UIManager.Instance.ShowDeathScreen();
        //Pause the Movement

        //TOBE implemented: Reload/Reset System. Game Over UI etc....
    }
}
