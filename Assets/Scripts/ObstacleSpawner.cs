using UnityEngine;
using System.Collections.Generic;

public class ObstacleSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject[] obstaclePrefabs;
    public float spawnRate = 2f;
    public float spawnDistance = 20f;
    public float destroyDistance = 10f;
    public int minObstaclesPerSpawn = 1; // Минимальное количество препятствий
    public int maxObstaclesPerSpawn = 3; // Максимальное количество препятствий
    private readonly float[] lanePositions = { -2.5f, 0f, 2.5f };
    
    [Header("Debug")]
    [SerializeField] private bool debugMode = true;
    
    private Transform player;
    private float nextSpawnTime;
    private List<GameObject> activeObstacles = new List<GameObject>();

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        
        if (player == null)
        {
            Debug.LogError("Player not found! Make sure player has 'Player' tag.");
            enabled = false;
            return;
        }

        if (obstaclePrefabs == null || obstaclePrefabs.Length == 0)
        {
            Debug.LogError("No obstacle prefabs assigned in the inspector!");
            enabled = false;
            return;
        }

        for (int i = 0; i < obstaclePrefabs.Length; i++)
        {
            if (obstaclePrefabs[i] == null)
            {
                Debug.LogError($"Obstacle prefab at index {i} is null!");
                enabled = false;
                return;
            }
        }
    }

    void Update()
    {
        if (Time.time >= nextSpawnTime)
        {
            SpawnObstacleGroup();
            nextSpawnTime = Time.time + spawnRate;
        }

        CleanupObstacles();
    }

    void SpawnObstacleGroup()
    {
        int obstaclesToSpawn = Random.Range(minObstaclesPerSpawn, maxObstaclesPerSpawn + 1);
        List<int> availableLanes = new List<int> { 0, 1, 2 }; // Все доступные дорожки

        for (int i = 0; i < obstaclesToSpawn; i++)
        {
            if (availableLanes.Count == 0) break; // Если дорожки закончились

            // Выбираем случайную дорожку из доступных
            int laneIndex = Random.Range(0, availableLanes.Count);
            int selectedLane = availableLanes[laneIndex];
            availableLanes.RemoveAt(laneIndex); // Удаляем выбранную дорожку

            SpawnSingleObstacle(selectedLane);
        }
    }

    void SpawnSingleObstacle(int laneIndex)
    {
        int obstacleIndex = Random.Range(0, obstaclePrefabs.Length);
        
        Vector3 spawnPos = new Vector3(
            lanePositions[laneIndex],
            obstaclePrefabs[obstacleIndex].transform.position.y,
            player.position.z + spawnDistance
        );

        GameObject newObstacle = Instantiate(
            obstaclePrefabs[obstacleIndex], 
            spawnPos, 
            Quaternion.identity
        );
        
        // Добавляем или получаем компонент эффекта
        var fade = newObstacle.GetComponent<ObstacleFadeEffect>();
        if (fade == null) 
        {
            fade = newObstacle.AddComponent<ObstacleFadeEffect>();
        }
        else
        {
            fade.ResetFade();
        }
        
        activeObstacles.Add(newObstacle);

        if (debugMode) Debug.Log($"Spawned {obstaclePrefabs[obstacleIndex].name} at lane {laneIndex}");
    }
    
    void CleanupObstacles()
    {
        for (int i = activeObstacles.Count - 1; i >= 0; i--)
        {
            if (activeObstacles[i] == null)
            {
                activeObstacles.RemoveAt(i);
                continue;
            }

            if (activeObstacles[i].transform.position.z < player.position.z - destroyDistance)
            {
                Destroy(activeObstacles[i]);
                activeObstacles.RemoveAt(i);
                
                if (debugMode) Debug.Log("Destroyed obstacle behind player");
            }
        }
    }

    void OnDestroy()
    {
        foreach (var obstacle in activeObstacles)
        {
            if (obstacle != null) Destroy(obstacle);
        }
        activeObstacles.Clear();
    }
}