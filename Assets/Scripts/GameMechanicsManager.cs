using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class GameMechanicsManager : MonoBehaviour
{
    public static GameMechanicsManager Instance;
    
    [Header("Player References")]
    public PlayerController playerController;
    public ObstacleSpawner obstacleSpawner;
    public ScoreManager scoreManager;
    
    [Header("Game Mechanics Settings")]
    public float mechanicChangeInterval = 30f;
    public float originalForwardSpeed = 10f;
    
    [Header("UI References")]
    public GameObject gameOverPanel;
    public Text finalScoreText;
    public TMP_Text mechanicNotificationText;
    public float notificationDisplayTime = 3f;
    
    // Цвета для уведомлений
    private readonly Color speedColor = Color.yellow;
    private readonly Color sizeColor = new Color(1f, 0.5f, 0f); // Оранжевый
    private readonly Color gravityColor = new Color(0.5f, 0f, 0.5f); // Фиолетовый
    private readonly Color chaosColor = Color.red;
    private readonly Color resetColor = Color.green;
    
    private float nextChangeTime;
    private bool chaosMode = false;
    private bool isGameOver = false;
    
    private float originalGravity;
    private Vector3 originalPlayerScale;
    private float originalSpeed;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        
        originalGravity = Physics.gravity.y;
        originalPlayerScale = playerController.transform.localScale;
        originalSpeed = playerController.forwardSpeed;
        
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
            
        if (mechanicNotificationText != null)
            mechanicNotificationText.gameObject.SetActive(false);
    }
    
    void Start()
    {
        nextChangeTime = Time.time + mechanicChangeInterval;
        playerController.onPlayerDeath.AddListener(GameOver);
    }
    
    void Update()
    {
        if (isGameOver) return;
        
        if (Time.time > nextChangeTime)
        {
            ChangeMechanics();
            nextChangeTime = Time.time + mechanicChangeInterval;
        }
    }
    
    void ChangeMechanics()
    {
        if (isGameOver) return;
        
        int randomEffect = Random.Range(0, 5);
        string notificationMessage = "";
        Color notificationColor = Color.white;
        
        switch(randomEffect)
        {
            case 0:
                float newGravity = Random.Range(-10f, -30f);
                Physics.gravity = new Vector3(0, newGravity, 0);
                notificationMessage = "ИЗМЕНЕНИЕ ГРАВИТАЦИИ";
                notificationColor = gravityColor;
                break;
                
            case 1:
                float randomSize = Random.Range(0.5f, 3f);
                playerController.transform.localScale = Vector3.one * randomSize;
                notificationMessage = "ИЗМЕНЕНИЕ РАЗМЕРА";
                notificationColor = sizeColor;
                break;
                                
            case 2:
                // Только активируем хаос здесь, уведомление будет в ToggleChaosMode
                if (!chaosMode) // Активируем только если еще не активен
                {
                    ToggleChaosMode();
                }
                return; // Не показываем уведомление здесь
                
            case 3:
                float newSpeed = Random.Range(15f, 20f);
                playerController.forwardSpeed = newSpeed;
                notificationMessage = "ИЗМЕНЕНИЕ СКОРОСТИ";
                notificationColor = speedColor;
                break;
                
            case 4:
                ResetToDefault();
                notificationMessage = "СБРОС ЭФФЕКТОВ";
                notificationColor = resetColor;
                break;
        }
        
        ShowMechanicNotification(notificationMessage, notificationColor);
    }
    
    void ShowMechanicNotification(string message, Color color)
    {
        if (mechanicNotificationText != null)
        {
            // Явно устанавливаем цвет и прозрачность
            color.a = 1f; // Полная непрозрачность
            mechanicNotificationText.color = color;
            
            // Отключаем rich text, если он мешает
            mechanicNotificationText.richText = false;
            
            mechanicNotificationText.text = message;
            mechanicNotificationText.gameObject.SetActive(true);
            StartCoroutine(HideNotificationAfterDelay());
        }
    }
    
    IEnumerator HideNotificationAfterDelay()
    {
        yield return new WaitForSeconds(notificationDisplayTime);
        mechanicNotificationText.gameObject.SetActive(false);
    }

    void ToggleChaosMode()
    {
        chaosMode = !chaosMode;
        
        if (playerController != null)
        {
            playerController.SetControlsInverted(chaosMode);
        }

        // Всегда показываем уведомление при изменении состояния хаоса
        string message = chaosMode ? "РЕЖИМ ХАОСА!" : "РЕЖИМ ХАОСА ОТКЛЮЧЕН";
        Color color = chaosMode ? chaosColor : resetColor;
        ShowMechanicNotification(message, color);

        if (chaosMode)
        {
            Debug.Log("Активирован режим хаоса!");
            // Запланируем отключение через время
            Invoke("ToggleChaosMode", mechanicChangeInterval / 2f);
        }
        else
        {
            Debug.Log("Режим хаоса деактивирован");
            ResetToDefault();
        }
    }
    
    void ResetToDefault()
    {
        Physics.gravity = new Vector3(0, originalGravity, 0);
        playerController.transform.localScale = originalPlayerScale;
        playerController.forwardSpeed = originalSpeed;
    }
    
    public void GameOver()
    {
        if (isGameOver) return;
        
        isGameOver = true;
        
        if (playerController != null)
            playerController.forwardSpeed = 0f;
        
        if (obstacleSpawner != null)
            obstacleSpawner.enabled = false;
        
        if (scoreManager != null)
            scoreManager.StopCounting();
        
        CancelInvoke();
        
        if (mechanicNotificationText != null)
            mechanicNotificationText.gameObject.SetActive(false);
        
        if (scoreManager != null && scoreManager.scoreText != null)
            scoreManager.scoreText.gameObject.SetActive(false);
        
        if (finalScoreText != null)
        {
            finalScoreText.gameObject.SetActive(true);
            finalScoreText.text = "FINAL SCORE:\n" + (scoreManager != null ? Mathf.FloorToInt(scoreManager.GetScore()).ToString() : "0");
        }
        
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
    }
    
    public void RestartGame()
    {
        ResetToDefault();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    public void ReturnToMenu()
    {
        ResetToDefault();
        SceneManager.LoadScene("Menu");
    }
    
    public bool IsGameOver()
    {
        return isGameOver;
    }
    
    public bool IsChaosModeActive()
    {
        return chaosMode;
    }
}