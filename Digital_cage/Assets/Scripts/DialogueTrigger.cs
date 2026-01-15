using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DialogueTrigger : MonoBehaviour
{
    [Header("Dialogue Settings")]
    public DialogueManager dialogueManager;

    [TextArea(2, 5)]
    public List<string> dialogueLines;

    [Header("Вторая часть начинается с этого индекса")]
    public int continueFromLine = 9;

    [Header("Hand / Phone Controller")]
    public HandPhoneController handPhoneController;

    [Header("Dialogue Trigger 2 Reference")]
    public DialogueTrigger2 dialogueTrigger2;

    [Header("Audio Settings")]
    public AudioSource audioSource;        // Аудиоисточник
    public AudioClip triggerSound;         // Звук при активации триггера

    private bool triggered = false;

    void Start()
    {
        // Автоматическое создание AudioSource если нет
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !triggered)
        {
            triggered = true;

            // Проигрываем звук при активации
            PlayTriggerSound();

            // Первая часть диалога до continueFromLine
            List<string> firstPart = dialogueLines.GetRange(0, continueFromLine);
            dialogueManager.StartDialogue(firstPart, OnDialogueLineFinished, true);
        }
    }

    private void PlayTriggerSound()
    {
        if (audioSource != null && triggerSound != null)
        {
            audioSource.PlayOneShot(triggerSound);
            Debug.Log("DialogueTrigger: Звук активации проигран");
        }
    }

    private void OnDialogueLineFinished(int lineIndex)
    {
        if (handPhoneController != null)
            handPhoneController.OnDialogueLineFinished(lineIndex);

        ChairSit chairSit = FindObjectOfType<ChairSit>();
        if (chairSit != null)
            chairSit.OnDialogueLineFinished(lineIndex);
    }

    // Вызывается секретаршей после ухода
    public void AllowSecondPartDialogue()
    {
        Debug.Log($"=== AllowSecondPartDialogue ВЫЗВАН ===");
        Debug.Log($"Caller: {new System.Diagnostics.StackTrace().GetFrame(1).GetMethod().Name}");

        ChairSit chair = FindObjectOfType<ChairSit>();
        if (chair != null)
            chair.DisableInteractionAfterSecretaryLeft();

        // Передаем управление второму триггеру
        if (dialogueTrigger2 != null)
        {
            dialogueTrigger2.StartSecondPartDialogue();
        }
        else
        {
            Debug.LogWarning("DialogueTrigger: DialogueTrigger2 не привязан!");
        }
    }
}