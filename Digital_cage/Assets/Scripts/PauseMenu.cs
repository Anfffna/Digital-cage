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

    // ДОБАВЬ ЭТУ СТРОЧКУ:
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
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (IsGamePaused)
                ResumeGame();
            else
                PauseGame();
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
        // ИЗМЕНИ ЭТУ СТРОЧКУ:
        IsGamePaused = true; // было: isPaused = true;
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
        // ИЗМЕНИ ЭТУ СТРОЧКУ:
        IsGamePaused = false; // было: isPaused = false;
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
        IsGamePaused = false; // И здесь тоже
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

    public void LoadGame()
    {
        Debug.Log("Игра загружена!");
    }
}