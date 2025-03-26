using UnityEngine;

public class SyrupBottle : MonoBehaviour
{
    public string syrupType; // Set this in the Inspector for each bottle

    public void TryAddSyrup(PlayerInteraction player)
    {
        if (player.heldObjectRight != null && player.heldObjectRight.GetComponent<CoffeeInteraction>())
        {
            CoffeeInteraction coffee = player.heldObjectRight.GetComponent<CoffeeInteraction>();
            coffee.AddSyrup(syrupType);
            Debug.Log($"Added {syrupType} syrup to coffee!");
        }
        else if (player.heldObjectLeft != null && player.heldObjectLeft.GetComponent<CoffeeInteraction>())
        {
            CoffeeInteraction coffee = player.heldObjectLeft.GetComponent<CoffeeInteraction>();
            coffee.AddSyrup(syrupType);
            Debug.Log($"Added {syrupType} syrup to coffee!");
        }
        else
        {
            Debug.Log("You must be holding a coffee to use syrup!");
        }
    }
}