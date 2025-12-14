using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class DialogueTrigger5 : MonoBehaviour
{
    [Header("Dialogue Settings")]
    public DialogueManager5 dialogueManager;

    [TextArea(2, 5)]
    public List<string> dialogueLines;

    [Header("Black Screen Settings")]
    public Image blackScreenImage;
    public float blackScreenTime = 1f;
    public float fadeDuration = 2f;

    [Header("Todo Settings")]
    public TodoUI5 todoManager; // Ссылка на TodoUI5 (опционально)
    public bool showTodoAfterDialogue = false; // Показать Todo после диалога

    private bool dialogueStarted = false;
    private bool playerInside = false;
    private bool allowStart = false;

    // Событие окончания диалога
    public static System.Action OnDialogue5Finished;

    void Start()
    {
        // Проверка зависимостей
        if (dialogueManager == null)
        {
            Debug.LogError("DialogueTrigger5: DialogueManager5 не назначен!");
            return;
        }

        StartCoroutine(StartSequence());
    }

    private IEnumerator StartSequence()
    {
        // Начальный черный экран
        if (blackScreenImage != null)
        {
            blackScreenImage.gameObject.SetActive(true);
            blackScreenImage.color = new Color(0f, 0f, 0f, 1f);
        }

        yield return new WaitForSeconds(blackScreenTime);

        // Плавное исчезновение черного экрана
        if (blackScreenImage != null)
            yield return StartCoroutine(FadeOut());

        allowStart = true;

        // Если игрок уже внутри триггера, запускаем диалог
        if (playerInside && !dialogueStarted)
        {
            StartMainDialogue();
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
        Debug.Log("DialogueTrigger5: Запуск основного диалога");

        // Подписываемся на событие завершения диалога
        dialogueManager.StartDialogue(dialogueLines, OnMainDialogueEnd);
    }

    /// <summary>
    /// Callback, вызываемый DialogueManager5 при завершении диалога
    /// </summary>
    private void OnMainDialogueEnd()
    {
        Debug.Log("DialogueTrigger5: Основной диалог завершён!");

        // Отправляем событие завершения
        OnDialogue5Finished?.Invoke();

        // Показываем Todo панель если нужно
        if (showTodoAfterDialogue && todoManager != null)
        {
            todoManager.ShowPanel();
            Debug.Log("DialogueTrigger5: TodoUI5 панель показана после диалога");
        }

        // Деактивируем триггер если нужно
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Плавное исчезновение черного экрана
    /// </summary>
    private IEnumerator FadeOut()
    {
        float timer = 0f;
        Color startColor = blackScreenImage.color;
        Color endColor = new Color(0, 0, 0, 0);

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            blackScreenImage.color = Color.Lerp(startColor, endColor, timer / fadeDuration);
            yield return null;
        }

        blackScreenImage.gameObject.SetActive(false);
    }

    /// <summary>
    /// Принудительный запуск диалога из кода
    /// </summary>
    public void TriggerDialogue()
    {
        if (!dialogueStarted && allowStart)
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
        Debug.Log("DialogueTrigger5: Игрок вошел в триггер");

        if (allowStart && !dialogueStarted)
        {
            StartMainDialogue();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = false;
            Debug.Log("DialogueTrigger5: Игрок вышел из триггера");
        }
    }

    void OnValidate()
    {
        // Автоматическое назначение ссылок в редакторе
        if (dialogueManager == null)
        {
            dialogueManager = FindObjectOfType<DialogueManager5>();
        }

        if (todoManager == null && showTodoAfterDialogue)
        {
            todoManager = FindObjectOfType<TodoUI5>();
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