using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;
    public static bool SoundEnabled { get; private set; } = true;

    [Header("Настройки")]
    public AudioClip[] musicTracks;
    public AudioMixerGroup musicMixerGroup;
    
    private AudioSource audioSource;
    private List<AudioClip> remainingTracks = new List<AudioClip>();
    private AudioClip currentTrack;
    private bool wasManuallyPaused = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Initialize();
            SceneManager.sceneLoaded += OnSceneLoaded; // Добавляем подписку на событие загрузки сцены
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Initialize()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.outputAudioMixerGroup = musicMixerGroup;
        audioSource.loop = false;
        audioSource.volume = 0.7f;
        
        // Загружаем сохраненные настройки звука
        SoundEnabled = PlayerPrefs.GetInt("SoundEnabled", 1) == 1;
        audioSource.mute = !SoundEnabled;
        
        // Инициализируем плейлист
        remainingTracks = new List<AudioClip>(musicTracks);
        ShuffleTracks();
        
        // Запускаем музыку если она включена
        if (SoundEnabled)
        {
            PlayNextTrack();
        }
    }

    public static void SetSoundEnabled(bool enabled)
    {
        SoundEnabled = enabled;
        PlayerPrefs.SetInt("SoundEnabled", enabled ? 1 : 0);
        PlayerPrefs.Save();
        
        if (Instance != null)
        {
            Instance.audioSource.mute = !enabled;
            Instance.wasManuallyPaused = !enabled;
            
            if (enabled && !Instance.audioSource.isPlaying)
            {
                Instance.PlayNextTrack();
            }
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Воспроизводим музыку только если:
        // 1. Звук включен в настройках
        // 2. Музыка не играет
        // 3. Это не было ручное отключение
        if (SoundEnabled && !audioSource.isPlaying && !wasManuallyPaused)
        {
            PlayNextTrack();
        }
    }

    void Update()
    {
        // Автоматическое переключение треков
        if (SoundEnabled && !audioSource.isPlaying && !wasManuallyPaused)
        {
            if (remainingTracks.Count > 0)
            {
                PlayNextTrack();
            }
            else
            {
                ResetPlaylist();
            }
        }
    }

    void ResetPlaylist()
    {
        remainingTracks = new List<AudioClip>(musicTracks);
        ShuffleTracks();
        
        if (SoundEnabled && !wasManuallyPaused)
        {
            PlayNextTrack();
        }
    }

    void ShuffleTracks()
    {
        // Алгоритм Фишера-Йетса для перемешивания
        for (int i = remainingTracks.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            AudioClip temp = remainingTracks[i];
            remainingTracks[i] = remainingTracks[randomIndex];
            remainingTracks[randomIndex] = temp;
        }
    }

    void PlayNextTrack()
    {
        if (remainingTracks.Count == 0 || !SoundEnabled || wasManuallyPaused) 
            return;
        
        currentTrack = remainingTracks[0];
        remainingTracks.RemoveAt(0);
        
        audioSource.clip = currentTrack;
        audioSource.Play();
        
        Debug.Log($"Now playing: {currentTrack.name} | Tracks left: {remainingTracks.Count}");
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    public void PauseMusic()
    {
        wasManuallyPaused = true;
        audioSource.Pause();
    }

    public void ResumeMusic()
    {
        wasManuallyPaused = false;
        audioSource.UnPause();
    }

    public void StopMusic()
    {
        wasManuallyPaused = true;
        audioSource.Stop();
    }
}