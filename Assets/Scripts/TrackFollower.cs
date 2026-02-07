using UnityEngine;

public class TrackFollower : MonoBehaviour
{
    [Header("Track Settings")]
    public Transform player;                  // Ссылка на игрока
    public float trackSegmentLength = 50f;    // Длина одного сегмента дороги
    public float activationDistance = 30f;    // Дистанция активации перемещения
    
    private Vector3 initialPosition;          // Начальная позиция дороги
    private float lastSegmentZ;               // Z-координата последнего сегмента
    private float nextActivationZ;            // Z-точка активации следующего перемещения

    void Start()
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
            if (player == null)
                Debug.LogError("Player reference not set in TrackFollower!");
        }

        initialPosition = transform.position;
        lastSegmentZ = initialPosition.z;
        nextActivationZ = lastSegmentZ + activationDistance;
    }

    void Update()
    {
        // Если игрок достиг точки активации
        if (player.position.z > nextActivationZ)
        {
            MoveTrack();
        }
    }

    void MoveTrack()
    {
        // Вычисляем сколько сегментов нужно переместить
        float segmentsToMove = Mathf.Floor((player.position.z - lastSegmentZ) / trackSegmentLength);
        
        if (segmentsToMove > 0)
        {
            // Перемещаем дорогу вперед
            float moveDistance = segmentsToMove * trackSegmentLength;
            transform.position += Vector3.forward * moveDistance;
            
            // Обновляем контрольные точки
            lastSegmentZ += moveDistance;
            nextActivationZ = lastSegmentZ + activationDistance;
            
            Debug.Log($"Moved track by {moveDistance} units. Next activation at Z: {nextActivationZ}");
        }
    }

    // Для визуализации в редакторе
    void OnDrawGizmosSelected()
    {
        if (player != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(
                new Vector3(-10, 0, nextActivationZ),
                new Vector3(10, 0, nextActivationZ)
            );
            Gizmos.DrawSphere(new Vector3(0, 1, nextActivationZ), 0.5f);
        }
    }
}