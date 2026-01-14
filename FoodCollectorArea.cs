using UnityEngine;
using Unity.MLAgentsExamples;
using System.Collections;
using System.Collections.Generic;

public class FoodCollectorArea : Area
{
    [Header("Food Settings")]
    public GameObject foodPrefab;
    public GameObject badFoodPrefab;
    public int initialFoodCount = 10;
    public int initialBadFoodCount = 5;

    [Header("Agent Settings")]
    public GameObject agentPrefab;
    public int numAgents;
    public List<GameObject> agents = new List<GameObject>();

    [Header("Respawn Settings")]
    public float respawnInterval = 5f; // Interval for periodic respawns
    public float respawnRadius = 5f; // Radius to check for food proximity
    public float proximityFactor = 0.1f; // Base respawn multiplier

    [Header("Area Settings")]
    public float areaRange = 20f;

    private List<GameObject> foodObjects = new List<GameObject>();
    private List<GameObject> badFoodObjects = new List<GameObject>();
    private Coroutine respawnCoroutine;

    private void OnEnable()
    {
        // Start the periodic respawn coroutine when the object is enabled
        respawnCoroutine = StartCoroutine(PeriodicRespawn());
    }

    private void OnDisable()
    {
        // Stop the coroutine when the object is disabled
        if (respawnCoroutine != null)
        {
            StopCoroutine(respawnCoroutine);
            respawnCoroutine = null;
        }
    }

    public override void ResetArea()
    {
        // Clear all objects
        ClearAllObjects();

        // Respawn agents, food, and bad food
        SpawnAgents();
        SpawnFood();
        SpawnBadFood();
    }


    public void ResetFoodArea()
    {
        // Clear all objects
        ClearAllFood();

        // Respawn agents, food, and bad food
        // SpawnAgents();
        SpawnFood();
        SpawnBadFood();
    }

    private void ClearAllObjects()
    {
        // Destroy all agents
        foreach (GameObject agent in agents)
        {
            Destroy(agent);
        }
        agents.Clear();

        // Destroy all food objects
        foreach (GameObject food in foodObjects)
        {
            Destroy(food);
        }
        foodObjects.Clear();

        foreach (GameObject badFood in badFoodObjects)
        {
            Destroy(badFood);
        }
        badFoodObjects.Clear();


        GameObject[] foodObjectsLeftOver = GameObject.FindGameObjectsWithTag("food");
        GameObject[] badfoodObjectsLeftOver = GameObject.FindGameObjectsWithTag("badFood");
        GameObject[] agentsLeftOver = GameObject.FindGameObjectsWithTag("agent");
        GameObject[] eliminatedAgentsLeftOver = GameObject.FindGameObjectsWithTag("eliminatedAgent");
        GameObject[] frozenAgentsLeftOver = GameObject.FindGameObjectsWithTag("frozenAgent");
        foreach (var food in foodObjectsLeftOver)
        {
            Destroy(food);
        }
        foreach (var food in badfoodObjectsLeftOver)
        {
            Destroy(food);
        }
        foreach (var agent in agentsLeftOver)
        {
            Destroy(agent);
        }
        foreach (var agent in eliminatedAgentsLeftOver)
        {
            Destroy(agent);
        }
        foreach (var agent in frozenAgentsLeftOver)
        {
            Destroy(agent);
        }


    }
    private void ClearAllFood()
    {

        // Destroy all food objects
        foreach (GameObject food in foodObjects)
        {
            Destroy(food);
        }
        foodObjects.Clear();

        foreach (GameObject badFood in badFoodObjects)
        {
            Destroy(badFood);
        }
        badFoodObjects.Clear();

        GameObject[] foodObjectsLeftOver = GameObject.FindGameObjectsWithTag("food");
        GameObject[] badfoodObjectsLeftOver = GameObject.FindGameObjectsWithTag("badFood");
        foreach (var food in foodObjectsLeftOver)
        {
            Destroy(food);
        }
        foreach (var food in badfoodObjectsLeftOver)
        {
            Destroy(food);
        }



    }

    private void SpawnAgents()
    {
        for (int i = 0; i < numAgents; i++)
        {
            Vector3 position = GetRandomPosition();
            Quaternion rotation = Quaternion.Euler(0f, Random.Range(0, 360f), 0f);

            GameObject agent = Instantiate(agentPrefab, position, rotation);
            agents.Add(agent);

            FoodCollectorAgent agentScript = agent.GetComponent<FoodCollectorAgent>();
            if (agentScript != null)
            {
                agentScript.area = gameObject;
                agentScript.OnEpisodeBegin();
            }
        }
    }

    private void SpawnFood()
    {
        for (int i = 0; i < initialFoodCount; i++)
        {
            Vector3 position = GetRandomPosition();
            GameObject food = Instantiate(foodPrefab, position, Quaternion.identity);
            food.GetComponent<FoodLogic>().myArea = this;
            foodObjects.Add(food);
        }
    }

    private void SpawnBadFood()
    {
        for (int i = 0; i < initialBadFoodCount; i++)
        {
            Vector3 position = GetRandomPosition();
            GameObject badFood = Instantiate(badFoodPrefab, position, Quaternion.identity);
            badFood.GetComponent<FoodLogic>().myArea = this;
            badFoodObjects.Add(badFood);
        }
    }

    private Vector3 GetRandomPosition()
    {
        return new Vector3(
            Random.Range(-areaRange, areaRange),
            1f,
            Random.Range(-areaRange, areaRange)
        ) + transform.position;
    }

    public void SpawnFoodAtPosition(Vector3 position)
    {
        GameObject food = Instantiate(foodPrefab, position, Quaternion.identity);
        food.GetComponent<FoodLogic>().myArea = this;
        foodObjects.Add(food);
    }

    private float CalculateRespawnChance(Vector3 position, FoodCollectorArea area)
    {
        int nearbyFoodCount = 0;

        foreach (var food in FindObjectsOfType<FoodLogic>())
        {
            if (Vector3.Distance(position, food.transform.position) <= respawnRadius)
            {
                nearbyFoodCount++;
            }
        }

        // Higher density increases respawn probability
        return Mathf.Clamp01(proximityFactor * nearbyFoodCount);
    }

    private IEnumerator PeriodicRespawn()
    {
        while (true)
        {
            yield return new WaitForSeconds(respawnInterval);

            Vector3 randomPoint = GetRandomPosition();
            if (Random.value < CalculateRespawnChance(randomPoint, this))
            {
                SpawnFoodAtPosition(randomPoint);
            }
        }
    }
}
