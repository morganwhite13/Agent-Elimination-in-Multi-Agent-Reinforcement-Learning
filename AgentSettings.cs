using UnityEngine;

[CreateAssetMenu(fileName = "AgentSettings", menuName = "ScriptableObjects/AgentSettings", order = 1)]
public class AgentSettings : ScriptableObject
{
    [Header("Rewards and Penalties")]
    public float rewardForZapping = 0.5f;
    public float rewardForBeingZapped = -0.5f;
    public float rewardForFiring = -0.1f;

    [Header("Thresholds")]
    public int agentsToFreezeThreshold = 1;
    public int agentsToEliminateThreshold = 3;

    [Header("Cooldown and Effects")]
    public float laserCooldown = 1.0f;
    public float frozenTime = 4.0f; // Time an agent stays frozen

    [Header("Agent Behavior")]
    public bool permanentlyEliminates = false; // Whether zapping permanently eliminates agents

    [Header("Observation Settings")]
    public bool useVectorObs = true; // Whether to use vector observations
    public bool useVectorFrozenFlag = false; // Whether to use only the frozen flag in vector observations


    
    // public void ResetAgentState(GameObject agent)
    // {
    //     if (agent.CompareTag("eliminatedAgent"))
    //     {
    //         agent.tag = "agent"; // Reset to the default agent tag

    //         var agentRb = agent.GetComponent<Rigidbody>();
    //         if (agentRb != null)
    //         {
    //             agentRb.isKinematic = false; // Ensure the agent can move again
    //         }

    //         var agentScript = agent.GetComponent<FoodCollectorAgent>();
    //         if (agentScript != null)
    //         {
    //             // Reset material through FoodCollectorAgent's normalMaterial
    //             var renderer = agent.GetComponentInChildren<Renderer>();
    //             if (renderer != null)
    //             {
    //                 renderer.material = agentScript.normalMaterial;
    //             }
    //         }
    //     }
    // }



}
