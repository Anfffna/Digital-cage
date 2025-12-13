using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BathroomSilhouette : MonoBehaviour
{
    [Header("Основные настройки")]
    [SerializeField] private GameObject silhouette;
    [SerializeField] private float hideDelay = 0.5f;

    [Header("Диалог")]
    [SerializeField] public ManagerDialogue2 dialogueManager;
    [SerializeField] public List<string> dialogueLines;

    [Header("Другие компоненты")]
    [SerializeField] private Collider bathroomTrigger;

    private bool hasTriggered = false;
    private Animator silhouetteAnimator;
    private Renderer silhouetteRenderer;
    private Transform player;
    private Camera playerCamera;
    private bool systemActivated = false;

    void Awake()
    {
        SetSilhouetteVisible(false);
        SetCollidersEnabled(false);
    }

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        playerCamera = Camera.main;

        silhouetteAnimator = silhouette.GetComponent<Animator>();
        silhouetteRenderer = silhouette.GetComponent<Renderer>();

        SetSilhouetteVisible(false);
        SetCollidersEnabled(false);
    }

    void Update()
    {
        // АКТИВИРУЕМСЯ ТОЛЬКО КОГДА НАЧИНАЕТСЯ ADVENT DOOR
        if (!systemActivated && IsAdventDoorReady())
        {
            systemActivated = true;
            StartCoroutine(ActivateSystem());
        }
    }

    IEnumerator ActivateSystem()
    {
        // Включаем коллайдер
        SetCollidersEnabled(true);

        // Ждем пока игрок НЕ смотрит на место силуэта
        yield return new WaitUntil(() => !IsPlayerLookingAtSilhouette());

        // Показываем силуэт
        ShowSilhouette();
    }

    void SetCollidersEnabled(bool enabled)
    {
        if (bathroomTrigger != null)
            bathroomTrigger.enabled = enabled;
    }

    bool IsPlayerLookingAtSilhouette()
    {
        if (playerCamera == null) return false;

        Vector3 directionToSilhouette = (transform.position - playerCamera.transform.position).normalized;
        float dot = Vector3.Dot(playerCamera.transform.forward, directionToSilhouette);

        return dot > 0.87f;
    }

    bool IsAnyDialogueActive()
    {
        if (dialogueManager != null && dialogueManager.dialoguePanel != null)
        {
            return dialogueManager.dialoguePanel.activeInHierarchy;
        }
        return false;
    }

    /// <summary>
    /// Проверяет, готов ли AdventDoor (двери появились)
    /// </summary>
    private bool IsAdventDoorReady()
    {
        AdventDoor adventDoor = FindObjectOfType<AdventDoor>();
        return adventDoor != null && adventDoor.DoorsSpawned;
    }

    void ShowSilhouette()
    {
        SetSilhouetteVisible(true);

        if (silhouetteAnimator != null)
        {
            silhouetteAnimator.Play("Idle", -1, 0f);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !hasTriggered && !IsAnyDialogueActive())
        {
            StartCoroutine(HideSilhouette());
        }
    }

    IEnumerator HideSilhouette()
    {
        hasTriggered = true;

        // Запускаем диалог если есть
        if (dialogueManager != null && dialogueLines != null && dialogueLines.Count > 0)
        {
            dialogueManager.StartDialogue(dialogueLines);
        }

        // Ждем задержку перед исчезновением
        yield return new WaitForSeconds(hideDelay);

        // Запускаем анимацию исчезновения
        if (silhouetteAnimator != null)
        {
            silhouetteAnimator.Play("Disappear", -1, 0f);
            yield return new WaitForSeconds(GetAnimationLength("Disappear"));
        }
        else
        {
            yield return new WaitForSeconds(1f);
        }

        // Навсегда скрываем силуэт
        SetSilhouetteVisible(false);
    }

    void SetSilhouetteVisible(bool visible)
    {
        if (silhouetteRenderer != null)
        {
            silhouetteRenderer.enabled = visible;
        }
    }

    float GetAnimationLength(string animationName)
    {
        if (silhouetteAnimator != null)
        {
            RuntimeAnimatorController ac = silhouetteAnimator.runtimeAnimatorController;
            if (ac != null)
            {
                foreach (AnimationClip clip in ac.animationClips)
                {
                    if (clip.name == animationName)
                    {
                        return clip.length;
                    }
                }
            }
        }
        return 1f;
    }
}