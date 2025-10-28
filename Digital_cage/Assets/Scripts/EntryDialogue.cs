using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EntryDialogue : MonoBehaviour
{
    [Header("Dialogue Settings")]
    public DialogueManager dialogueManager;
    public SignatureDrawerTexture signatureDrawer;

    [TextArea(2, 5)]
    public List<string> dialogueLines;

    [Header("Auto Start Settings")]
    public bool startOnTrigger = false;
    public float startDelay = 0f;

    [Header("Black Screen Settings")]
    public BlackScreenController blackScreenController; // вместо Image

    private bool triggered = false;
    private bool signatureCompleted = false;
    private bool waitingForBlackScreen = false;

    void OnTriggerEnter(Collider other)
    {
        if (!startOnTrigger) return;

        if (other.CompareTag("Player") && !triggered)
        {
            triggered = true;
            if (startDelay > 0)
                StartCoroutine(StartDialogueWithDelay(startDelay));
            else
                StartDialogue();
        }
    }

    private IEnumerator StartDialogueWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        StartDialogue();
    }

    void Update()
    {
        if (waitingForBlackScreen && Input.GetMouseButtonDown(0))
        {
            waitingForBlackScreen = false;
            Debug.Log("Запускаем черный экран после клика");
            StartCoroutine(BlackScreenSequence());
        }
    }

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
        {
            initialLines.Add(dialogueLines[i]);
        }

        dialogueManager.StartDialogue(initialLines, OnLineFinished, false, false);
    }

    private void OnRemainingLinesFinished(int lineIndex)
    {
        // lineIndex здесь 0 или 1 (относительно нового диалога)
        Debug.Log($"Завершена оставшаяся строка {lineIndex}");

        if (lineIndex == 1) // Это вторая строка в новом диалоге (6 индекс оригинальный)
        {
            Debug.Log("Запускаем черный экран после 6 индекса");
            waitingForBlackScreen = true;
        }
    }

    private void OnLineFinished(int lineIndex)
    {
        Debug.Log($"Завершена строка {lineIndex}: {dialogueLines[lineIndex]}");

        if (lineIndex == 4)
        {
            Debug.Log("Активируем поле для подписи");
            if (signatureDrawer != null)
            {
                signatureDrawer.gameObject.SetActive(true);
                StartCoroutine(CheckForSignature());
            }
        }
        // Убрал отсюда обработку 6 индекса
    }

    private IEnumerator CheckForSignature()
    {
        Debug.Log("Начинаем проверку подписи...");

        while (!signatureCompleted)
        {
            yield return new WaitForSeconds(0.5f);

            if (signatureDrawer.IsSigned())
            {
                signatureCompleted = true;
                Debug.Log("ПОДПИСЬ ОБНАРУЖЕНА! Показываем строки 5 и 6");

                // Запускаем диалог с 5 и 6 индексами
                List<string> remainingLines = new List<string>();
                if (dialogueLines.Count > 5) remainingLines.Add(dialogueLines[5]);
                if (dialogueLines.Count > 6) remainingLines.Add(dialogueLines[6]);

                // Используем специальный колбэк для этих строк
                dialogueManager.StartDialogue(remainingLines, OnRemainingLinesFinished, false, false);
            }
        }
    }

    private IEnumerator BlackScreenSequence()
    {
        Debug.Log("Скрываем объявление и запускаем черный экран");

        // Сначала запускаем черный экран
        blackScreenController.StartCoroutine(blackScreenController.ShowBlackScreen());

        // Ждем 2 секунды перед выключением объявления
        yield return new WaitForSeconds(2f);

        // Теперь скрываем объявление
        gameObject.SetActive(false);
    }
}