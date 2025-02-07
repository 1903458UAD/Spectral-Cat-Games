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

   
    public void LoseLife() // Reduce player health (Called when get an order wrong)
    {
   
        Debug.Log("Player lost a life! Remaining lives: " + currentLives);

        if (currentLives > 0)
        {
            currentLives--;
            UpdateLifeUI();
        }
        if (currentLives <= 0)
        {
            GameOver();
        }
    }


    void UpdateLifeUI()
    {
        for (int i = 0; i < lifeIcons.Length; i++)
        {
            if (i < currentLives)
            {
                lifeIcons[i].color = Color.white; // Placeholders - Represent lives remaining
            }
            else
            {
                lifeIcons[i].color = Color.red; // Placeholders - Represent Lives Lost
            }
        }
    }

    private void GameOver()
    {
        Debug.Log("Game Over! Player ran out of lives.");
        UIManager.Instance.ShowGameOverScreen();
        //Pause the Movement

        //TOBE implemented: Reload/Reset System. Game Over UI etc....
    }
}
