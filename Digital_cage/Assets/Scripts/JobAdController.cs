using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JobAdController : MonoBehaviour
{
    [Header("UI Panel (Oferta)")]
    public GameObject jobAdPanel; // Панель UI оферты

    [Header("Cursor Manager")]
    public CursorUI cursorManager;

    [Header("Dialogue Settings")]
    public DialogueManager0 dialogueManager;

    [Header("Signature Drawer (опционально)")]
    public SignatureDrawerTexture signatureDrawer;

    [Header("Entry Dialogue Settings")]
    public List<string> dialogueLines; // Можно напрямую сюда засунуть текст EntryDialogue

    [Header("Black Screen Controller (опционально)")]
    public BlackScreenController blackScreenController;

    private bool signatureCompleted = false;
    private bool waitingForBlackScreen = false;

    void Start()
    {
        if (jobAdPanel != null)
            jobAdPanel.SetActive(false);
    }

    /// <summary>
    /// Открываем UI оферты и запускаем диалог
    /// </summary>
    public void OpenJobAd()
    {
        if (jobAdPanel != null)
            jobAdPanel.SetActive(true);

        if (cursorManager != null)
            cursorManager.ShowCursor();

        if (dialogueManager != null && dialogueLines != null && dialogueLines.Count > 0)
        {
            // Запускаем диалог до 4 индекса
            List<string> initialLines = new List<string>();
            for (int i = 0; i <= 4 && i < dialogueLines.Count; i++)
                initialLines.Add(dialogueLines[i]);

            dialogueManager.StartDialogue(initialLines, OnInitialDialogueFinished);
        }
    }

    private void OnInitialDialogueFinished()
    {
        Debug.Log("JobAdController: Первые линии диалога завершены.");

        if (signatureDrawer != null)
        {
            signatureDrawer.gameObject.SetActive(true);
            StartCoroutine(CheckForSignature());
        }
    }

    private IEnumerator CheckForSignature()
    {
        while (!signatureCompleted)
        {
            yield return new WaitForSeconds(0.5f);

            if (signatureDrawer.IsSigned())
            {
                signatureCompleted = true;
                Debug.Log("JobAdController: Подпись обнаружена, запускаем оставшиеся строки.");

                // Остальные строки
                List<string> remainingLines = new List<string>();
                if (dialogueLines.Count > 5) remainingLines.Add(dialogueLines[5]);
                if (dialogueLines.Count > 6) remainingLines.Add(dialogueLines[6]);

                dialogueManager.StartDialogue(remainingLines, OnRemainingDialogueFinished);
            }
        }
    }

    private void OnRemainingDialogueFinished()
    {
        Debug.Log("JobAdController: Диалог полностью завершен.");

        if (blackScreenController != null)
        {
            StartCoroutine(BlackScreenSequence());
        }
    }

    private IEnumerator BlackScreenSequence()
    {
        blackScreenController.StartCoroutine(blackScreenController.ShowBlackScreen());
        yield return new WaitForSeconds(2f);
        jobAdPanel.SetActive(false);

        if (cursorManager != null)
            cursorManager.HideCursor();
    }
}
