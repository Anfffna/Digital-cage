using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ShadowBasement : MonoBehaviour, IInteractable
{
    [Header("Dialogue Settings")]
    public ManagerDialogue3 dialogueManager;
    [TextArea(2, 5)]
    public List<string> dialogueLines;

    [Header("Glitch Effects")]
    public float glitchIntensity = 1f;
    public float minGlitchInterval = 0.05f;
    public float maxGlitchInterval = 0.15f;

    [Header("Position Glitch")]
    public float positionGlitchAmount = 10f; // Увеличил в 10 раз!
    public float positionGlitchChance = 0.3f;

    [Header("Scale Glitch")]
    public float scaleGlitchAmount = 0.3f; // Увеличил в 3 раза!
    public float scaleGlitchChance = 0.2f;

    [Header("Alpha Glitch")]
    public float alphaGlitchChance = 0.1f;

    private bool hasBeenUsed = false;
    private bool dialogueTriggered = false;
    private bool isHidden = false;
    private Coroutine glitchCoroutine;
    private CanvasGroup canvasGroup;
    private Renderer objectRenderer;
    private Vector3 originalPosition;
    private Vector3 originalScale;

    void Start()
    {
        gameObject.layer = LayerMask.NameToLayer("Interactable");
        originalPosition = transform.localPosition;
        originalScale = transform.localScale;

        canvasGroup = GetComponent<CanvasGroup>();
        objectRenderer = GetComponent<Renderer>();

        glitchCoroutine = StartCoroutine(GlitchEffect());
    }

    public string GetInteractionText()
    {
        if (hasBeenUsed || isHidden)
            return "";

        return "Нажмите E";
    }

    public void Interact()
    {
        if (hasBeenUsed || dialogueTriggered || isHidden) return;

        dialogueTriggered = true;

        // НЕМЕДЛЕННО меняем слой на одноразовое взаимодействие
        gameObject.layer = LayerMask.NameToLayer("Default");

        if (dialogueManager != null && dialogueLines != null && dialogueLines.Count > 0)
        {
            dialogueManager.OnDialogueIndexReached += OnDialogueIndexReached;
            dialogueManager.StartDialogue(dialogueLines, OnDialogueEnd);
        }
        else
        {
            OnDialogueEnd();
        }
    }

    private void OnDialogueIndexReached(int lineIndex)
    {
        if (lineIndex == 4 && !isHidden)
        {
            HideObjectPermanently();
        }
    }

    private void HideObjectPermanently()
    {
        isHidden = true;

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }
        else if (objectRenderer != null)
        {
            objectRenderer.enabled = false;
        }
        else
        {
            gameObject.SetActive(false);
        }

        transform.localPosition = originalPosition;
        transform.localScale = originalScale;
    }

    private void OnDialogueEnd()
    {
        if (dialogueManager != null)
        {
            dialogueManager.OnDialogueIndexReached -= OnDialogueIndexReached;
        }

        hasBeenUsed = true;
        dialogueTriggered = false;
        // Слой уже изменен в методе Interact(), так что здесь не нужно менять слой повторно
    }

    private IEnumerator GlitchEffect()
    {
        while (true)
        {
            if (isHidden) yield break;

            // БЫСТРЫЕ и ЗАМЕТНЫЕ глитчи
            float glitchDelay = Random.Range(minGlitchInterval, maxGlitchInterval);
            yield return new WaitForSeconds(glitchDelay);

            if (!isHidden)
            {
                ExecuteRandomGlitch();
            }
        }
    }

    private void ExecuteRandomGlitch()
    {
        int glitchType = Random.Range(0, 3);

        switch (glitchType)
        {
            case 0: // РЕЗКОЕ смещение позиции
                if (Random.value <= positionGlitchChance)
                {
                    Vector3 glitchPosition = new Vector3(
                        originalPosition.x + Random.Range(-positionGlitchAmount, positionGlitchAmount),
                        originalPosition.y + Random.Range(-positionGlitchAmount, positionGlitchAmount),
                        originalPosition.z
                    );
                    transform.localPosition = glitchPosition;

                    // Возвращаем обратно через короткое время
                    StartCoroutine(ResetPositionAfterDelay(0.08f));
                }
                break;

            case 1: // РЕЗКОЕ изменение размера
                if (Random.value <= scaleGlitchChance)
                {
                    Vector3 glitchScale = new Vector3(
                        originalScale.x * (1f + Random.Range(-scaleGlitchAmount, scaleGlitchAmount)),
                        originalScale.y * (1f + Random.Range(-scaleGlitchAmount, scaleGlitchAmount)),
                        originalScale.z
                    );
                    transform.localScale = glitchScale;

                    // Возвращаем обратно через короткое время
                    StartCoroutine(ResetScaleAfterDelay(0.08f));
                }
                break;

            case 2: // Мигание
                if (Random.value <= alphaGlitchChance)
                {
                    StartCoroutine(ExecuteFlicker());
                }
                break;
        }
    }

    private IEnumerator ResetPositionAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (!isHidden)
        {
            transform.localPosition = originalPosition;
        }
    }

    private IEnumerator ResetScaleAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (!isHidden)
        {
            transform.localScale = originalScale;
        }
    }

    private IEnumerator ExecuteFlicker()
    {
        if (isHidden) yield break;

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            yield return new WaitForSeconds(0.05f);
            if (!isHidden) canvasGroup.alpha = 1f;
        }
        else if (objectRenderer != null)
        {
            objectRenderer.enabled = false;
            yield return new WaitForSeconds(0.05f);
            if (!isHidden) objectRenderer.enabled = true;
        }
    }

    void OnDestroy()
    {
        if (dialogueManager != null)
        {
            dialogueManager.OnDialogueIndexReached -= OnDialogueIndexReached;
        }

        if (glitchCoroutine != null)
        {
            StopCoroutine(glitchCoroutine);
        }
    }
}