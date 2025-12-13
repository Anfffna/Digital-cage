using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using TMPro;

public class PhotoHall3 : MonoBehaviour, IInteractable
{
    [Header("Dialogue Settings")]
    public ManagerDialogue2 dialogueManager;

    [Header("Photo Hall Dialogue")]
    [TextArea(2, 5)]
    public List<string> photoHallDialogueTexts;

    [Header("Todo Settings")]
    public TodoUIManager todoManager;
    public int requiredTodoIndex = 0;

    private bool isInteractable = false;
    private bool dialogueTriggered = false;
    public bool hasBeenUsed = false;

    void Start()
    {
        gameObject.layer = LayerMask.NameToLayer("Interactable");
        SetInteractable(false);
        StartCoroutine(CheckTodoCompletion());
    }

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
                    Debug.Log($"PhotoHall3: Объект теперь интерактивен! Задача {requiredTodoIndex} выполнена.");
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

        if (dialogueManager != null)
        {
            dialogueManager.OnDialogueIndexReached += OnDialogueLineChanged;
        }

        if (dialogueManager != null && photoHallDialogueTexts != null && photoHallDialogueTexts.Count > 0)
        {
            dialogueTriggered = true;
            dialogueManager.StartDialogue(photoHallDialogueTexts, OnDialogueEnd);
        }
    }

    // Оставил метод на случай если нужно будет добавлять логику на определенных репликах
    private void OnDialogueLineChanged(int currentLineIndex)
    {
        // Здесь можно добавить другую логику на определенных репликах
        // Например, звуковые эффекты или события
    }

    private void OnDialogueEnd()
    {
        dialogueTriggered = false;

        if (dialogueManager != null)
        {
            dialogueManager.OnDialogueIndexReached -= OnDialogueLineChanged;
        }

        Debug.Log("PhotoHall3: Диалог завершен");
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

        if (dialogueManager != null)
        {
            dialogueManager.OnDialogueIndexReached -= OnDialogueLineChanged;
        }
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