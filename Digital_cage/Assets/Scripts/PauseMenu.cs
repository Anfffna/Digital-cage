using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class PauseMenu : MonoBehaviour
{
    [Header("UI References")]
    public GameObject pauseMenuCanvas;

    [Header("Post Processing")]
    public PostProcessVolume blurVolume;

    [Header("Cursor")]
    public CursorUI cursorManager;

    [Header("Settings")]
    public float blurFadeSpeed = 8f;
    public float menuFadeSpeed = 8f;

    // ДОБАВИМ ССЫЛКУ НА ПАНЕЛЬ НАСТРОЕК
    [Header("Settings Menu")]
    public GameObject settingsPanel;

    public static bool IsGamePaused { get; private set; } = false;

    private CanvasGroup menuCanvasGroup;

    void Start()
    {
        // Изначально скрываем меню
        if (pauseMenuCanvas != null)
        {
            pauseMenuCanvas.SetActive(true);
            menuCanvasGroup = pauseMenuCanvas.GetComponent<CanvasGroup>();
            if (menuCanvasGroup == null)
                menuCanvasGroup = pauseMenuCanvas.AddComponent<CanvasGroup>();

            menuCanvasGroup.alpha = 0f;
            menuCanvasGroup.interactable = false;
            menuCanvasGroup.blocksRaycasts = false;
        }

        if (blurVolume != null)
        {
            blurVolume.weight = 0f;
        }

        // Закрываем панель настроек при старте
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // ЕСЛИ ОТКРЫТЫ НАСТРОЙКИ - ЗАКРЫВАЕМ ИХ
            if (settingsPanel != null && settingsPanel.activeInHierarchy)
            {
                CloseSettings();
            }
            else
            {
                if (IsGamePaused)
                    ResumeGame();
                else
                    PauseGame();
            }
        }

        UpdatePauseEffects();
    }

    void UpdatePauseEffects()
    {
        if (blurVolume != null)
        {
            float targetWeight = IsGamePaused ? 1f : 0f;
            blurVolume.weight = Mathf.Lerp(blurVolume.weight, targetWeight,
                Time.unscaledDeltaTime * blurFadeSpeed);

            if (!IsGamePaused && blurVolume.weight < 0.01f)
            {
                blurVolume.weight = 0f;
            }
        }

        if (menuCanvasGroup != null)
        {
            float targetAlpha = IsGamePaused ? 1f : 0f;
            menuCanvasGroup.alpha = Mathf.Lerp(menuCanvasGroup.alpha, targetAlpha,
                Time.unscaledDeltaTime * menuFadeSpeed);

            if (IsGamePaused && menuCanvasGroup.alpha > 0.1f)
            {
                menuCanvasGroup.blocksRaycasts = true;
                menuCanvasGroup.interactable = true;
            }
            else if (!IsGamePaused && menuCanvasGroup.alpha < 0.1f)
            {
                menuCanvasGroup.blocksRaycasts = false;
                menuCanvasGroup.interactable = false;
            }
        }
    }

    public void PauseGame()
    {
        IsGamePaused = true;
        Time.timeScale = 0f;

        if (menuCanvasGroup != null)
        {
            menuCanvasGroup.blocksRaycasts = true;
            menuCanvasGroup.interactable = true;
        }

        if (cursorManager != null)
        {
            cursorManager.ShowCursor();
        }
        else
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        Debug.Log("Игра на паузе!");
    }

    public void ResumeGame()
    {
        IsGamePaused = false;
        Time.timeScale = 1f;

        if (menuCanvasGroup != null)
        {
            menuCanvasGroup.blocksRaycasts = false;
            menuCanvasGroup.interactable = false;
        }

        JobAdController jobAd = FindObjectOfType<JobAdController>();
        bool isJobAdActive = jobAd != null && jobAd.jobAdPanel != null && jobAd.jobAdPanel.activeInHierarchy;

        if (isJobAdActive)
        {
            if (cursorManager != null)
            {
                cursorManager.ShowCursor();
            }
            Debug.Log("Возврат в UI - курсор включен");
        }
        else
        {
            if (cursorManager != null)
            {
                cursorManager.HideCursor();
            }
            Debug.Log("Возврат в игровой процесс - курсор выключен");
        }

        Debug.Log("Игра продолжается!");
    }

    public void ExitToMainMenu()
    {
        IsGamePaused = false;
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }

    public void SaveGame()
    {
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.SaveGame();
            Debug.Log("Игра сохранена через SaveManager!");
        }
        else
        {
            Debug.LogError("SaveManager не найден!");
        }
    }

    // ЗАМЕНЯЕМ LoadGame НА ОТКРЫТИЕ НАСТРОЕК
    public void OpenSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
            Debug.Log("Открыты настройки из меню паузы");

            // Скрываем основное меню паузы при открытии настроек
            if (menuCanvasGroup != null)
            {
                menuCanvasGroup.blocksRaycasts = false;
                menuCanvasGroup.interactable = false;
            }
        }
        else
        {
            Debug.LogWarning("SettingsPanel не назначен в PauseMenu!");
        }
    }

    // ДОБАВЛЯЕМ МЕТОД ДЛЯ ЗАКРЫТИЯ НАСТРОЕК
    public void CloseSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
            Debug.Log("Настройки закрыты");

            // Возвращаем управление основному меню паузы
            if (menuCanvasGroup != null && IsGamePaused)
            {
                menuCanvasGroup.blocksRaycasts = true;
                menuCanvasGroup.interactable = true;
            }
        }
    }

    // ========== МЕТОДЫ ДЛЯ АУДИО НАСТРОЕК (как в SceneLoader) ==========

    public void SetMasterVolume(float volume)
    {
        AudioListener.volume = volume;
        Debug.Log($"Громкость установлена: {volume}");
    }

    public void SetMusicVolume(float volume)
    {
        // Находим аудио источник в сцене
        AudioSource audioSource = FindObjectOfType<AudioSource>();
        if (audioSource != null)
        {
            audioSource.volume = volume;
        }
        Debug.Log($"Громкость музыки установлена: {volume}");
    }

    public void ToggleMusic(bool isOn)
    {
        AudioSource audioSource = FindObjectOfType<AudioSource>();
        if (audioSource != null)
        {
            audioSource.mute = !isOn;
        }
        Debug.Log($"Музыка {(isOn ? "включена" : "выключена")}");
    }

    public void ToggleSFX(bool isOn)
    {
        // Здесь можно добавить логику для SFX
        Debug.Log($"SFX {(isOn ? "включены" : "выключены")}");
    }

    // ========== ГРАФИЧЕСКИЕ НАСТРОЙКИ (как в SceneLoader) ==========

    public void SetQualityLevel(int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex);
        Debug.Log($"Уровень качества установлен: {QualitySettings.names[qualityIndex]}");
    }

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        Debug.Log($"Полноэкранный режим: {isFullscreen}");
    }

    public void SetResolution(int resolutionIndex)
    {
        Resolution[] resolutions = Screen.resolutions;
        if (resolutionIndex >= 0 && resolutionIndex < resolutions.Length)
        {
            Resolution resolution = resolutions[resolutionIndex];
            Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
            Debug.Log($"Разрешение установлено: {resolution.width}x{resolution.height}");
        }
    }
}