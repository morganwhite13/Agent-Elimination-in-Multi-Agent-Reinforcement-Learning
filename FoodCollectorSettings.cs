using UnityEngine;
using UnityEngine.UI;
using Unity.MLAgents;

public class FoodCollectorSettings : MonoBehaviour
{
    [HideInInspector]
    public GameObject[] agents;
    [HideInInspector]
    public FoodCollectorArea[] listArea;

    public int totalScore;
    public Text scoreText;

    private StatsRecorder m_Recorder;

    public void Awake()
    {
        Academy.Instance.OnEnvironmentReset += EnvironmentReset;
        m_Recorder = Academy.Instance.StatsRecorder;
    }

    private void Start()
    {
        // Get all FoodCollectorAreas in the scene
        listArea = FindObjectsOfType<FoodCollectorArea>();
        ResetAllAreas();
    }

    private void Update()
    {
        scoreText.text = $"Score: {totalScore}";

        if ((Time.frameCount % 100) == 0)
        {
            m_Recorder.Add("TotalScore", totalScore);
        }
    }

    private void EnvironmentReset()
    {
        ResetAllAreas();
        agents = GetAllAgents();
        totalScore = 0;
    }

    private void ResetAllAreas()
{
    // ClearObjects(GameObject.FindGameObjectsWithTag("food"));
    // ClearObjects(GameObject.FindGameObjectsWithTag("badFood"));

    
    foreach (var area in listArea)
    {
        area.ResetArea(); // Reset the area
    }

    // // Notify all FoodResetAgents to reset food
    // var foodResetAgents = FindObjectsOfType<FoodResetAgent>();
    // foreach (var agent in foodResetAgents)
    // {
    //     agent.OnEpisodeBegin();
    // }

    // Reset the FoodRespawner logic
    // FoodRespawner.Instance?.ResetRespawnLogic();
}


    private GameObject[] GetAllAgents()
    {
        var allAgents = new System.Collections.Generic.List<GameObject>();
        foreach (var area in listArea)
        {
            allAgents.AddRange(area.agents);
        }
        return allAgents.ToArray();
    }
}
