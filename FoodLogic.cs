using UnityEngine;

public class FoodLogic : MonoBehaviour
{
    public FoodCollectorArea myArea;

    public void OnEaten()
    {
        if (myArea == null)
        {
            Debug.LogError("FoodLogic: myArea is not assigned.");
            return;
        }

        // Notify the area about food consumption (optional, can track stats)
        Destroy(gameObject); // Destroy the eaten food
    }
}
