using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // ДОБАВИЛИ
using System.Collections;
using System.Collections.Generic;

public class DeadMama : MonoBehaviour
{
    [Header("Black Screen Settings")]
    public Image blackScreen;           // Черный экран (UI Image)
    public float blackScreenDelay = 0.5f; // Задержка перед появлением черного экрана

    [Header("Dialogue Settings")]
    public ManagerDialogue7 dialogueManager; // Диалог на черном экране
    [TextArea(2, 5)]
    public List<string> deadMamaDialogue;    // Реплики для DeadMama

    [Header("Next Scene Settings")]
    public string nextSceneName = "PhoneCutsceneScene"; // Название сцены с катсценой
    public float sceneTransitionDelay = 3f; // Задержка перед переходом (3 секунды)

    [Header("Audio Settings")]
    public AudioSource audioSource;     // Аудиоисточник
    public AudioClip ambientSound;      // Атмосферный звук (опционально)
    public AudioClip blackScreenAppearSound; // Звук при появлении черного экрана

    [Header("Timing Settings")]
    public float dialogueStartDelay = 1f; // Задержка перед началом диалога

    private bool sequenceStarted = false;
    private bool dialogueCompleted = false;

    void Start()
    {
        // Инициализация
        InitializeComponents();

        // Скрываем черный экран при старте
        if (blackScreen != null)
        {
            blackScreen.gameObject.SetActive(false);
            Debug.Log("DeadMama: Черный экран скрыт");
        }
        else
        {
            Debug.LogError("DeadMama: blackScreen не назначен!");
        }
    }

    void InitializeComponents()
    {
        // Создаем AudioSource если нет
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        // Автонаход dialogueManager если не назначен
        if (dialogueManager == null)
        {
            dialogueManager = FindObjectOfType<ManagerDialogue7>();
        }
    }

    /// <summary>
    /// Начать последовательность DeadMama
    /// </summary>
    public void StartDeadMamaSequence()
    {
        if (sequenceStarted) return;

        sequenceStarted = true;
        Debug.Log("DeadMama: Начинаю последовательность...");

        StartCoroutine(DeadMamaSequence());
    }

    IEnumerator DeadMamaSequence()
    {
        Debug.Log("DeadMama: Шаг 1 - Жду небольшую паузу...");
        yield return new WaitForSeconds(blackScreenDelay);

        Debug.Log("DeadMama: Шаг 2 - Включаю черный экран...");
        ShowBlackScreen();

        // Проигрываем звук при появлении черного экрана
        PlayBlackScreenSound();

        // Проигрываем атмосферный звук если есть
        if (audioSource != null && ambientSound != null)
        {
            audioSource.PlayOneShot(ambientSound);
        }

        Debug.Log($"DeadMama: Шаг 3 - Жду {dialogueStartDelay} секунд перед диалогом...");
        yield return new WaitForSeconds(dialogueStartDelay);

        Debug.Log("DeadMama: Шаг 4 - Запускаю диалог...");
        StartDeadMamaDialogue();

        // Ждем завершения диалога
        yield return new WaitUntil(() => dialogueCompleted);
        Debug.Log("DeadMama: Диалог завершен, жду перед переходом...");

        // ЖДЕМ 3 СЕКУНДЫ после завершения диалога
        Debug.Log($"DeadMama: Жду {sceneTransitionDelay} секунд перед переходом в сцену '{nextSceneName}'...");
        yield return new WaitForSeconds(sceneTransitionDelay);

        // ПЕРЕХОДИМ В СЛЕДУЮЩУЮ СЦЕНУ
        Debug.Log($"DeadMama: Перехожу в сцену: {nextSceneName}");
        LoadNextScene();
    }

    void ShowBlackScreen()
    {
        if (blackScreen != null)
        {
            // Активируем и делаем полностью черным
            blackScreen.gameObject.SetActive(true);
            Color color = blackScreen.color;
            color.a = 1f;
            color.r = color.g = color.b = 0f; // Чисто черный
            blackScreen.color = color;

            Debug.Log("DeadMama: Черный экран включен");
        }
    }

    void PlayBlackScreenSound()
    {
        if (audioSource != null && blackScreenAppearSound != null)
        {
            audioSource.PlayOneShot(blackScreenAppearSound);
            Debug.Log("DeadMama: Воспроизводится звук черного экрана");
        }
        else
        {
            Debug.Log("DeadMama: Звук черного экрана не настроен или нет AudioSource");
        }
    }

    void StartDeadMamaDialogue()
    {
        if (dialogueManager != null && deadMamaDialogue != null && deadMamaDialogue.Count > 0)
        {
            dialogueManager.StartDialogue(deadMamaDialogue, OnDeadMamaDialogueEnd);
            Debug.Log($"DeadMama: Запущен диалог ({deadMamaDialogue.Count} реплик)");
        }
        else
        {
            Debug.LogWarning("DeadMama: Не могу запустить диалог - не настроен!");
            dialogueCompleted = true; // Пропускаем если нет диалога
        }
    }

    void OnDeadMamaDialogueEnd()
    {
        dialogueCompleted = true;
        Debug.Log("DeadMama: Диалог завершен (callback)");
    }

    void LoadNextScene()
    {
        if (string.IsNullOrEmpty(nextSceneName))
        {
            Debug.LogError("DeadMama: Не указано имя следующей сцены!");
            return;
        }

        try
        {
            SceneManager.LoadScene(nextSceneName);
            Debug.Log($"DeadMama: Сцена '{nextSceneName}' загружается...");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"DeadMama: Ошибка при загрузке сцены '{nextSceneName}': {e.Message}");
        }
    }

    [ContextMenu("Тест: Запустить DeadMama последовательность")]
    public void TestStartSequence()
    {
        if (!sequenceStarted)
        {
            StartDeadMamaSequence();
        }
    }

    [ContextMenu("Тест: Перейти в следующую сцену")]
    public void TestLoadNextScene()
    {
        LoadNextScene();
    }

    [ContextMenu("Тест: Показать черный экран")]
    public void TestShowBlackScreen()
    {
        ShowBlackScreen();
    }

    [ContextMenu("Тест: Запустить диалог")]
    public void TestStartDialogue()
    {
        StartDeadMamaDialogue();
    }

    [ContextMenu("Сбросить состояние")]
    public void ResetState()
    {
        sequenceStarted = false;
        dialogueCompleted = false;

        if (blackScreen != null)
        {
            blackScreen.gameObject.SetActive(false);
        }

        Debug.Log("DeadMama: Состояние сброшено");
    }

    void OnValidate()
    {
        // Автоподключение компонентов
        if (blackScreen != null)
        {
            // Проверяем что Image черный
            if (blackScreen.color.r > 0.1f || blackScreen.color.g > 0.1f || blackScreen.color.b > 0.1f)
            {
                Debug.LogWarning("DeadMama: Черный экран не черный! Установите цвет (0,0,0)");
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        // Визуализация в редакторе
        if (blackScreen != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(blackScreen.transform.position, Vector3.one * 0.5f);

            GUIStyle style = new GUIStyle();
            style.normal.textColor = Color.red;
            style.alignment = TextAnchor.MiddleCenter;

#if UNITY_EDITOR
            UnityEditor.Handles.Label(blackScreen.transform.position + Vector3.up * 0.3f, 
                "Черный экран", style);
#endif
        }
    }
}