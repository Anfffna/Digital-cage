using UnityEngine;
using System.Collections.Generic;

public class DialogueTrigger : MonoBehaviour
{
    public DialogueManager dialogueManager;
    [TextArea(2, 5)] public List<string> dialogueLines; // Твои фразы

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            dialogueManager.StartDialogue(dialogueLines);
            gameObject.SetActive(false); // Чтобы триггер сработал 1 раз
        }
    }
}
