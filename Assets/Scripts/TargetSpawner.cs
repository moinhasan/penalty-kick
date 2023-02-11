using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetSpawner : MonoBehaviour
{
    [SerializeField] private GameObject targetObject; // Reference of the Target object
    private float minDistance = 1.0f; // Minimum distance between Target objects
    private int minTargetCount = 3;
    private int maxTargetCount = 5;
    private List<Vector3> targetPositions;
    private List<GameObject> targets;

    void Awake()
    {
        GameManager.OnGameStart += OnStartGame; 
        GameManager.OnGameRestart += OnRestartGame;
        GameManager.OnGameOver += OnGameOver; 
    }

    // Start is called before the first frame update
    void Start()
    {
        targets = new List<GameObject>(); // Target object list
    }

    // Spawn Targets on Game Start
    void OnStartGame()
    {
        SpawnTargets();
    }

    /// <summary>
    /// Clear all old targets and Spawn new targets on Game Restart
    /// </summary>
    void OnRestartGame()
    {
        ClearTargets();
        SpawnTargets();
    }

    // Clear all Targets on Game Over
    void OnGameOver(int score)
    {
        ClearTargets();
    }

    /// <summary>
    /// Spawn Targets within the Goal Area
    /// </summary>
    void SpawnTargets()
    {
        int targetCount = Random.Range(GameDataManager.Level.minTargets, GameDataManager.Level.maxTargets+ 1);
        // Now restrict the max number of targets that can be placed inside the goal post.
        targetCount = Mathf.Clamp(targetCount, 0, 8);

        targetPositions = GetTargetPositions(targetCount);
        foreach(Vector3 targetPosition in targetPositions)
        {
            GameObject target = Instantiate(targetObject, gameObject.transform, true);
            target.transform.position = targetPosition;
            target.GetComponent<TargetHitTrigger>().SetPoints(Random.Range(GameDataManager.Level.minTargetPoints, GameDataManager.Level.maxTargetPoints+1));
            targets.Add(target);
        }
    }

    /// <summary>
    /// Clear all Targets and Targets Positions
    /// </summary>
    void ClearTargets()
    {
        foreach (GameObject target in targets)
        {
            Destroy(target);
        }
        targets.Clear();
        targetPositions.Clear();
    }

    /// <summary>
    /// Get Target positions within Goal Area
    /// </summary>
    /// <param name="count"> Number of target positions</param>
    public List<Vector3> GetTargetPositions(int count)
    {
        // Get all vertices of the plane mesh (goal area)
        Vector3[] vertices = gameObject.GetComponent<MeshFilter>().sharedMesh.vertices;

        // Get 4 corner world space positions of the goal area
        List<Vector3> CornerVertexPositions = new List<Vector3>();
        CornerVertexPositions.Add(transform.TransformPoint(vertices[0]));
        CornerVertexPositions.Add(transform.TransformPoint(vertices[10]));
        CornerVertexPositions.Add(transform.TransformPoint(vertices[110]));
        CornerVertexPositions.Add(transform.TransformPoint(vertices[120]));

        // List of Positions to spawn targets
        List<Vector3> positions = new List<Vector3>();

        for (int i = 0; i < count; i++)
        {
            Vector3 position = Vector3.zero;
            bool isValidPosition = false;
            while (!isValidPosition)
            {
                // Get a random position from the area
                float x = Random.Range(CornerVertexPositions[1].x + 0.5f, CornerVertexPositions[2].x - 0.5f); // Keep .5 Distance from the bar
                float y = Random.Range(CornerVertexPositions[1].y + 0.5f, CornerVertexPositions[2].y - 0.5f); // Keep .5 Distance from the bar and ground
                position = new Vector3(x, y, -0.2f); // Place a bit ahead of goal 

                // Check if position is valid. 
                isValidPosition = true;
                for (int j = 0; j < i; j++)
                {
                    // Check if the position is at least distance away from the other points.
                    if (Vector3.Distance(position, positions[j]) < minDistance)
                    {
                        isValidPosition = false;
                        break;
                    }
                }
            }
            positions.Add(position);
        }

        return positions;
    }
}
