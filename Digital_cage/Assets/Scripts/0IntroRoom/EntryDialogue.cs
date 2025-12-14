using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntryDialogue : MonoBehaviour
{
    [Header("Dialogue Settings")]
    public DialogueManager0 dialogueManager;
    public SignatureDrawerTexture signatureDrawer;

    [TextArea(2, 5)]
    public List<string> dialogueLines;

    [Header("Oferta Settings")]
    public GameObject ofertaSprite; // спрайт оферты
    public float ofertaSpriteDelay = 2f;   // через сколько показать спрайт
    public float dialogueDelayAfterSprite = 1f; // через сколько запустить диалог после спрайта

    [Header("Black Screen Settings")]
    public BlackScreenController blackScreenController;

    [Header("Cursor Manager")]
    public CursorUI cursorManager;

    // В начале класса EntryDialogue
    public event System.Action OnSignatureCompleted;


    private bool signatureCompleted = false;
    public bool SignatureCompleted => signatureCompleted;
    private bool waitingForBlackScreen = false;

    // ------------------------------------------
    // Внешний метод для открытия UI оферты
    // ------------------------------------------
    public void OpenUI()
    {
        // 1. Активируем UI оферты
        gameObject.SetActive(true);  // EntryDialogue / UI панель должна быть активной

        // 2. Показываем курсор
        if (cursorManager != null)
            cursorManager.ShowCursor();

        // 3. Если есть спрайт оферты, запускаем корутину
        if (ofertaSprite != null)
            StartCoroutine(ShowSpriteThenDialogue());
        else
            StartDialogue();
    }


    private IEnumerator ShowSpriteThenDialogue()
    {
        // Ждем перед показом спрайта
        yield return new WaitForSeconds(ofertaSpriteDelay);

        ofertaSprite.SetActive(true);
        Debug.Log("EntryDialogue: Спрайт оферты показан!");

        // Ждем перед стартом диалога
        yield return new WaitForSeconds(dialogueDelayAfterSprite);

        StartDialogue();
    }

    // ------------------------------------------
    // Основной диалог EntryDialogue
    // ------------------------------------------
    public void StartDialogue()
    {
        if (dialogueManager == null)
        {
            Debug.LogWarning("EntryDialogue: DialogueManager не привязан!");
            return;
        }

        Debug.Log("=== НАЧАЛО ДИАЛОГА ===");

        // Запускаем диалог до 4 индекса включительно
        List<string> initialLines = new List<string>();
        for (int i = 0; i <= 4 && i < dialogueLines.Count; i++)
            initialLines.Add(dialogueLines[i]);

        dialogueManager.StartDialogue(initialLines, () => OnLineFinishedWrapper(initialLines));
    }

    private void OnLineFinishedWrapper(List<string> lines)
    {
        Debug.Log("EntryDialogue: Завершение начальных линий");

        if (!gameObject.activeInHierarchy)  // если объект неактивен — включаем
            gameObject.SetActive(true);

        if (lines.Count == 0) return;
        int lastIndex = lines.Count - 1;

        if (lastIndex >= 4 && signatureDrawer != null)
        {
            signatureDrawer.gameObject.SetActive(true);
            StartCoroutine(CheckForSignature());
        }
    }


    private void OnRemainingLinesFinishedWrapper(List<string> lines)
    {
        if (lines.Count >= 2)
        {
            waitingForBlackScreen = true;
            Debug.Log("EntryDialogue: Запускаем черный экран после оставшихся линий");
        }
    }

    private IEnumerator CheckForSignature()
    {
        while (!signatureCompleted)
        {
            yield return new WaitForSeconds(0.5f);

            if (signatureDrawer != null && signatureDrawer.IsSigned())
            {
                signatureCompleted = true;

                // вызываем событие для всех подписчиков
                OnSignatureCompleted?.Invoke();

                List<string> remainingLines = new List<string>();
                if (dialogueLines.Count > 5) remainingLines.Add(dialogueLines[5]);
                if (dialogueLines.Count > 6) remainingLines.Add(dialogueLines[6]);

                dialogueManager.StartDialogue(remainingLines, () => OnRemainingLinesFinishedWrapper(remainingLines));
            }

        }
    }

    private IEnumerator BlackScreenSequence()
    {
        if (blackScreenController != null)
            blackScreenController.StartCoroutine(blackScreenController.ShowBlackScreen());

        yield return new WaitForSeconds(2f);
        gameObject.SetActive(false);
    }

    void Update()
    {
        if (waitingForBlackScreen && Input.GetMouseButtonDown(0))
        {
            waitingForBlackScreen = false;
            StartCoroutine(BlackScreenSequence());
        }
    }
}
