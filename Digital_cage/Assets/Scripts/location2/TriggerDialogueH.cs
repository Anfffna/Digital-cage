using UnityEngine;
using System.Collections.Generic;

public class TriggerDialogueH : MonoBehaviour
{
    [Header("Dialogue Settings")]
    public ManagerDialogue2 dialogueManager;

    [TextArea(2, 5)]
    public List<string> dialogueLines;

    private bool triggered = false;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !triggered)
        {
            triggered = true;
            dialogueManager.StartDialogue(dialogueLines);
        }
    }
}