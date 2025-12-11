using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class DialogueTrigger0 : MonoBehaviour
{
    [Header("Dialogue Settings")]
    public DialogueManager0 dialogueManager;

    [TextArea(2, 5)]
    public List<string> dialogueLines;

    [Header("Black Screen Settings")]
    public Image blackScreenImage;
    public float blackScreenTime = 1f;
    public float fadeDuration = 2f;

    private bool dialogueStarted = false;
    private bool playerInside = false;
    private bool allowStart = false;

    // Событие окончания основного диалога
    public static System.Action OnDialogue0Finished;

    void Start()
    {
        StartCoroutine(StartSequence());
    }

    private IEnumerator StartSequence()
    {
        if (blackScreenImage != null)
        {
            blackScreenImage.gameObject.SetActive(true);
            blackScreenImage.color = new Color(0f, 0f, 0f, 1f);
        }

        yield return new WaitForSeconds(blackScreenTime);

        if (blackScreenImage != null)
            yield return StartCoroutine(FadeOut());

        allowStart = true;

        if (playerInside && !dialogueStarted)
        {
            StartMainDialogue();
        }
    }

    private void StartMainDialogue()
    {
        dialogueStarted = true;

        // ВАЖНО: теперь мы передаем callback завершения
        dialogueManager.StartDialogue(dialogueLines, OnMainDialogueEnd);
    }

    // === Это вызывается АВТОМАТИЧЕСКИ, когда DialogueManager0 завершает диалог ===
    private void OnMainDialogueEnd()
    {
        Debug.Log("DialogueTrigger0: основной диалог завершён!");

        OnDialogue0Finished?.Invoke(); // Отправляем сигнал Оферте
    }

    private IEnumerator FadeOut()
    {
        float timer = 0f;
        Color startColor = blackScreenImage.color;
        Color endColor = new Color(0, 0, 0, 0);

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            blackScreenImage.color = Color.Lerp(startColor, endColor, timer / fadeDuration);
            yield return null;
        }

        blackScreenImage.gameObject.SetActive(false);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInside = true;

        if (allowStart && !dialogueStarted)
        {
            StartMainDialogue();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            playerInside = false;
    }
}
