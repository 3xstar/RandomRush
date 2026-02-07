using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player;
    public Vector3 offset;
    public float smoothSpeed = 5f;
    
    void Start()
    {
        // Инициализируем смещение при старте
        offset = transform.position - player.position;
    }
    
    void LateUpdate()
    {
        if (player == null) return;
        
        // Только позиция по Z следует за игроком
        Vector3 targetPosition = new Vector3(
            transform.position.x, 
            transform.position.y, 
            player.position.z + offset.z
        );
        
        transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed * Time.deltaTime);
    }
}