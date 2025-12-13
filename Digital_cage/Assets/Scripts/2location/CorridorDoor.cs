using UnityEngine;
using System.Collections.Generic;

public class CorridorDoor : MonoBehaviour, IInteractable
{
    [Header("Dialogue Settings")]
    public ManagerDialogue2 dialogueManager;
    public int requiredDialogueIndex = 6;

    [Header("Door Dialogue")]
    [TextArea(2, 5)]
    public List<string> doorDialogueTexts;

    [Header("Todo Settings")]
    public TodoUIManager todoManager; // Ссылка на менеджер туду
    public int todoIndexToComplete = 0; // Какой пункт туду зачеркнуть (0 = первый)

    private bool isInteractable = false;
    private bool dialogueTriggered = false;
    private bool hasBeenUsed = false; // Флаг однократного использования

    void Start()
    {
        // Устанавливаем слой Interactable для работы с InteractionController
        gameObject.layer = LayerMask.NameToLayer("Interactable");

        // Подписываемся на событие диалога
        if (dialogueManager != null)
        {
            dialogueManager.OnDialogueIndexReached += OnDialogueIndexReached;
        }

        // Изначально делаем объект неинтерактивным
        SetInteractable(false);
    }

    /// <summary>
    /// Вызывается InteractionController при наведении
    /// </summary>
    public string GetInteractionText()
    {
        if (!isInteractable || hasBeenUsed)
            return ""; // Пустой текст = не показывать подсказку

        return "Нажмите E";
    }

    /// <summary>
    /// Вызывается InteractionController при нажатии E
    /// </summary>
    public void Interact()
    {
        if (!isInteractable || hasBeenUsed || dialogueTriggered) return;

        SetInteractable(false);
        hasBeenUsed = true; // Помечаем как использованную

        // Запускаем диалог двери
        if (dialogueManager != null && doorDialogueTexts != null && doorDialogueTexts.Count > 0)
        {
            dialogueTriggered = true;
            dialogueManager.StartDialogue(doorDialogueTexts, OnDoorDialogueEnd);
        }
    }

    /// <summary>
    /// Обработчик события достижения индекса диалога
    /// </summary>
    private void OnDialogueIndexReached(int currentIndex)
    {
        if (currentIndex >= requiredDialogueIndex && !isInteractable && !hasBeenUsed)
        {
            SetInteractable(true);
            Debug.Log("CorridorDoor: Дверь теперь интерактивна!");
        }
    }

    /// <summary>
    /// Вызывается когда диалог двери завершен
    /// </summary>
    private void OnDoorDialogueEnd()
    {
        dialogueTriggered = false;
        Debug.Log("CorridorDoor: Диалог двери завершен");

        // Зачеркиваем задачу в TodoManager
        CompleteTodoTask();

        // После использования делаем дверь полностью неинтерактивной
        SetInteractable(false);
    }

    /// <summary>
    /// Устанавливает интерактивность объекта
    /// </summary>
    private void SetInteractable(bool interactable)
    {
        isInteractable = interactable;

        // Меняем слой чтобы InteractionController не мог навеститься
        if (interactable)
        {
            gameObject.layer = LayerMask.NameToLayer("Interactable");
        }
        else
        {
            // Меняем на слой Default или другой неинтерактивный слой
            gameObject.layer = LayerMask.NameToLayer("Default");
        }
    }

    /// <summary>
    /// Зачеркивает задачу в TodoManager после диалога
    /// </summary>
    private void CompleteTodoTask()
    {
        if (todoManager != null)
        {
            todoManager.CompleteTodoItem(todoIndexToComplete);
            Debug.Log($"CorridorDoor: Задача {todoIndexToComplete} отмечена как выполненная");
        }
        else
        {
            Debug.LogWarning("CorridorDoor: TodoManager не назначен, не могу отметить задачу");
        }
    }

    void OnDestroy()
    {
        // Отписываемся от события при уничтожении
        if (dialogueManager != null)
        {
            dialogueManager.OnDialogueIndexReached -= OnDialogueIndexReached;
        }
    }

    // Визуальная отладка в редакторе
    void OnDrawGizmos()
    {
        if (isInteractable && !hasBeenUsed)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(transform.position, Vector3.one * 1.5f);
        }
        else if (hasBeenUsed)
        {
            Gizmos.color = Color.gray;
            Gizmos.DrawWireCube(transform.position, Vector3.one * 1.3f);
        }
        else
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(transform.position, Vector3.one * 1.1f);
        }
    }
}