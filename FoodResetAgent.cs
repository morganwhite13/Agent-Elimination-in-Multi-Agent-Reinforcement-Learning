using UnityEngine;
using Unity.MLAgents;

public class FoodResetAgent : Agent
{
    public FoodCollectorArea area; // Reference to the associated area

    public override void OnEpisodeBegin()
    {
        if (area != null)
        {
            Debug.Log("Resetting food for new episode...");
            area.ResetArea(); // Trigger food reset for the associated area
        }
        else
        {
            Debug.LogWarning("FoodResetAgent: No associated FoodCollectorArea found.");
        }
    }
}
