using UnityEngine;
using System.Collections.Generic;

public class GameMachine : MonoBehaviour, IInteractable
{
    [Header("Dialogue Settings")]
    public ManagerDialogue3 dialogueManager;
    [TextArea(2, 5)]
    public List<string> dialogueLines;

    private bool hasBeenUsed = false;
    private bool dialogueTriggered = false;

    void Start()
    {
        // Устанавливаем слой Interactable
        gameObject.layer = LayerMask.NameToLayer("Interactable");
    }

    public string GetInteractionText()
    {
        if (hasBeenUsed)
            return "";

        return "Нажмите E";
    }

    public void Interact()
    {
        if (hasBeenUsed || dialogueTriggered) return;

        dialogueTriggered = true;

        // Запускаем диалог
        if (dialogueManager != null && dialogueLines != null && dialogueLines.Count > 0)
        {
            dialogueManager.StartDialogue(dialogueLines, OnDialogueEnd);
        }
        else
        {
            // Если диалога нет, сразу завершаем взаимодействие
            OnDialogueEnd();
        }
    }

    private void OnDialogueEnd()
    {
        // После завершения диалога делаем объект неинтерактивным
        hasBeenUsed = true;
        dialogueTriggered = false;

        // Меняем слой чтобы нельзя было взаимодействовать повторно
        gameObject.layer = LayerMask.NameToLayer("Default");

        Debug.Log("GameMachine: Взаимодействие завершено");
    }
}