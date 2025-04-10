using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    public int maxLives = 3; // Lives the player starts with (Likely needing to adjust for balancing)
    private int currentLives;

    public Image[] lifeIcons; // Assign these in the inspector



    private void Start()
    {
        currentLives = maxLives; // Player starts with full lives

    }

   
    public void LoseLife(string lifeLossDesc) // Reduce player health (Called when get an order wrong)
    {
   
        Debug.Log("Player lost a life! Remaining lives: " + currentLives);

        if (currentLives > 0)
        {
            currentLives--;
            UpdateLifeUI(lifeLossDesc);
        }
        if (currentLives <= 0)
        {
            GameOver();
        }
    }


    void UpdateLifeUI(string lifeLossDesc)
    {
        UIManager.Instance.UpdateLifeUI(currentLives, lifeLossDesc);
    }

    private void GameOver()
    {
        Debug.Log("Game Over! Player ran out of lives.");
        UIManager.Instance.ShowGameOverScreen();

    }
}
