// using UnityEngine;
// using System.Collections;
// using System.Collections.Generic;

// public class FoodRespawner : MonoBehaviour
// {
//     public static FoodRespawner Instance { get; private set; }

//     [Header("Respawn Settings")]
//     public float respawnInterval = 5f; // Interval for periodic respawns
//     public float respawnRadius = 5f; // Radius to check for food proximity
//     public float proximityFactor = 0.1f; // Base respawn multiplier

//     private List<FoodCollectorArea> areas = new List<FoodCollectorArea>();

//     private void Awake()
//     {
//         if (Instance != null && Instance != this)
//         {
//             Destroy(this);
//         }
//         else
//         {
//             Instance = this;
//         }
//     }

//     private void Start()
//     {
//         areas.AddRange(FindObjectsOfType<FoodCollectorArea>());
//         StartCoroutine(PeriodicRespawn());
//     }

//     public void ResetRespawnLogic()
//     {
//         StopAllCoroutines();

//         // Refresh the list of areas to ensure synchronization with episode resets
//         areas.Clear();
//         areas.AddRange(FindObjectsOfType<FoodCollectorArea>());

//         StartCoroutine(PeriodicRespawn());
//     }

//     private IEnumerator PeriodicRespawn()
//     {
//         while (true)
//         {
//             yield return new WaitForSeconds(respawnInterval);

//             foreach (var area in areas)
//             {
//                 Vector3 randomPoint = GetRandomPositionInArea(area);
//                 if (Random.value < CalculateRespawnChance(randomPoint, area))
//                 {
//                     area.SpawnFoodAtPosition(randomPoint);
//                 }
//             }
//         }
//     }

//     private float CalculateRespawnChance(Vector3 position, FoodCollectorArea area)
//     {
//         int nearbyFoodCount = 0;

//         foreach (var food in FindObjectsOfType<FoodLogic>())
//         {
//             if (Vector3.Distance(position, food.transform.position) <= respawnRadius)
//             {
//                 nearbyFoodCount++;
//             }
//         }

//         // Higher density increases respawn probability
//         return Mathf.Clamp01(proximityFactor * nearbyFoodCount);
//     }

//     private Vector3 GetRandomPositionInArea(FoodCollectorArea area)
//     {
//         float radius = area.areaRange;
//         return new Vector3(
//             Random.Range(-radius, radius),
//             area.transform.position.y + 0.5f, // Ensure Y is slightly above the grid
//             Random.Range(-radius, radius)
//         ) + area.transform.position;
//     }
// }
