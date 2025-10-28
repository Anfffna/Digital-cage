using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class PauseMenu : MonoBehaviour
{
    [Header("UI References")]
    public GameObject pauseMenuCanvas; // Весь Canvas с меню паузы

    [Header("Post Processing")]
    public PostProcessVolume blurVolume; // Перетащи сюда PauseBlurVolume

    [Header("Cursor")]
    public CursorUI cursorManager; // Перетащи сюда объект с CursorUI

    [Header("Settings")]
    public float blurFadeSpeed = 8f;
    public float menuFadeSpeed = 8f; // Скорость появления меню

    private bool isPaused = false;
    private CanvasGroup menuCanvasGroup;

    void Start()
    {
        // Изначально скрываем меню
        if (pauseMenuCanvas != null)
        {
            pauseMenuCanvas.SetActive(true); // Активен для анимации

            // Добавляем CanvasGroup для плавности
            menuCanvasGroup = pauseMenuCanvas.GetComponent<CanvasGroup>();
            if (menuCanvasGroup == null)
                menuCanvasGroup = pauseMenuCanvas.AddComponent<CanvasGroup>();

            menuCanvasGroup.alpha = 0f;
            menuCanvasGroup.interactable = false;
            menuCanvasGroup.blocksRaycasts = false;
        }

        // ВЫКЛЮЧАЕМ размытие ПРИНУДИТЕЛЬНО
        if (blurVolume != null)
        {
            blurVolume.weight = 0f; // ? Гарантированно 0!
            Debug.Log("Post Processing Weight сброшен на: " + blurVolume.weight);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("ESC pressed!");
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }

        // Плавное обновление размытия и меню
        UpdatePauseEffects();
    }

    void UpdatePauseEffects()
    {
        // Плавное размытие
        if (blurVolume != null)
        {
            float targetWeight = isPaused ? 1f : 0f;
            float previousWeight = blurVolume.weight;

            blurVolume.weight = Mathf.Lerp(blurVolume.weight, targetWeight,
                Time.unscaledDeltaTime * blurFadeSpeed);

            // Логируем если вес изменился
            if (Mathf.Abs(previousWeight - blurVolume.weight) > 0.01f)
            {
                Debug.Log($"Blur Weight: {blurVolume.weight:F2}, Target: {targetWeight}, isPaused: {isPaused}");
            }

            // ДОБАВЬ ПРОВЕРКУ: если близко к 0 - ставим точно 0
            if (!isPaused && blurVolume.weight < 0.01f)
            {
                blurVolume.weight = 0f;
            }
        }

        // Плавное появление/исчезновение меню
        if (menuCanvasGroup != null)
        {
            float targetAlpha = isPaused ? 1f : 0f;
            menuCanvasGroup.alpha = Mathf.Lerp(menuCanvasGroup.alpha, targetAlpha,
                Time.unscaledDeltaTime * menuFadeSpeed);

            // Включаем/выключаем интерактивность когда анимация завершена
            if (isPaused && menuCanvasGroup.alpha > 0.9f)
            {
                menuCanvasGroup.interactable = true;
                menuCanvasGroup.blocksRaycasts = true;
            }
            else if (!isPaused && menuCanvasGroup.alpha < 0.1f)
            {
                menuCanvasGroup.interactable = false;
                menuCanvasGroup.blocksRaycasts = false;
            }
        }
    }

    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;

        if (cursorManager != null)
        {
            cursorManager.ShowCursor();
        }
        else
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.None;
        }

        Debug.Log("Игра на паузе!");
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;

        // Проверяем, активно ли еще объявление о работе
        JobAdController jobAd = FindObjectOfType<JobAdController>();
        bool isJobAdActive = jobAd != null && jobAd.jobAdPanel != null && jobAd.jobAdPanel.activeInHierarchy;

        // ДОБАВИМ ПРОВЕРКУ НА ТИП СЦЕНЫ
        // Если это сцена с игровым процессом (без JobAd) - выключаем курсор
        // Если это UI-сцена (с JobAd) - оставляем курсор

        if (isJobAdActive)
        {
            // UI-сцена (объявление активно) - курсор ВКЛЮЧЕН
            if (cursorManager != null)
            {
                cursorManager.ShowCursor();
            }
            Debug.Log("Возврат в UI - курсор включен");
        }
        else
        {
            // Игровая сцена (объявления нет) - курсор ВЫКЛЮЧЕН
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
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }

    public void SaveGame()
    {
        Debug.Log("Игра сохранена!");
    }

    public void LoadGame()
    {
        Debug.Log("Игра загружена!");
    }
}