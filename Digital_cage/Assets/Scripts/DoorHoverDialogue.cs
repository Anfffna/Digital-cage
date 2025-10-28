using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;

public class DoorHoverDialogue : MonoBehaviour, IInteractable
{
    [Header("UI References")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;

    [Header("Hover Text")]
    [TextArea] public string hoverText;

    [Header("Interaction Text")]
    [TextArea] public string interactText;

    [Header("Typing Settings")]
    public float typingSpeed = 0.03f;
    public float displayTime = 2f;

    private Coroutine typingCoroutine;
    private bool hasInteracted = false;
    private bool isHovering = false;

    // Для InteractionController
    public string GetInteractionText()
    {
        if (!hasInteracted)
        {
            if (!isHovering)
            {
                isHovering = true;
                StartHover();
            }
        }
        return ""; // отключаем [E]
    }

    public void StopHover()
    {
        if (isHovering && !hasInteracted)
        {
            isHovering = false;
            StopCurrentTyping();
            dialoguePanel.SetActive(false);
        }
    }

    public void Interact()
    {
        if (!hasInteracted)
        {
            hasInteracted = true;

            // стопаем hover
            isHovering = false;
            StopCurrentTyping();

            // запускаем интеракт-текст
            typingCoroutine = StartCoroutine(ShowDialogue(interactText, true));
        }
    }

    private void StartHover()
    {
        StopCurrentTyping();
        typingCoroutine = StartCoroutine(ShowDialogue(hoverText, false));
    }

    private void StopCurrentTyping()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }
    }

    private IEnumerator ShowDialogue(string text, bool autoHide)
    {
        dialoguePanel.SetActive(true);
        dialogueText.alignment = TextAlignmentOptions.Center;
        dialogueText.richText = true;
        dialogueText.text = "";

        int i = 0;
        bool insideTag = false;

        while (i < text.Length)
        {
            char c = text[i];

            if (c == '<') insideTag = true;
            dialogueText.text += c;
            if (c == '>') insideTag = false;

            i++;
            if (!insideTag)
            {
                yield return new WaitForSeconds(typingSpeed);
                LayoutRebuilder.ForceRebuildLayoutImmediate(dialogueText.rectTransform);
            }
        }

        // для hover не скрываем панель, пока игрок наведен
        if (autoHide)
        {
            yield return new WaitForSeconds(displayTime);
            dialoguePanel.SetActive(false);
        }
    }
}





