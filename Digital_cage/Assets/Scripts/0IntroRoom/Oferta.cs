using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Oferta : MonoBehaviour, IInteractable
{
    [Header("Oferta Sprite (SpriteRenderer)")]
    public SpriteRenderer ofertaSprite;

    [Header("Next Dialogue Settings")]
    public DialogueManager0 dialogueManager;

    [TextArea(2, 5)]
    public List<string> nextDialogueLines;

    [Header("Cursor Manager")]
    public CursorUI cursorManager; // привяжи через инспектор

    [Header("Audio Settings")]
    public AudioSource audioSource;      // источник звука
    public AudioClip spriteAppearClip;   // звук появления спрайта

    public float spriteDelay = 2f;   // задержка появления спрайта
    public float dialogueDelay = 3f; // задержка старта диалога после DialogueTrigger0

    [Header("UI Screen Settings")]
    public GameObject ofertaUIScreen; // UI, который открывается по клику

    private bool isInteractableReady = false;
    private bool hasInteracted = false; // чтобы оферта использовалась только один раз

    void Start()
    {
        if (ofertaSprite != null)
            ofertaSprite.gameObject.SetActive(false);

        if (ofertaUIScreen != null)
            ofertaUIScreen.SetActive(false);

        gameObject.layer = LayerMask.NameToLayer("Default");

        DialogueTrigger0.OnDialogue0Finished += OnDialogue0End;
    }

    private void OnDestroy()
    {
        DialogueTrigger0.OnDialogue0Finished -= OnDialogue0End;
    }

    private void OnDialogue0End()
    {
        StartCoroutine(StartOfertaSequence());
    }

    private IEnumerator StartOfertaSequence()
    {
        yield return new WaitForSeconds(spriteDelay);

        // Появление спрайта
        if (ofertaSprite != null)
        {
            ofertaSprite.gameObject.SetActive(true);

            // Проигрываем звук появления
            if (audioSource != null && spriteAppearClip != null)
                audioSource.PlayOneShot(spriteAppearClip);
        }

        Debug.Log("Oferta: спрайт оферты показан!");

        // Делаем объект интерактивным
        isInteractableReady = true;
        gameObject.layer = LayerMask.NameToLayer("Interactable");

        // Ждем оставшееся время перед запуском диалога
        float remainingDelay = Mathf.Max(0f, dialogueDelay - spriteDelay);
        yield return new WaitForSeconds(remainingDelay);

        // Запускаем диалог оферты автоматически
        if (dialogueManager != null && nextDialogueLines.Count > 0)
        {
            dialogueManager.StartDialogue(nextDialogueLines);
            Debug.Log("Oferta: диалог оферты начат!");
        }
    }

    // ------------------------------
    // Реализация IInteractable
    // ------------------------------
    public void Interact()
    {
        if (!isInteractableReady || hasInteracted) return;

        hasInteracted = true; // оферта используется один раз

        // После первого взаимодействия делаем объект неинтерактивным
        isInteractableReady = false;
        gameObject.layer = LayerMask.NameToLayer("Default");

        // Включаем UI
        if (ofertaUIScreen != null)
            ofertaUIScreen.SetActive(true);

        // Показываем курсор
        if (cursorManager != null)
            cursorManager.ShowCursor();

        // Запускаем диалог на UI
        EntryDialogue entry = ofertaUIScreen.GetComponent<EntryDialogue>();
        if (entry != null)
        {
            entry.dialogueManager = this.dialogueManager;
            entry.StartDialogue();
        }

        Debug.Log("Oferta: игрок нажал E, UI оферты включен, диалог стартует");
    }

    public string GetInteractionText()
    {
        if (!isInteractableReady || hasInteracted) return "";
        return "Нажмите E, чтобы взаимодействовать";
    }
}
