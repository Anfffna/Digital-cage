using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MamaTalk : MonoBehaviour
{
    [Header("Dialogue Settings")]
    public ManagerDialogue7 dialogueManager; // Ссылка на ManagerDialogue7
    [TextArea(2, 5)]
    public List<string> dialogueLines; // Строки диалога

    [Header("Audio Settings")]
    public AudioSource audioSource;    // Аудиоисточник
    public AudioClip soundOnLine7;     // Звук на 7 реплике
    public int triggerSoundLine = 7;   // На какой реплике звук (можно менять)

    [Header("Dead Mama Transition")]
    public DeadMama deadMamaController; // Скрипт DeadMama
    public bool autoFindDeadMama = true; // Автоматически искать DeadMama

    private bool dialogueStarted = false;
    private bool playerInside = false;
    private bool soundPlayed = false;

    // Событие окончания диалога
    public static System.Action OnMamaTalkFinished;

    void Start()
    {
        // Проверка зависимостей
        if (dialogueManager == null)
        {
            Debug.LogError("MamaTalk: ManagerDialogue7 не назначен!");
            return;
        }

        // Автонаход DeadMama
        if (autoFindDeadMama && deadMamaController == null)
        {
            deadMamaController = FindObjectOfType<DeadMama>();
            if (deadMamaController != null)
            {
                Debug.Log("MamaTalk: Найден DeadMama автоматически");
            }
        }

        // Создаем AudioSource если нет
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
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
        soundPlayed = false;
        Debug.Log("MamaTalk: Запуск диалога");

        // Подписываемся на события диалога
        SubscribeToDialogueEvents();

        // Запускаем диалог
        dialogueManager.StartDialogue(dialogueLines, OnMainDialogueEnd);
    }

    void SubscribeToDialogueEvents()
    {
        // Отписываемся если уже подписаны
        dialogueManager.OnDialogueIndexReached -= OnDialogueLineChanged;

        // Подписываемся на событие смены строки
        dialogueManager.OnDialogueIndexReached += OnDialogueLineChanged;
    }

    void UnsubscribeFromDialogueEvents()
    {
        if (dialogueManager != null)
        {
            dialogueManager.OnDialogueIndexReached -= OnDialogueLineChanged;
        }
    }

    void OnDialogueLineChanged(int lineIndex)
    {
        Debug.Log($"MamaTalk: Текущая реплика {lineIndex}");

        // Проверяем достигли ли мы 7 реплики
        if (lineIndex == triggerSoundLine && !soundPlayed)
        {
            PlaySoundOnLine7();
        }
    }

    void PlaySoundOnLine7()
    {
        if (soundPlayed) return;

        soundPlayed = true;

        // ОСТАНАВЛИВАЕМ ЗВУК ТЕЛЕПОРТАЦИИ ИЗ DoorsClose
        DoorsClose doorsClose = FindObjectOfType<DoorsClose>();
        if (doorsClose != null)
        {
            doorsClose.StopTeleportAmbientSound();
            Debug.Log("MamaTalk: Звук телепортации остановлен");
        }

        if (audioSource != null && soundOnLine7 != null)
        {
            audioSource.PlayOneShot(soundOnLine7);
            Debug.Log($"MamaTalk: Проигрываю звук на реплике {triggerSoundLine}");
        }
        else
        {
            Debug.LogWarning("MamaTalk: Не могу проиграть звук - нет AudioSource или AudioClip");
        }
    }

    /// <summary>
    /// Callback, вызываемый ManagerDialogue7 при завершении диалога
    /// </summary>
    private void OnMainDialogueEnd()
    {
        Debug.Log("MamaTalk: Диалог завершён!");

        // Отписываемся от событий
        UnsubscribeFromDialogueEvents();

        // Отправляем событие завершения
        OnMamaTalkFinished?.Invoke();

        // Запускаем DeadMama последовательность
        StartDeadMamaSequence();

        // Деактивируем триггер если нужно
        gameObject.SetActive(false);
    }

    void StartDeadMamaSequence()
    {
        if (deadMamaController != null)
        {
            Debug.Log("MamaTalk: Запускаю DeadMama последовательность...");
            deadMamaController.StartDeadMamaSequence();
        }
        else
        {
            Debug.LogError("MamaTalk: DeadMama controller не назначен!");
        }
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
        soundPlayed = false;
        playerInside = false;

        UnsubscribeFromDialogueEvents();

        gameObject.SetActive(true);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInside = true;
        Debug.Log("MamaTalk: Игрок вошел в триггер");

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
            Debug.Log("MamaTalk: Игрок вышел из триггера");
        }
    }

    void OnDestroy()
    {
        UnsubscribeFromDialogueEvents();
    }

    void OnValidate()
    {
        // Автоматическое назначение ссылок в редакторе
        if (dialogueManager == null)
        {
            dialogueManager = FindObjectOfType<ManagerDialogue7>();
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