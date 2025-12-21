using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DialogueWork5 : MonoBehaviour
{
    [Header("Dialogue Settings")]
    public DialogueManager5 dialogueManager;

    [TextArea(2, 5)]
    public List<string> dialogueLines;

    [Header("Sleep Check")]
    public Sleep sleepSystem; // Ссылка на скрипт сна
    public bool requireSleepCompletion = true; // Требовать завершение сна

    private bool dialogueStarted = false;
    private bool playerInside = false;
    private bool allowStart = true; // Теперь сразу доступен для запуска
    private bool playerHasSlept = false; // Игрок уже поспал и проснулся

    // Событие окончания диалога
    public static System.Action OnDialogue5Finished;

    void Start()
    {
        // Проверка зависимостей
        if (dialogueManager == null)
        {
            Debug.LogError("DialogueWork5: DialogueManager5 не назначен!");
            return;
        }

        // Если требуется проверка сна, подписываемся на событие пробуждения
        if (requireSleepCompletion && sleepSystem != null)
        {
            sleepSystem.OnPlayerWokeUp += OnPlayerWokeUpHandler;
            Debug.Log("DialogueWork5: Подписался на событие пробуждения игрока");
        }
        else if (requireSleepCompletion && sleepSystem == null)
        {
            Debug.LogWarning("DialogueWork5: Требуется Sleep система, но она не назначена!");
        }

        // Если нужно запускать диалог сразу при старте
        // StartMainDialogue();
    }

    void OnDestroy()
    {
        // Отписываемся от событий при уничтожении объекта
        if (sleepSystem != null)
        {
            sleepSystem.OnPlayerWokeUp -= OnPlayerWokeUpHandler;
        }
    }

    /// <summary>
    /// Обработчик события пробуждения игрока
    /// </summary>
    private void OnPlayerWokeUpHandler()
    {
        playerHasSlept = true;
        Debug.Log("DialogueWork5: Игрок проснулся, диалог теперь доступен");

        // Если игрок уже внутри триггера, запускаем диалог
        if (playerInside && !dialogueStarted && allowStart)
        {
            StartMainDialogue();
        }
    }

    /// <summary>
    /// Проверяем, можно ли запустить диалог
    /// </summary>
    private bool CanStartDialogue()
    {
        if (!allowStart) return false;
        if (dialogueStarted) return false;

        // Если требуется завершение сна, проверяем
        if (requireSleepCompletion)
        {
            return playerHasSlept;
        }

        return true;
    }

    /// <summary>
    /// Запуск основного диалога
    /// </summary>
    private void StartMainDialogue()
    {
        if (!CanStartDialogue() || dialogueManager == null || dialogueLines == null || dialogueLines.Count == 0)
            return;

        dialogueStarted = true;
        Debug.Log("DialogueWork5: Запуск основного диалога");

        // Подписываемся на событие завершения диалога
        dialogueManager.StartDialogue(dialogueLines, OnMainDialogueEnd);
    }

    /// <summary>
    /// Callback, вызываемый DialogueManager5 при завершении диалога
    /// </summary>
    private void OnMainDialogueEnd()
    {
        Debug.Log("DialogueWork5: Основной диалог завершён!");

        // Отправляем событие завершения
        OnDialogue5Finished?.Invoke();

        // Деактивируем триггер если нужно
        // gameObject.SetActive(false);
    }

    /// <summary>
    /// Принудительный запуск диалога из кода
    /// </summary>
    public void TriggerDialogue()
    {
        if (!dialogueStarted && CanStartDialogue())
        {
            StartMainDialogue();
        }
    }

    /// <summary>
    /// Принудительный сброс триггера
    /// </summary>
    public void ResetDialogue()
    {
        dialogueStarted = false;
        playerInside = false;
        playerHasSlept = false;
        gameObject.SetActive(true);
    }

    /// <summary>
    /// Добавить новую строку диалога
    /// </summary>
    public void AddDialogueLine(string line)
    {
        if (dialogueLines == null)
            dialogueLines = new List<string>();

        dialogueLines.Add(line);
    }

    /// <summary>
    /// Очистить все строки диалога
    /// </summary>
    public void ClearDialogueLines()
    {
        if (dialogueLines != null)
            dialogueLines.Clear();
    }

    /// <summary>
    /// Установить новые строки диалога
    /// </summary>
    public void SetDialogueLines(List<string> newLines)
    {
        dialogueLines = newLines;
    }

    /// <summary>
    /// Принудительно установить флаг, что игрок поспал
    /// (например, для тестирования или пропуска сна)
    /// </summary>
    public void ForceSetSlept(bool slept)
    {
        playerHasSlept = slept;
        Debug.Log($"DialogueWork5: Флаг сна принудительно установлен в {slept}");
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInside = true;
        Debug.Log("DialogueWork5: Игрок вошел в триггер");

        if (CanStartDialogue())
        {
            StartMainDialogue();
        }
        else if (requireSleepCompletion && !playerHasSlept)
        {
            Debug.Log("DialogueWork5: Игрок в триггере, но еще не поспал. Ждем...");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = false;
            Debug.Log("DialogueWork5: Игрок вышел из триггера");
        }
    }

    void OnValidate()
    {
        // Автоматическое назначение ссылок в редакторе
        if (dialogueManager == null)
        {
            dialogueManager = FindObjectOfType<DialogueManager5>();
        }

        // Автоматический поиск Sleep системы если требуется
        if (requireSleepCompletion && sleepSystem == null)
        {
            sleepSystem = FindObjectOfType<Sleep>();
            if (sleepSystem != null)
            {
                Debug.Log("DialogueWork5: Найден Sleep система автоматически");
            }
        }
    }

    void OnDrawGizmos()
    {
        BoxCollider collider = GetComponent<BoxCollider>();
        if (collider != null)
        {
            // Цвет меняется в зависимости от состояния
            if (dialogueStarted)
            {
                Gizmos.color = Color.gray; // Диалог уже запущен
            }
            else if (playerInside && CanStartDialogue())
            {
                Gizmos.color = Color.yellow; // Готов к запуску
            }
            else if (requireSleepCompletion && !playerHasSlept)
            {
                Gizmos.color = Color.red; // Ждет, пока игрок поспит
            }
            else
            {
                Gizmos.color = Color.cyan; // Ожидает игрока
            }

            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(collider.center, collider.size);
        }
    }
}