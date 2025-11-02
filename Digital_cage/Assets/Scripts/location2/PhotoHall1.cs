using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using TMPro;

public class PhotoHall1 : MonoBehaviour, IInteractable
{
    [Header("Dialogue Settings")]
    public ManagerDialogue2 dialogueManager;

    [Header("Photo Hall Dialogue")]
    [TextArea(2, 5)]
    public List<string> photoHallDialogueTexts;

    [Header("Todo Settings")]
    public TodoUIManager todoManager; // Ссылка на менеджер туду
    public int requiredTodoIndex = 0; // Какой пункт туду должен быть выполнен (0 = первый)

    private bool isInteractable = false;
    private bool dialogueTriggered = false;
    private bool hasBeenUsed = false;

    void Start()
    {
        // Устанавливаем слой Interactable
        gameObject.layer = LayerMask.NameToLayer("Interactable");

        // Изначально делаем объект неинтерактивным
        SetInteractable(false);

        // Начинаем проверку выполнения задачи в туду
        StartCoroutine(CheckTodoCompletion());
    }

    /// <summary>
    /// Проверяет выполнение задачи в туду менеджере
    /// </summary>
    private IEnumerator CheckTodoCompletion()
    {
        // Ждем пока туду менеджер не будет готов
        while (todoManager == null)
        {
            yield return new WaitForSeconds(0.5f);
            todoManager = FindObjectOfType<TodoUIManager>();
        }

        // Постоянно проверяем, выполнен ли requiredTodoIndex
        while (!isInteractable && !hasBeenUsed)
        {
            if (todoManager != null)
            {
                // Проверяем, выполнен ли требуемый пункт туду
                if (IsTodoItemCompleted(requiredTodoIndex))
                {
                    SetInteractable(true);
                    Debug.Log($"PhotoHall1: Объект теперь интерактивен! Задача {requiredTodoIndex} выполнена.");
                    break;
                }
            }
            yield return new WaitForSeconds(0.3f);
        }
    }

    /// <summary>
    /// Проверяет, выполнен ли конкретный пункт туду
    /// </summary>
    private bool IsTodoItemCompleted(int index)
    {
        if (todoManager == null || todoManager.todoItems == null) return false;
        if (index < 0 || index >= todoManager.todoItems.Length) return false;

        TextMeshProUGUI todoItem = todoManager.todoItems[index];
        return todoItem.text.StartsWith("<s>");
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
        hasBeenUsed = true;

        // Запускаем диалог
        if (dialogueManager != null && photoHallDialogueTexts != null && photoHallDialogueTexts.Count > 0)
        {
            dialogueTriggered = true;
            dialogueManager.StartDialogue(photoHallDialogueTexts, OnDialogueEnd);
        }
    }

    /// <summary>
    /// Вызывается когда диалог завершен
    /// </summary>
    private void OnDialogueEnd()
    {
        dialogueTriggered = false;
        Debug.Log("PhotoHall1: Диалог завершен");

        // После использования делаем объект полностью неинтерактивным
        SetInteractable(false);

        // ЗАЧЕРКИВАНИЕ ТУДУ УБРАНО - будет в других триггерах
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
            gameObject.layer = LayerMask.NameToLayer("Default");
        }
    }

    void OnDestroy()
    {
        // Останавливаем все корутины при уничтожении
        StopAllCoroutines();
    }

    // Визуальная отладка в редакторе
    void OnDrawGizmos()
    {
        if (isInteractable && !hasBeenUsed)
        {
            Gizmos.color = Color.blue;
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