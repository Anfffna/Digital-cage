using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LightSwitch : MonoBehaviour, IInteractable
{
    [Header("Dialogue Settings")]
    public ManagerDialogue3 dialogueManager;
    [TextArea(2, 5)]
    public List<string> dialogueLines;

    [Header("Todo Settings")]
    public LightTodoUIManager todoManager;

    [Header("Audio Settings")]
    public AudioClip switchOnSound;
    public AudioSource audioSource;

    private bool isInteractable = false;
    private bool hasBeenUsed = false;
    private bool dialogueTriggered = false;

    void Start()
    {
        gameObject.layer = LayerMask.NameToLayer("Interactable");
        SetInteractable(false);

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            Debug.Log("LightSwitch: AudioSource создан автоматически");
        }

        audioSource.playOnAwake = false;
        audioSource.loop = false;

        StartCoroutine(CheckTodoUIAvailability());
    }

    private IEnumerator CheckTodoUIAvailability()
    {
        while (todoManager == null)
        {
            yield return new WaitForSeconds(0.5f);
            todoManager = FindObjectOfType<LightTodoUIManager>();
        }

        while (!isInteractable && !hasBeenUsed)
        {
            if (todoManager != null)
            {
                if (IsTodoUIActive())
                {
                    SetInteractable(true);
                    break;
                }
            }
            yield return new WaitForSeconds(0.3f);
        }
    }

    private bool IsTodoUIActive()
    {
        if (todoManager == null) return false;

        var todoManagerType = todoManager.GetType();
        var isShowingField = todoManagerType.GetField("isShowing",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (isShowingField != null)
        {
            return (bool)isShowingField.GetValue(todoManager);
        }

        return false;
    }

    public string GetInteractionText()
    {
        if (!isInteractable || hasBeenUsed)
        {
            return "";
        }

        return "Нажмите E";
    }

    public void Interact()
    {
        if (!isInteractable || hasBeenUsed || dialogueTriggered)
        {
            return;
        }

        if (switchOnSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(switchOnSound);
            Debug.Log("LightSwitch: Проигрывается звук включения выключателя");
        }
        else
        {
            if (switchOnSound == null)
            {
                Debug.LogWarning("LightSwitch: SwitchOnSound не назначен!");
            }
            if (audioSource == null)
            {
                Debug.LogWarning("LightSwitch: AudioSource не найден!");
            }
        }

        SetInteractable(false);
        hasBeenUsed = true;
        dialogueTriggered = true;

        if (dialogueManager != null && dialogueLines != null && dialogueLines.Count > 0)
        {
            dialogueManager.StartDialogue(dialogueLines, OnDialogueEnd);
        }
        else
        {
            CompleteTask();
        }
    }

    private void OnDialogueEnd()
    {
        CompleteTask();
        dialogueTriggered = false;
    }

    private void CompleteTask()
    {
        if (todoManager != null)
        {
            todoManager.CompleteLightTask();
        }
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