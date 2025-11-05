using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using TMPro;

public class PhotoHall2 : MonoBehaviour, IInteractable
{
    [Header("Dialogue Settings")]
    public ManagerDialogue2 dialogueManager;

    [Header("Photo Hall Dialogue")]
    [TextArea(2, 5)]
    public List<string> photoHallDialogueTexts;

    [Header("Todo Settings")]
    public TodoUIManager todoManager; // Ссылка на менеджер туду
    public int requiredTodoIndex = 0; // Какой пункт туду должен быть выполнен (0 = первый)

    [Header("Photo Settings")]
    public SpriteRenderer photoSprite; // Ссылка на SpriteRenderer фото
    public int showPhotoAtLine = 2;    // На какой реплике показать фото (начиная с 0)
    public int hidePhotoAtLine = 5;    // На какой реплике скрыть фото (начиная с 0)
    public float lagIntensity = 0.1f;  // Интенсивность лагов (0-1)

    private bool isInteractable = false;
    private bool dialogueTriggered = false;
    public bool hasBeenUsed = false;
    private Coroutine photoLagCoroutine;

    void Start()
    {
        // Устанавливаем слой Interactable
        gameObject.layer = LayerMask.NameToLayer("Interactable");

        // Изначально скрываем фото
        if (photoSprite != null)
        {
            photoSprite.enabled = false;
        }

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
                    Debug.Log($"PhotoHall2: Объект теперь интерактивен! Задача {requiredTodoIndex} выполнена.");
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

        // Подписываемся на событие смены реплики
        if (dialogueManager != null)
        {
            dialogueManager.OnDialogueIndexReached += OnDialogueLineChanged;
        }

        // Запускаем диалог
        if (dialogueManager != null && photoHallDialogueTexts != null && photoHallDialogueTexts.Count > 0)
        {
            dialogueTriggered = true;
            dialogueManager.StartDialogue(photoHallDialogueTexts, OnDialogueEnd);
        }
    }

    /// <summary>
    /// Обработчик смены реплики в диалоге
    /// </summary>
    private void OnDialogueLineChanged(int currentLineIndex)
    {
        // Показываем фото на определенной реплике
        if (currentLineIndex == showPhotoAtLine && photoSprite != null)
        {
            ShowPhotoWithLag();
        }
        // Скрываем фото на определенной реплике
        else if (currentLineIndex == hidePhotoAtLine && photoSprite != null)
        {
            HidePhoto();
        }
    }

    /// <summary>
    /// Показывает фото с эффектом лагов
    /// </summary>
    private void ShowPhotoWithLag()
    {
        if (photoSprite == null) return;

        if (photoLagCoroutine != null)
            StopCoroutine(photoLagCoroutine);

        photoLagCoroutine = StartCoroutine(PhotoLagEffect(true));
    }

    /// <summary>
    /// Скрывает фото с эффектом лагов
    /// </summary>
    private void HidePhoto()
    {
        if (photoSprite == null) return;

        if (photoLagCoroutine != null)
            StopCoroutine(photoLagCoroutine);

        photoLagCoroutine = StartCoroutine(PhotoLagEffect(false));
    }

    /// <summary>
    /// Эффект лагов при появлении/исчезновении фото
    /// </summary>
    private IEnumerator PhotoLagEffect(bool show)
    {
        if (show)
        {
            // Эффект "лагов" при ПОЯВЛЕНИИ - быстрое мигание
            for (int i = 0; i < 3; i++)
            {
                photoSprite.enabled = true;
                yield return new WaitForSeconds(0.1f * lagIntensity);
                photoSprite.enabled = false;
                yield return new WaitForSeconds(0.05f * lagIntensity);
            }

            // Финальное появление
            photoSprite.enabled = true;
            Debug.Log("PhotoHall2: Фото показано с эффектом лагов");
        }
        else
        {
            // Эффект "лагов" при ИСЧЕЗНОВЕНИИ - быстрое мигание
            for (int i = 0; i < 4; i++)
            {
                photoSprite.enabled = false;
                yield return new WaitForSeconds(0.07f * lagIntensity);
                photoSprite.enabled = true;
                yield return new WaitForSeconds(0.04f * lagIntensity);
            }

            // Дополнительное быстрое мигание перед полным исчезновением
            for (int i = 0; i < 2; i++)
            {
                photoSprite.enabled = false;
                yield return new WaitForSeconds(0.05f * lagIntensity);
                photoSprite.enabled = true;
                yield return new WaitForSeconds(0.02f * lagIntensity);
            }

            // Финальное исчезновение
            photoSprite.enabled = false;
            Debug.Log("PhotoHall2: Фото скрыто с эффектом лагов");
        }
    }

    /// <summary>
    /// Вызывается когда диалог завершен
    /// </summary>
    private void OnDialogueEnd()
    {
        dialogueTriggered = false;

        // Отписываемся от события
        if (dialogueManager != null)
        {
            dialogueManager.OnDialogueIndexReached -= OnDialogueLineChanged;
        }

        // Гарантируем, что фото скрыто после диалога
        if (photoSprite != null)
        {
            photoSprite.enabled = false;
        }

        // Останавливаем корутину лагов
        if (photoLagCoroutine != null)
        {
            StopCoroutine(photoLagCoroutine);
        }

        Debug.Log("PhotoHall2: Диалог завершен");

        // После использования делаем объект полностью неинтерактивным
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
            gameObject.layer = LayerMask.NameToLayer("Default");
        }
    }

    void OnDestroy()
    {
        // Останавливаем все корутины при уничтожении
        StopAllCoroutines();

        // Отписываемся от события
        if (dialogueManager != null)
        {
            dialogueManager.OnDialogueIndexReached -= OnDialogueLineChanged;
        }
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