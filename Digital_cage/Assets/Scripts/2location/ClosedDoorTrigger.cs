using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ClosedDoorTrigger : MonoBehaviour
{
    [Header("Trigger Settings")]
    public bool triggerEnabled = false;

    [Header("Dialogue Settings")]
    public ManagerDialogue2 dialogueManager;
    [TextArea(2, 5)]
    public List<string> dialogueLines;

    [Header("Audio Settings")]
    public AudioSource audioSource;
    public int audioStartIndex = 5;

    private Collider triggerCollider;
    private bool hasBeenTriggered = false;
    private List<AudioSource> allAudioSources = new List<AudioSource>();
    private bool isOurDialogueActive = false; // Флаг что наш диалог активен

    void Start()
    {
        triggerCollider = GetComponent<Collider>();
        if (triggerCollider != null)
        {
            triggerCollider.enabled = false;
        }

        if (dialogueManager == null)
            dialogueManager = FindObjectOfType<ManagerDialogue2>();

        if (dialogueManager != null)
        {
            dialogueManager.OnDialogueIndexReached += OnDialogueIndexReached;
        }

        if (audioSource != null)
        {
            audioSource.Stop();
        }
    }

    void Update()
    {
        if (triggerEnabled && triggerCollider != null && !triggerCollider.enabled && !hasBeenTriggered)
        {
            triggerCollider.enabled = true;
        }
    }

    public void EnableTrigger()
    {
        triggerEnabled = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!triggerEnabled || hasBeenTriggered) return;

        if (other.CompareTag("Player"))
        {
            hasBeenTriggered = true;
            StartDoorDialogue();

            if (triggerCollider != null)
            {
                triggerCollider.enabled = false;
            }
        }
    }

    private void StartDoorDialogue()
    {
        if (dialogueLines != null && dialogueLines.Count > 0 && dialogueManager != null)
        {
            isOurDialogueActive = true; // Помечаем что наш диалог начался
            dialogueManager.StartDialogue(dialogueLines, OnDoorDialogueEnd);
        }
        else
        {
            OnDoorDialogueEnd();
        }
    }

    private void OnDialogueIndexReached(int currentIndex)
    {
        // Проверяем что это именно НАШ диалог и нужный индекс
        if (isOurDialogueActive && currentIndex == audioStartIndex)
        {
            PlaySpecialAudio();
        }
    }

    private void PlaySpecialAudio()
    {
        if (audioSource != null && audioSource.clip != null)
        {
            AudioSource[] allSources = FindObjectsOfType<AudioSource>();
            allAudioSources.Clear();

            foreach (AudioSource source in allSources)
            {
                if (source != audioSource && source.isPlaying)
                {
                    allAudioSources.Add(source);
                    source.Stop();
                }
            }

            audioSource.Play();
        }
    }

    private void RestoreAllAudio()
    {
        foreach (AudioSource source in allAudioSources)
        {
            if (source != null)
            {
                source.Play();
            }
        }
        allAudioSources.Clear();
    }

    private void OnDoorDialogueEnd()
    {
        isOurDialogueActive = false; // Сбрасываем флаг когда диалог завершен

        RestoreAllAudio();

        if (dialogueManager != null)
        {
            dialogueManager.OnDialogueIndexReached -= OnDialogueIndexReached;
        }

        ScaryToDo scaryToDo = FindObjectOfType<ScaryToDo>();
        if (scaryToDo != null)
        {
            scaryToDo.ShowTaskList();
        }

        AdventDoor adventDoor = FindObjectOfType<AdventDoor>();
        if (adventDoor != null)
        {
            adventDoor.ForceSpawnDoors();
        }
    }

    void OnDestroy()
    {
        if (dialogueManager != null)
        {
            dialogueManager.OnDialogueIndexReached -= OnDialogueIndexReached;
        }
    }
}