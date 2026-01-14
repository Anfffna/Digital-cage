using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DialogueTrigger7 : MonoBehaviour
{
    [Header("Dialogue Settings")]
    public ManagerDialogue7 dialogueManager; // Изменено на ManagerDialogue7

    [TextArea(2, 5)]
    public List<string> dialogueLines;

    [Header("Todo Settings")]
    public TodoUI7 todoManager; // Ссылка на TodoUI7
    public bool showTodoAfterDialogue = false; // Показать Todo после диалога

    private bool dialogueStarted = false;
    private bool playerInside = false;

    // Событие окончания диалога
    public static System.Action OnDialogue7Finished;

    void Start()
    {
        // Проверка зависимостей
        if (dialogueManager == null)
        {
            Debug.LogError("DialogueTrigger7: ManagerDialogue7 не назначен!");
            return;
        }
    }

    /// <summary>
    /// Запуск основного диалога
    /// </summary>
    private void StartMainDialogue()
    {
        if (dialogueStarted || dialogueManager == null || dialogueLines == null || dialogueLines.Count == 0)
            return;

        dialogueStarted = true;
        Debug.Log("DialogueTrigger7: Запуск основного диалога");

        // Подписываемся на событие завершения диалога
        dialogueManager.StartDialogue(dialogueLines, OnMainDialogueEnd);
    }

    /// <summary>
    /// Callback, вызываемый ManagerDialogue7 при завершении диалога
    /// </summary>
    private void OnMainDialogueEnd()
    {
        Debug.Log("DialogueTrigger7: Основной диалог завершён!");

        // Отправляем событие завершения
        OnDialogue7Finished?.Invoke();

        // Показываем Todo панель если нужно
        if (showTodoAfterDialogue && todoManager != null)
        {
            todoManager.ShowPanel();
            Debug.Log("DialogueTrigger7: TodoUI7 панель показана после диалога");
        }

        // Деактивируем триггер если нужно
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Принудительный запуск диалога из кода
    /// </summary>
    public void TriggerDialogue()
    {
        if (!dialogueStarted)
        {
            StartMainDialogue();
        }
    }

    /// <summary>
    /// Принудительный сброс триггера
    /// </summary>
    public void ResetTrigger()
    {
        dialogueStarted = false;
        playerInside = false;
        gameObject.SetActive(true);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInside = true;
        Debug.Log("DialogueTrigger7: Игрок вошел в триггер");

        if (!dialogueStarted)
        {
            StartMainDialogue();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = false;
            Debug.Log("DialogueTrigger7: Игрок вышел из триггера");
        }
    }

    void OnValidate()
    {
        // Автоматическое назначение ссылок в редакторе
        if (dialogueManager == null)
        {
            dialogueManager = FindObjectOfType<ManagerDialogue7>();
        }

        if (todoManager == null && showTodoAfterDialogue)
        {
            todoManager = FindObjectOfType<TodoUI7>();
        }
    }

    void OnDrawGizmos()
    {
        BoxCollider collider = GetComponent<BoxCollider>();
        if (collider != null)
        {
            Gizmos.color = dialogueStarted ? Color.gray : (playerInside ? Color.yellow : Color.green);
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(collider.center, collider.size);
        }
    }
}