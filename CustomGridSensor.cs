// using System.Collections.Generic;
// using UnityEngine;
// using Unity.MLAgents.Sensors;

// [AddComponentMenu("ML Agents/Custom Grid Sensor")]
// public class CustomGridSensorComponent : SensorComponent
// {
//     [Tooltip("Name of the generated sensor.")]
//     public string sensorName = "CustomGridSensor";

//     [Tooltip("The size of each grid cell.")]
//     public Vector3 cellScale = new Vector3(1f, 0.01f, 1f);

//     [Tooltip("The dimensions of the grid (X, Y, Z).")]
//     public Vector3Int gridSize = new Vector3Int(40, 1, 40);

//     [Tooltip("Rotate the grid with the agent.")]
//     public bool rotateWithAgent = true;

//     [Tooltip("GameObject to use as the agent reference.")]
//     public GameObject agentGameObject;

//     [Tooltip("List of detectable tags.")]
//     public List<string> detectableTags = new List<string>();

//     [Tooltip("The layer mask for objects to detect.")]
//     public LayerMask colliderMask;

//     [Tooltip("Compression type for the sensor output.")]
//     public SensorCompressionType compressionType = SensorCompressionType.PNG;

//     [Tooltip("Number of observation stacks.")]
//     [Range(1, 50)]
//     public int observationStacks = 4;

//     [Tooltip("Maximum buffer size for colliders.")]
//     public int maxColliderBufferSize = 500;

//     [Tooltip("Initial buffer size for colliders.")]
//     public int initialColliderBufferSize = 10;

//     private GridSensorBase gridSensor;

//     public override ISensor[] CreateSensors()
//     {
//         // Validate detectable tags
//         if (detectableTags.Count == 0)
//         {
//             Debug.LogError("CustomGridSensorComponent: No detectable tags set. Add tags to the Detectable Tags list.");
//         }

//         // Create the grid perception system
//         var gridPerception = new BoxOverlapChecker(
//             cellScale,
//             gridSize,
//             rotateWithAgent,
//             colliderMask,
//             gameObject,
//             agentGameObject ? agentGameObject : gameObject,
//             detectableTags.ToArray(),
//             initialColliderBufferSize,
//             maxColliderBufferSize
//         );

//         // Create the custom grid sensor
//         gridSensor = new OneHotGridSensor(sensorName, cellScale, gridSize, detectableTags.ToArray(), compressionType)
//         {
//             m_GridPerception = gridPerception
//         };

//         // Stack observations if necessary
//         if (observationStacks > 1)
//         {
//             return new ISensor[] { new StackingSensor(gridSensor, observationStacks) };
//         }

//         return new ISensor[] { gridSensor };
//     }

//     private void OnValidate()
//     {
//         // Ensure Y dimension of the grid is always 1
//         if (gridSize.y != 1)
//         {
//             gridSize.y = 1;
//         }
//     }

//     private void OnDrawGizmos()
//     {
//         if (gridSensor == null)
//         {
//             return;
//         }

//         gridSensor.ResetPerceptionBuffer();
//         gridSensor.m_GridPerception.UpdateGizmo();
//     }
// }
