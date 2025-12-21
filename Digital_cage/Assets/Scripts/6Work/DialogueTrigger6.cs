using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DialogueTrigger6 : MonoBehaviour
{
    [Header("Dialogue Settings")]
    public ManagerDialogue6 dialogueManager; // Изменено на ManagerDialogue6

    [TextArea(2, 5)]
    public List<string> dialogueLines;

    [Header("Todo Settings")]
    public ToDoUI todoManager; // Ссылка на ToDoUI
    public bool showTodoAfterDialogue = false; // Показать Todo после диалога

    private bool dialogueStarted = false;
    private bool playerInside = false;

    // Событие окончания диалога
    public static System.Action OnDialogue6Finished;

    void Start()
    {
        // Проверка зависимостей
        if (dialogueManager == null)
        {
            Debug.LogError("DialogueTrigger6: ManagerDialogue6 не назначен!");
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
        Debug.Log("DialogueTrigger6: Запуск основного диалога");

        // Подписываемся на событие завершения диалога
        dialogueManager.StartDialogue(dialogueLines, OnMainDialogueEnd);
    }

    /// <summary>
    /// Callback, вызываемый ManagerDialogue6 при завершении диалога
    /// </summary>
    private void OnMainDialogueEnd()
    {
        Debug.Log("DialogueTrigger6: Основной диалог завершён!");

        // Отправляем событие завершения
        OnDialogue6Finished?.Invoke();

        // Показываем Todo панель если нужно
        if (showTodoAfterDialogue && todoManager != null)
        {
            todoManager.ShowPanel();
            Debug.Log("DialogueTrigger6: ToDoUI панель показана после диалога");
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
        Debug.Log("DialogueTrigger6: Игрок вошел в триггер");

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
            Debug.Log("DialogueTrigger6: Игрок вышел из триггера");
        }
    }

    void OnValidate()
    {
        // Автоматическое назначение ссылок в редакторе
        if (dialogueManager == null)
        {
            dialogueManager = FindObjectOfType<ManagerDialogue6>();
        }

        if (todoManager == null && showTodoAfterDialogue)
        {
            todoManager = FindObjectOfType<ToDoUI>();
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