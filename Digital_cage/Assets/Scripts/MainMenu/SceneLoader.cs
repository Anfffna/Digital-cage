using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneLoader : MonoBehaviour
{
    [Header("Audio Settings")]
    public AudioClip mainMenuMusic;
    public AudioSource audioSource;

    [Header("Settings Menu")]
    public GameObject settingsPanel; // Ссылка на панель настроек
    public float animationDuration = 0.3f; // Длительность анимации

    private string currentSceneName;
    private bool isSettingsOpen = false;
    private bool isAnimating = false;
    private CanvasGroup settingsCanvasGroup;

    void Start()
    {
        // Получаем текущую сцену
        currentSceneName = SceneManager.GetActiveScene().name;

        // Если мы на главном меню - запускаем музыку
        if (currentSceneName == "MainMenu")
        {
            PlayMainMenuMusic();
        }

        // Подписываемся на событие смены сцены
        SceneManager.activeSceneChanged += OnSceneChanged;

        // Настраиваем панель настроек
        if (settingsPanel != null)
        {
            // Добавляем CanvasGroup если его нет
            settingsCanvasGroup = settingsPanel.GetComponent<CanvasGroup>();
            if (settingsCanvasGroup == null)
            {
                settingsCanvasGroup = settingsPanel.AddComponent<CanvasGroup>();
            }

            // Сразу делаем панель неактивной и невидимой
            settingsPanel.SetActive(false);
            settingsCanvasGroup.alpha = 0f;
            settingsCanvasGroup.interactable = false;
            settingsCanvasGroup.blocksRaycasts = false;
        }
    }

    void Update()
    {
        // Обработка закрытия настроек по ESC
        if (isSettingsOpen && Input.GetKeyDown(KeyCode.Escape) && !isAnimating)
        {
            CloseSettings();
        }
    }

    void OnSceneChanged(Scene previousScene, Scene newScene)
    {
        // Останавливаем музыку при уходе с главного меню
        if (previousScene.name == "MainMenu" && audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }

        // Запускаем музыку при входе в главное меню
        if (newScene.name == "MainMenu")
        {
            PlayMainMenuMusic();
        }

        currentSceneName = newScene.name;

        // Закрываем настройки при смене сцены
        if (isSettingsOpen && !isAnimating)
        {
            StartCoroutine(CloseSettingsCoroutine());
        }
    }

    private void PlayMainMenuMusic()
    {
        if (mainMenuMusic != null && audioSource != null)
        {
            audioSource.clip = mainMenuMusic;
            audioSource.loop = true; // Зацикливаем музыку
            audioSource.Play();
            Debug.Log("Запущена музыка главного меню");
        }
        else
        {
            if (mainMenuMusic == null)
                Debug.LogWarning("MainMenuMusic не назначен!");
            if (audioSource == null)
                Debug.LogWarning("AudioSource не найден!");
        }
    }

    // ========== МЕТОДЫ ДЛЯ НАСТРОЕК ==========

    public void OpenSettings()
    {
        if (settingsPanel != null && !isAnimating && !isSettingsOpen)
        {
            StartCoroutine(OpenSettingsCoroutine());
        }
        else
        {
            Debug.LogWarning("SettingsPanel не назначен или анимация уже идет!");
        }
    }

    public void CloseSettings()
    {
        if (settingsPanel != null && !isAnimating && isSettingsOpen)
        {
            StartCoroutine(CloseSettingsCoroutine());
        }
    }

    private IEnumerator OpenSettingsCoroutine()
    {
        isAnimating = true;

        // Активируем панель перед анимацией
        settingsPanel.SetActive(true);
        settingsCanvasGroup.interactable = false;
        settingsCanvasGroup.blocksRaycasts = false;

        // Приостанавливаем игру, если мы не в главном меню
        if (currentSceneName != "MainMenu")
        {
            Time.timeScale = 0f;
        }

        // Анимация появления
        float elapsedTime = 0f;
        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float progress = elapsedTime / animationDuration;

            // Плавное увеличение прозрачности
            settingsCanvasGroup.alpha = Mathf.SmoothStep(0f, 1f, progress);

            // Легкое масштабирование для эффекта "появления"
            settingsPanel.transform.localScale = Vector3.one * Mathf.SmoothStep(0.8f, 1f, progress);

            yield return null;
        }

        // Устанавливаем финальные значения
        settingsCanvasGroup.alpha = 1f;
        settingsPanel.transform.localScale = Vector3.one;
        settingsCanvasGroup.interactable = true;
        settingsCanvasGroup.blocksRaycasts = true;

        isSettingsOpen = true;
        isAnimating = false;

        Debug.Log("Открыты настройки");
    }

    private IEnumerator CloseSettingsCoroutine()
    {
        isAnimating = true;

        // Отключаем взаимодействие во время анимации
        settingsCanvasGroup.interactable = false;
        settingsCanvasGroup.blocksRaycasts = false;

        // Анимация исчезновения
        float elapsedTime = 0f;
        Vector3 startScale = settingsPanel.transform.localScale;

        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float progress = elapsedTime / animationDuration;

            // Плавное уменьшение прозрачности
            settingsCanvasGroup.alpha = Mathf.SmoothStep(1f, 0f, progress);

            // Легкое масштабирование для эффекта "исчезновения"
            settingsPanel.transform.localScale = startScale * Mathf.SmoothStep(1f, 0.8f, progress);

            yield return null;
        }

        // Устанавливаем финальные значения
        settingsCanvasGroup.alpha = 0f;
        settingsPanel.transform.localScale = startScale;
        settingsPanel.SetActive(false);

        isSettingsOpen = false;
        isAnimating = false;

        // Возобновляем игру, если мы не в главном меню
        if (currentSceneName != "MainMenu")
        {
            Time.timeScale = 1f;
        }

        Debug.Log("Настройки закрыты");
    }

    public void ToggleSettings()
    {
        if (isAnimating) return;

        if (isSettingsOpen)
        {
            CloseSettings();
        }
        else
        {
            OpenSettings();
        }
    }

    // ========== МЕТОДЫ ДЛЯ АУДИО НАСТРОЕК ==========

    public void SetMasterVolume(float volume)
    {
        AudioListener.volume = volume;
        Debug.Log($"Громкость установлена: {volume}");
    }

    public void SetMusicVolume(float volume)
    {
        if (audioSource != null)
        {
            audioSource.volume = volume;
        }
        Debug.Log($"Громкость музыки установлена: {volume}");
    }

    public void ToggleMusic(bool isOn)
    {
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

    // ========== ГРАФИЧЕСКИЕ НАСТРОЙКИ ==========

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
        // Пример: можно создать массив разрешений и выбирать по индексу
        Resolution[] resolutions = Screen.resolutions;
        if (resolutionIndex >= 0 && resolutionIndex < resolutions.Length)
        {
            Resolution resolution = resolutions[resolutionIndex];
            Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
            Debug.Log($"Разрешение установлено: {resolution.width}x{resolution.height}");
        }
    }

    // ========== ОСНОВНЫЕ МЕТОДЫ ЗАГРУЗКИ СЦЕН ==========

    public void LoadGameScene()
    {
        // Восстанавливаем время перед загрузкой сцены
        Time.timeScale = 1f;
        SceneManager.LoadScene("Game");
    }

    public void LoadMainMenu()
    {
        // Восстанавливаем время перед загрузкой сцены
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public void LoadSceneByName(string sceneName)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }

    public void LoadSceneByIndex(int sceneIndex)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneIndex);
    }

    public void QuitGame()
    {
        Debug.Log("Выход из игры!");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    void OnDestroy()
    {
        // Отписываемся от события при уничтожении объекта
        SceneManager.activeSceneChanged -= OnSceneChanged;
    }
}