using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public class FoodCollectorAgent : Agent
{
    public AgentSettings agentSettings; // Reference to the ScriptableObject

    FoodCollectorSettings m_FoodCollecterSettings;
    public GameObject area;
    FoodCollectorArea m_MyArea;
    bool m_Frozen;
    bool m_PermanentlyEliminated; // Tracks permanent elimination status
    float m_FrozenTime;
    float m_EffectTime;
    Rigidbody m_AgentRb;
    float m_LaserLength;
    public float turnSpeed = 300;
    public float moveSpeed = 2;
    public Material normalMaterial;
    public Material badMaterial;
    public Material goodMaterial;
    public Material frozenMaterial;
    public Material eliminatedMaterial;
    public GameObject myLaser;
    public bool contribute;
    public bool useVectorObs;
    public bool useVectorFrozenFlag;

    public bool permanentlyEliminates;
    public int agentsToFreezeThreshold = 1;
    public int agentsToEliminateThreshold = 3;
    public float laserCooldown = 1.0f; // Cooldown in seconds
    public float rewardForZapping = -1f; // Reward for zapping another agent
    public float rewardForBeingZapped = -5f; // Penalty for being zapped
    public float rewardForFiring = -0.5f; // Penalty or reward for firing a zap
    private float lastFiredTime = -1.0f; // Tracks last time the laser was fired

    private static Dictionary<FoodCollectorAgent, HashSet<FoodCollectorAgent>> hitTracker = new Dictionary<FoodCollectorAgent, HashSet<FoodCollectorAgent>>();
    EnvironmentParameters m_ResetParams;

    public override void Initialize()
    {
        m_AgentRb = GetComponent<Rigidbody>();
        m_MyArea = area.GetComponent<FoodCollectorArea>();
        m_FoodCollecterSettings = FindObjectOfType<FoodCollectorSettings>();
        m_ResetParams = Academy.Instance.EnvironmentParameters;

        // Use the ScriptableObject for default values
        rewardForZapping = agentSettings.rewardForZapping;
        rewardForBeingZapped = agentSettings.rewardForBeingZapped;
        rewardForFiring = agentSettings.rewardForFiring;
        agentsToFreezeThreshold = agentSettings.agentsToFreezeThreshold;
        agentsToEliminateThreshold = agentSettings.agentsToEliminateThreshold;
        laserCooldown = agentSettings.laserCooldown;

        SetResetParameters();
    }


    public override void CollectObservations(VectorSensor sensor)
    {
        if (useVectorObs)
        {
            // Normalize velocity
            Vector3 localVelocity = transform.InverseTransformDirection(m_AgentRb.velocity) / 10f; // Assuming max velocity is ~10
            sensor.AddObservation(localVelocity.x);
            sensor.AddObservation(localVelocity.z);
            sensor.AddObservation(m_Frozen ? 1.0f : 0.0f);

            // Add normalized cooldown
            float cooldownNormalized = Mathf.Clamp01((Time.time - lastFiredTime) / laserCooldown);
            sensor.AddObservation(cooldownNormalized);

            // Add distances to nearest objects
            GameObject closestFood = FindClosestObjectWithTag("food");
            GameObject closestAgent = FindClosestAgent();

            float distanceToNearestFood = closestFood ? Vector3.Distance(closestFood.transform.position, transform.position) / m_MyArea.areaRange : 1.0f;
            float distanceToNearestAgent = closestAgent ? Vector3.Distance(closestAgent.transform.position, transform.position) / m_MyArea.areaRange : 1.0f;

            sensor.AddObservation(distanceToNearestFood);
            sensor.AddObservation(distanceToNearestAgent);

            // Add angular information to nearest food
            if (closestFood)
            {
                Vector3 directionToFood = (closestFood.transform.position - transform.position).normalized;
                float angleToFood = Vector3.Angle(transform.forward, directionToFood) / 180f; // Normalize to [0, 1]
                sensor.AddObservation(angleToFood);
            }
            else
            {
                sensor.AddObservation(1.0f); // No food in range
            }

            // Add relative positions and states of other agents
            var agents = FindObjectsOfType<FoodCollectorAgent>();
            foreach (var agent in agents)
            {
                if (agent == this) continue; // Skip self

                // Relative position
                Vector3 relativePosition = (agent.transform.position - transform.position) / m_MyArea.areaRange;
                sensor.AddObservation(relativePosition.x);
                sensor.AddObservation(relativePosition.y);
                sensor.AddObservation(relativePosition.z);

                // Agent state
                sensor.AddObservation(agent.m_Frozen ? 1.0f : 0.0f);
                sensor.AddObservation(agent.m_PermanentlyEliminated ? 1.0f : 0.0f);

                // Times hit
                if (hitTracker.ContainsKey(agent))
                {
                    sensor.AddObservation(hitTracker[agent].Count / 5f); // Normalize hits assuming max ~10
                }
                else
                {
                    sensor.AddObservation(0.0f);
                }
            }
        }
        else if (useVectorFrozenFlag)
        {
            sensor.AddObservation(m_Frozen);
        }
    }

    GameObject FindClosestObjectWithTag(string tag)
    {
        GameObject[] objects = GameObject.FindGameObjectsWithTag(tag);
        GameObject closest = null;
        float minDistance = float.MaxValue;

        foreach (GameObject obj in objects)
        {
            float distance = Vector3.Distance(transform.position, obj.transform.position);
            if (distance < minDistance)
            {
                closest = obj;
                minDistance = distance;
            }
        }

        return closest;
    }

    GameObject FindClosestAgent()
    {
        GameObject closest = null;
        float minDistance = float.MaxValue;

        foreach (var agent in FindObjectsOfType<FoodCollectorAgent>())
        {
            if (agent == this) continue; // Skip self

            float distance = Vector3.Distance(transform.position, agent.transform.position);
            if (distance < minDistance)
            {
                closest = agent.gameObject;
                minDistance = distance;
            }
        }

        return closest;
    }

    public void MoveAgent(ActionBuffers actionBuffers)
    {
        if (m_PermanentlyEliminated)
        {
            return; // Eliminated agents can't move or shoot.
        }
        if (Time.time > m_FrozenTime + 4f && m_Frozen)
        {
            Unfreeze();
        }

        var dirToGo = Vector3.zero;
        var rotateDir = Vector3.zero;

        var continuousActions = actionBuffers.ContinuousActions;
        var discreteActions = actionBuffers.DiscreteActions;

        if (!m_Frozen)
        {
            var forward = Mathf.Clamp(continuousActions[0], -1f, 1f);
            var right = Mathf.Clamp(continuousActions[1], -1f, 1f);
            var rotate = Mathf.Clamp(continuousActions[2], -1f, 1f);

            dirToGo = transform.forward * forward;
            dirToGo += transform.right * right;
            rotateDir = -transform.up * rotate;

            if (discreteActions[0] > 0 && Time.time > lastFiredTime + laserCooldown)
            {
                FireLaser();
                lastFiredTime = Time.time; // Update last fired time
            }

            m_AgentRb.AddForce(dirToGo * moveSpeed, ForceMode.VelocityChange);
            transform.Rotate(rotateDir, Time.fixedDeltaTime * turnSpeed);
        }

        if (m_AgentRb.velocity.sqrMagnitude > 25f) // slow it down
        {
            m_AgentRb.velocity *= 0.95f;
        }
    }

    private void FireLaser()
{
    // Activate the beam
    myLaser.transform.localScale = new Vector3(1f, 1f, m_LaserLength);

    RaycastHit hit;
    if (Physics.SphereCast(transform.position, 2f, transform.forward, out hit, 25f))
    {
        if (hit.collider.gameObject.CompareTag("agent"))
        {
            var targetAgent = hit.collider.gameObject.GetComponent<FoodCollectorAgent>();
            TrackHit(targetAgent, this);
            AddReward(rewardForZapping); // Reward for zapping another agent
            targetAgent.AddReward(rewardForBeingZapped); // Penalty for being zapped
        }
    }

    // Reset beam after a short delay
    Invoke(nameof(ResetLaserVisual), 0.1f);
}

    void ResetLaserVisual()
    {
        myLaser.transform.localScale = new Vector3(0f, 0f, 0f);
    }

    void TrackHit(FoodCollectorAgent targetAgent, FoodCollectorAgent shooter)
    {
        if (!hitTracker.ContainsKey(targetAgent))
        {
            hitTracker[targetAgent] = new HashSet<FoodCollectorAgent>();
        }

        if (!hitTracker[targetAgent].Contains(shooter))
        {
            hitTracker[targetAgent].Add(shooter);
            CheckAgentStatus(targetAgent);
        }
    }

    void CheckAgentStatus(FoodCollectorAgent agent)
    {
        int uniqueHitCount = hitTracker[agent].Count;

        if (uniqueHitCount >= agentsToEliminateThreshold && permanentlyEliminates)
        {
            agent.PermanentlyEliminate();
        }
        else if (uniqueHitCount >= agentsToFreezeThreshold)
        {
            agent.Freeze();
        }
    }

    void Freeze()
    {
        gameObject.tag = "frozenAgent";
        m_Frozen = true;
        m_FrozenTime = Time.time;
        gameObject.GetComponentInChildren<Renderer>().material = frozenMaterial;
    }

    void Unfreeze()
    {
        m_Frozen = false;
        gameObject.tag = "agent";
        gameObject.GetComponentInChildren<Renderer>().material = normalMaterial;
    }

    void PermanentlyEliminate()
    {
        if (!m_PermanentlyEliminated)
        {
            m_PermanentlyEliminated = true;
            gameObject.GetComponentInChildren<Renderer>().material = eliminatedMaterial;
            gameObject.tag = "eliminatedAgent";
            m_AgentRb.velocity = Vector3.zero;
            m_AgentRb.isKinematic = true;
        }
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        MoveAgent(actionBuffers);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        if (Input.GetKey(KeyCode.D))
        {
            continuousActionsOut[2] = 1;
        }
        if (Input.GetKey(KeyCode.W))
        {
            continuousActionsOut[0] = 1;
        }
        if (Input.GetKey(KeyCode.A))
        {
            continuousActionsOut[2] = -1;
        }
        if (Input.GetKey(KeyCode.S))
        {
            continuousActionsOut[0] = -1;
        }
        var discreteActionsOut = actionsOut.DiscreteActions;
        discreteActionsOut[0] = Input.GetKey(KeyCode.Space) ? 1 : 0;
    }

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

    //         // Call a reset method on the agent itself
    //         var agentScript = agent.GetComponent<FoodCollectorAgent>();
    //         if (agentScript != null)
    //         {
    //             agentScript.ResetEliminationState(); // Delegate resetting to the agent
    //         }
    //     }
    // }


public void ResetAgentState()
    {
        if (this.CompareTag("eliminatedAgent"))
        {
            this.tag = "agent"; // Reset to the default agent tag

            var agentRb = this.GetComponent<Rigidbody>();
            if (agentRb != null)
            {
                agentRb.isKinematic = false; // Ensure the agent can move again
            }

            m_PermanentlyEliminated = false; // Reset elimination status
            m_Frozen = false; // Reset frozen status

            
        }
    }





    public static void ClearHitTracker()
    {
        hitTracker.Clear();
    }
public override void OnEpisodeBegin()
{
    // Reset agent's own state
    ResetAgentState();
    m_AgentRb.velocity = Vector3.zero;
    Unfreeze();
    

    // Ensure the agent's position is slightly above the grid
    transform.position = new Vector3(
        Random.Range(-m_MyArea.areaRange, m_MyArea.areaRange),
        0.5f, // Slightly above the grid
        Random.Range(-m_MyArea.areaRange, m_MyArea.areaRange)
    ) + m_MyArea.transform.position;

    transform.rotation = Quaternion.Euler(0f, Random.Range(0, 360f), 0f);

    // Reset the beam to an inactive state
    myLaser.transform.localScale = new Vector3(0f, 0f, 0f);

    SetResetParameters();
}






    void OnCollisionEnter(Collision collision)
    {
        if (m_Frozen || m_PermanentlyEliminated) return; // Prevent interaction if frozen/eliminated

        if (collision.gameObject.CompareTag("food"))
        {
            collision.gameObject.GetComponent<FoodLogic>().OnEaten();
            AddReward(1f);
            if (contribute)
            {
                m_FoodCollecterSettings.totalScore += 1;
            }
        }
        if (collision.gameObject.CompareTag("badFood"))
        {
            collision.gameObject.GetComponent<FoodLogic>().OnEaten();
            AddReward(-1f);
            if (contribute)
            {
                m_FoodCollecterSettings.totalScore -= 1;
            }
        }
    }

    public void SetLaserLengths()
    {
        m_LaserLength = m_ResetParams.GetWithDefault("laser_length", 1.0f);
    }

    public void SetAgentScale()
    {
        float agentScale = m_ResetParams.GetWithDefault("agent_scale", 1.0f);
        gameObject.transform.localScale = new Vector3(agentScale, agentScale, agentScale);
    }

    public void SetResetParameters()
    {
        SetLaserLengths();
        SetAgentScale();
    }
}