using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BathroomSilhouette : MonoBehaviour
{
    [Header("Основные настройки")]
    [SerializeField] private GameObject silhouette;
    [SerializeField] private float hideDelay = 0.5f; // Задержка перед исчезновением

    [Header("Диалог")]
    [SerializeField] private ManagerDialogue2 dialogueManager;
    [SerializeField] private List<string> dialogueLines; // Список строк диалога

    private bool hasTriggered = false;
    private Animator silhouetteAnimator;
    private Renderer silhouetteRenderer;

    void Start()
    {
        silhouetteAnimator = silhouette.GetComponent<Animator>();
        silhouetteRenderer = silhouette.GetComponent<Renderer>();

        // Силуэт изначально ВИДИМ
        SetSilhouetteVisible(true);

        // Включаем анимацию покоя
        if (silhouetteAnimator != null)
        {
            silhouetteAnimator.Play("Idle", -1, 0f);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // Если игрок зашел в зону и еще не срабатывало
        if (other.CompareTag("Player") && !hasTriggered)
        {
            StartCoroutine(HideSilhouette());
        }
    }

    IEnumerator HideSilhouette()
    {
        hasTriggered = true;

        Debug.Log("Игрок вошел в зону - скрываем силуэт");

        // Запускаем диалог если есть DialogueManager и строки
        if (dialogueManager != null && dialogueLines != null && dialogueLines.Count > 0)
        {
            dialogueManager.StartDialogue(dialogueLines);
            Debug.Log("Запущен диалог");
        }

        // Ждем небольшую задержку перед исчезновением (параллельно с диалогом)
        yield return new WaitForSeconds(hideDelay);

        // Запускаем анимацию исчезновения (параллельно с диалогом)
        if (silhouetteAnimator != null)
        {
            silhouetteAnimator.Play("Disappear", -1, 0f);
            yield return new WaitForSeconds(GetAnimationLength("Disappear"));
        }
        else
        {
            // Если нет аниматора, просто ждем
            yield return new WaitForSeconds(1f);
        }

        // Навсегда скрываем силуэт (диалог может еще продолжаться)
        SetSilhouetteVisible(false);
        Debug.Log("Силуэт скрыт навсегда");
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