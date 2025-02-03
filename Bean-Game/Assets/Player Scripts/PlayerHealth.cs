using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 100;
    private int currentHealth;

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        UnityEngine.Debug.Log("[PlayerHealth] Player took damage: " + damage);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.UpdatePlayerLives(currentHealth);
        }
        else
        {
            UnityEngine.Debug.LogError("[PlayerHealth] GameManager instance is null! Cannot update player lives.");
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        UnityEngine.Debug.Log("[PlayerHealth] Player has died.");
        if (GameManager.Instance != null)
        {
            GameManager.Instance.UpdatePlayerLives(0);
        }
    }

    public void LoseLife()
    {
        if (currentHealth > 0)
        {
            currentHealth--;
            UnityEngine.Debug.Log("[PlayerHealth] Player lost a life. Remaining: " + currentHealth);

            if (GameManager.Instance != null)
            {
                GameManager.Instance.UpdatePlayerLives(currentHealth);
            }
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }
}
