using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [Header("Настройки звука")]
    public Toggle soundToggle; // Перетащите сюда ваш Toggle из Hierarchy

    [Header("Ссылки")]
    public GameObject settingsPanel;

    [Header("Настройки времени суток")]
    public Toggle dayNightToggle; // Перетащите сюда Toggle из Hierarchy
    private const string DayNightKey = "DayNightMode"; // Ключ для сохранения

    void Start()
    {
        // Инициализация переключателя звука (существующий код)
        if (soundToggle != null)
        {
            soundToggle.onValueChanged.AddListener(OnSoundToggleChanged);
        }

        // Инициализация переключателя дня/ночи
        if (dayNightToggle != null)
        {
            dayNightToggle.onValueChanged.AddListener(OnDayNightToggleChanged);
        }
    }

    private void OnDayNightToggleChanged(bool isDay)
    {
        // Сохраняем выбор: true - день, false - ночь
        PlayerPrefs.SetInt(DayNightKey, isDay ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void OnPlayButton()
    {
        // Определяем какую сцену загружать
        bool isDayMode = PlayerPrefs.GetInt(DayNightKey, 1) == 1;
        SceneManager.LoadScene(isDayMode ? "Game" : "NightGame");
    }
    
    private void OnSoundToggleChanged(bool isOn)
    {
        // Вызываем статический метод MusicManager
        MusicManager.SetSoundEnabled(isOn);
    }

    public void OnSettingsButton()
    {
        settingsPanel.SetActive(true);
    }

    public void OnBackButton()
    {
        settingsPanel.SetActive(false);
    }

    public void OnQuitButton()
    {
        Application.Quit();
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}