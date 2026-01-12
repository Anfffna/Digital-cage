using UnityEngine;
using System.Collections.Generic;

public class ErrorTalk : MonoBehaviour
{
    [Header("Dialogue Settings")]
    public ManagerDialogue6 dialogueManager;

    [Header("Error Dialogue")]
    [TextArea(2, 5)]
    public List<string> firstErrorDialogue = new List<string>();

    [Header("Settings")]
    public bool onlyFirstError = true; // Только при первой ошибке

    private bool errorAlreadyTriggered = false;

    void Start()
    {
        // Ищем ManagerDialogue6 если не назначен
        if (dialogueManager == null)
        {
            dialogueManager = FindObjectOfType<ManagerDialogue6>();
            if (dialogueManager == null)
            {
                Debug.LogWarning("ErrorTalk: ManagerDialogue6 не найден в сцене!");
            }
        }

        // Проверяем наличие диалогов
        if (firstErrorDialogue == null || firstErrorDialogue.Count == 0)
        {
            Debug.LogWarning("ErrorTalk: Не настроены строки диалога для первой ошибки!");
        }

        errorAlreadyTriggered = false;
        Debug.Log("ErrorTalk: Инициализирован");
    }

    // Метод для вызова из Work при ошибке
    public void TriggerErrorDialogue()
    {
        // Если уже срабатывал и включена опция "только первая ошибка"
        if (onlyFirstError && errorAlreadyTriggered)
        {
            Debug.Log("ErrorTalk: Диалог ошибки уже срабатывал, пропускаем");
            return;
        }

        // Если нет менеджера диалогов
        if (dialogueManager == null)
        {
            Debug.LogError("ErrorTalk: Нет ссылки на ManagerDialogue6!");
            return;
        }

        // Если нет строк диалога
        if (firstErrorDialogue == null || firstErrorDialogue.Count == 0)
        {
            Debug.LogError("ErrorTalk: Нет строк диалога для отображения!");
            return;
        }

        // Запускаем диалог
        Debug.Log($"ErrorTalk: Запускаю диалог ошибки ({firstErrorDialogue.Count} строк)");
        dialogueManager.StartDialogue(firstErrorDialogue);

        // Отмечаем, что ошибка уже сработала
        errorAlreadyTriggered = true;
    }

    // Метод для сброса состояния (если нужно)
    public void ResetErrorState()
    {
        errorAlreadyTriggered = false;
        Debug.Log("ErrorTalk: Состояние ошибки сброшено");
    }
}