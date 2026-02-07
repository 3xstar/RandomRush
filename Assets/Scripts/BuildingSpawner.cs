using UnityEngine;
using System.Collections.Generic;

public class BuildingSpawner : MonoBehaviour
{
    [Header("Building Prefabs")]
    public List<GameObject> leftBuildings;  // Префабы для левой стороны (будут повернуты на 180°)
    public List<GameObject> rightBuildings; // Префабы для правой стороны (оригинальный поворот)

    [Header("Spawn Settings")]
    public float spawnDistance = 30f;       // Дистанция спавна перед игроком
    public float destroyDistance = 20f;    // Дистанция удаления за игроком
    public float minSpawnRate = 3f;       // Минимальная частота спавна
    public float maxSpawnRate = 6f;       // Максимальная частота спавна
    public float leftLanePosition = -8f;  // Позиция левой полосы для зданий (дальше от центра)
    public float rightLanePosition = 8f;  // Позиция правой полосы для зданий (дальше от центра)
    public float minOffset = 0f;          // Минимальное смещение по Z
    public float maxOffset = 5f;          // Максимальное смещение по Z

    [Header("Debug")]
    [SerializeField] private bool debugMode = true;

    private Transform player;
    private float nextSpawnTime;
    private List<GameObject> activeBuildings = new List<GameObject>();
    private bool isGameOver = false;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        
        if (player == null)
        {
            Debug.LogError("Player not found! Make sure player has 'Player' tag.");
            enabled = false;
            return;
        }

        // Подписываемся на событие смерти игрока
        var playerController = player.GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.onPlayerDeath.AddListener(StopSpawning);
        }

        ValidatePrefabs(leftBuildings, "Left buildings");
        ValidatePrefabs(rightBuildings, "Right buildings");

        nextSpawnTime = Time.time + Random.Range(minSpawnRate, maxSpawnRate);
    }

    void StopSpawning()
    {
        isGameOver = true;
        // Очищаем все активные здания
        CleanupBuildings();
    }

    void Update()
    {
        if (GameMechanicsManager.Instance != null && GameMechanicsManager.Instance.IsGameOver()) 
        return;
        
        if (Time.time >= nextSpawnTime)
        {
            SpawnBuildings();
            nextSpawnTime = Time.time + Random.Range(minSpawnRate, maxSpawnRate);
        }

        CleanupBuildings();
    }

    void ValidatePrefabs(List<GameObject> prefabs, string listName)
    {
        if (prefabs == null || prefabs.Count == 0)
        {
            Debug.LogError($"No {listName} prefabs assigned in the inspector!");
            enabled = false;
            return;
        }

        for (int i = 0; i < prefabs.Count; i++)
        {
            if (prefabs[i] == null)
            {
                Debug.LogError($"{listName} prefab at index {i} is null!");
                enabled = false;
                return;
            }
        }
    }

    void SpawnBuildings()
    {
        // Спавним здание слева (с поворотом на 180°)
        if (leftBuildings.Count > 0)
        {
            SpawnSingleBuilding(leftBuildings[Random.Range(0, leftBuildings.Count)], 
                              leftLanePosition, Quaternion.Euler(0, 180, 0));
        }

        // Спавним здание справа (без поворота)
        if (rightBuildings.Count > 0)
        {
            SpawnSingleBuilding(rightBuildings[Random.Range(0, rightBuildings.Count)], 
                              rightLanePosition, Quaternion.identity);
        }
    }

    void SpawnSingleBuilding(GameObject prefab, float xPosition, Quaternion rotation)
    {
        float zOffset = Random.Range(minOffset, maxOffset);
        Vector3 spawnPos = new Vector3(
            xPosition,
            prefab.transform.position.y,
            player.position.z + spawnDistance + zOffset
        );

        GameObject newBuilding = Instantiate(prefab, spawnPos, rotation);
        
        // Добавляем эффект плавного появления
        var fade = newBuilding.GetComponent<ObstacleFadeEffect>();
        if (fade == null) 
        {
            fade = newBuilding.AddComponent<ObstacleFadeEffect>();
        }
        else
        {
            fade.ResetFade();
        }
        
        activeBuildings.Add(newBuilding);

        if (debugMode) Debug.Log($"Spawned building at {xPosition} side");
    }
    
    void CleanupBuildings()
    {
        for (int i = activeBuildings.Count - 1; i >= 0; i--)
        {
            if (activeBuildings[i] == null)
            {
                activeBuildings.RemoveAt(i);
                continue;
            }

            if (activeBuildings[i].transform.position.z < player.position.z - destroyDistance)
            {
                Destroy(activeBuildings[i]);
                activeBuildings.RemoveAt(i);
                
                if (debugMode) Debug.Log("Destroyed building behind player");
            }
        }
    }

    void OnDestroy()
    {
        foreach (var building in activeBuildings)
        {
            if (building != null) Destroy(building);
        }
        activeBuildings.Clear();
    }
}