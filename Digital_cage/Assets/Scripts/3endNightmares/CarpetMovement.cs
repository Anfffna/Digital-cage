using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class CarpetMovement : MonoBehaviour, IInteractable
{
    [Header("Animation Settings")]
    public Animator carpetAnimator;

    [Header("Dialogue Settings")]
    public ManagerDialogue3 dialogueManager;
    [TextArea(2, 5)]
    public List<string> dialogueLines;

    [Header("Todo Settings")]
    public LightTodoUIManager todoManager;

    [Header("Audio Settings")]
    public AudioClip carpetSound;
    public AudioSource audioSource;

    private bool hasBeenActivated = false;
    private bool isInteractable = false;
    private bool dialogueTriggered = false;

    void Start()
    {
        gameObject.layer = LayerMask.NameToLayer("Default");

        Collider collider = GetComponent<Collider>();
        if (collider != null)
        {
            collider.enabled = false;
        }

        if (carpetAnimator != null)
        {
            carpetAnimator.Play("idle");
        }

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            Debug.Log("CarpetMovement: AudioSource создан автоматически");
        }

        audioSource.playOnAwake = false;
        audioSource.loop = false;

        Debug.Log("CarpetMovement: Ковер заблокирован при старте");

        InvokeRepeating("CheckTodoState", 1f, 0.5f);
    }

    private void CheckTodoState()
    {
        if (todoManager == null || hasBeenActivated || isInteractable) return;

        bool isNewTaskActive = todoManager.IsNewTaskActive();
        Debug.Log($"CarpetMovement: CheckTodoState() - isNewTaskActive = {isNewTaskActive}");

        if (isNewTaskActive)
        {
            UnlockCarpet();
            CancelInvoke("CheckTodoState");
        }
    }

    private void UnlockCarpet()
    {
        isInteractable = true;

        Collider collider = GetComponent<Collider>();
        if (collider != null)
        {
            collider.enabled = true;
        }

        gameObject.layer = LayerMask.NameToLayer("Interactable");

        Debug.Log("CarpetMovement: Ковер РАЗБЛОКИРОВАН после появления newTaskText!");
    }

    public string GetInteractionText()
    {
        if (!isInteractable || hasBeenActivated)
        {
            return "";
        }

        return "Нажмите E";
    }

    public void Interact()
    {
        if (!isInteractable || hasBeenActivated || dialogueTriggered)
        {
            Debug.Log("CarpetMovement: Взаимодействие заблокировано");
            return;
        }

        Debug.Log("CarpetMovement: Взаимодействие с ковром начато");
        dialogueTriggered = true;

        // УБРАТЬ отсюда проигрывание звука

        if (dialogueManager != null && dialogueLines != null && dialogueLines.Count > 0)
        {
            dialogueManager.StartDialogue(dialogueLines, OnDialogueEnd);
        }
        else
        {
            ActivateCarpetMovement();
        }
    }

    private void OnDialogueEnd()
    {
        ActivateCarpetMovement();
        dialogueTriggered = false;
    }

    private void ActivateCarpetMovement()
    {
        hasBeenActivated = true;
        isInteractable = false;

        if (carpetAnimator != null)
        {
            carpetAnimator.Play("movement");

            // Проигрываем звук когда начинается анимация movement
            if (carpetSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(carpetSound);
                Debug.Log("CarpetMovement: Проигрывается звук ковра при начале анимации movement");
            }
            else
            {
                if (carpetSound == null)
                {
                    Debug.LogWarning("CarpetMovement: CarpetSound не назначен!");
                }
                if (audioSource == null)
                {
                    Debug.LogWarning("CarpetMovement: AudioSource не найден!");
                }
            }
        }

        gameObject.layer = LayerMask.NameToLayer("Default");

        Debug.Log("CarpetMovement: Ковер активирован!");
    }

    void OnDestroy()
    {
        CancelInvoke("CheckTodoState");
    }
}