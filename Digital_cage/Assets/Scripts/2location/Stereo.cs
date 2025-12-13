using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using TMPro;

public class Stereo : MonoBehaviour, IInteractable
{
    [Header("Dialogue Settings")]
    public ManagerDialogue2 dialogueManager;

    [Header("Stereo Dialogue")]
    [TextArea(2, 5)]
    public List<string> stereoDialogueTexts;

    [Header("Audio Settings")]
    public AudioSource audioSource;
    public AudioClip firstAudioClip;
    public AudioClip secondAudioClip;
    public float delayBetweenAudios = 0.5f;

    [Header("Todo Settings")]
    public TodoUIManager todoManager;
    public int requiredTodoIndex = 0;

    private bool isInteractable = false;
    private bool dialogueTriggered = false;
    public bool hasBeenUsed = false;

    void Start()
    {
        // Устанавливаем слой Interactable
        gameObject.layer = LayerMask.NameToLayer("Interactable");

        // Изначально делаем объект неинтерактивным
        SetInteractable(false);

        // Автоматически находим AudioSource если не назначен
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }

        // Начинаем проверку выполнения задачи в туду
        StartCoroutine(CheckTodoCompletion());
    }

    /// <summary>
    /// Проверяет выполнение задачи в туду менеджере
    /// </summary>
    private IEnumerator CheckTodoCompletion()
    {
        while (todoManager == null)
        {
            yield return new WaitForSeconds(0.5f);
            todoManager = FindObjectOfType<TodoUIManager>();
        }

        while (!isInteractable && !hasBeenUsed)
        {
            if (todoManager != null)
            {
                if (IsTodoItemCompleted(requiredTodoIndex))
                {
                    SetInteractable(true);
                    Debug.Log($"Stereo: Объект теперь интерактивен! Задача {requiredTodoIndex} выполнена.");
                    break;
                }
            }
            yield return new WaitForSeconds(0.3f);
        }
    }

    private bool IsTodoItemCompleted(int index)
    {
        if (todoManager == null || todoManager.todoItems == null) return false;
        if (index < 0 || index >= todoManager.todoItems.Length) return false;

        TextMeshProUGUI todoItem = todoManager.todoItems[index];
        return todoItem.text.StartsWith("<s>");
    }

    public string GetInteractionText()
    {
        if (!isInteractable || hasBeenUsed)
            return "";

        return "Нажмите E";
    }

    public void Interact()
    {
        if (!isInteractable || hasBeenUsed || dialogueTriggered) return;

        SetInteractable(false);
        hasBeenUsed = true;

        // Останавливаем фоновую музыку перед началом аудио
        StopBackgroundMusic();

        // Запускаем аудио и диалог
        if (dialogueManager != null && stereoDialogueTexts != null && stereoDialogueTexts.Count > 0)
        {
            dialogueTriggered = true;

            // Запускаем воспроизведение аудио
            StartCoroutine(PlayAudioSequence());

            // Запускаем диалог
            dialogueManager.StartDialogue(stereoDialogueTexts, OnDialogueEnd);
        }
    }

    /// <summary>
    /// Останавливает фоновую музыку
    /// </summary>
    private void StopBackgroundMusic()
    {
        MusicController musicController = FindObjectOfType<MusicController>();
        if (musicController != null)
        {
            musicController.StopMusic();
            Debug.Log("?? Фоновая музыка остановлена");
        }
    }

    /// <summary>
    /// Воспроизводит два аудиофайла по порядку
    /// </summary>
    private IEnumerator PlayAudioSequence()
    {
        if (audioSource == null) yield break;

        // Первое аудио
        if (firstAudioClip != null)
        {
            audioSource.clip = firstAudioClip;
            audioSource.Play();
            Debug.Log("?? Воспроизводится первое аудио");

            // Ждем окончания первого аудио
            yield return new WaitForSeconds(firstAudioClip.length);

            // Небольшая пауза между аудио
            yield return new WaitForSeconds(delayBetweenAudios);
        }

        // Второе аудио
        if (secondAudioClip != null)
        {
            audioSource.clip = secondAudioClip;
            audioSource.Play();
            Debug.Log("?? Воспроизводится второе аудио");
        }
    }

    private void OnDialogueEnd()
    {
        dialogueTriggered = false;
        Debug.Log("Stereo: Диалог завершен");
        SetInteractable(false);
    }

    private void SetInteractable(bool interactable)
    {
        isInteractable = interactable;

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
        StopAllCoroutines();
    }

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