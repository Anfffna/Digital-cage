using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DialogueTrigger : MonoBehaviour
{
    [Header("Dialogue Settings")]
    public DialogueManager dialogueManager;

    [TextArea(2, 5)]
    public List<string> dialogueLines;

    [Header("Вторая часть начинается с этого индекса")]
    public int continueFromLine = 9;

    [Header("Hand / Phone Controller")]
    public HandPhoneController handPhoneController;

    [Header("ToDo UI")]
    public ToDoUI toDoUI;

    private bool triggered = false;
    private bool secondPartAllowed = false;
    private bool secondPartStarted = false;
    private bool postToDoStarted = false;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !triggered)
        {
            triggered = true;

            // Первая часть диалога до continueFromLine
            List<string> firstPart = dialogueLines.GetRange(0, continueFromLine);
            dialogueManager.StartDialogue(firstPart, OnDialogueLineFinished, true);
        }
    }

    private void OnDialogueLineFinished(int lineIndex)
    {
        if (handPhoneController != null)
            handPhoneController.OnDialogueLineFinished(lineIndex);

        ChairSit chairSit = FindObjectOfType<ChairSit>();
        if (chairSit != null)
            chairSit.OnDialogueLineFinished(lineIndex);
    }

    // Вызывается секретаршей после ухода
    public void AllowSecondPartDialogue()
    {
        Debug.Log($"=== AllowSecondPartDialogue ВЫЗВАН ===");
        Debug.Log($"Caller: {new System.Diagnostics.StackTrace().GetFrame(1).GetMethod().Name}");

        if (secondPartAllowed) return;
        secondPartAllowed = true;

        ChairSit chair = FindObjectOfType<ChairSit>();
        if (chair != null)
            chair.DisableInteractionAfterSecretaryLeft();

        if (!secondPartStarted)
        {
            secondPartStarted = true;

            // Вторая часть диалога: реплики от continueFromLine до 13 включительно
            int secondPartCount = 13 - continueFromLine + 1; // чтобы включить индекс 13
            List<string> remainingLines = dialogueLines.GetRange(continueFromLine, secondPartCount);
            StartCoroutine(StartNextDialogueAfterDelay(remainingLines, 0.5f));

            // Показываем ToDo панель через 7 секунд
            ShowToDoListWithDelay(7f);
        }

    }

    private IEnumerator StartNextDialogueAfterDelay(List<string> nextLines, float delay)
    {
        yield return new WaitForSeconds(delay);
        dialogueManager.StartDialogue(nextLines, OnDialogueLineFinished, false, true);
    }

    private IEnumerator PostToDoDialogue(float delay)
    {
        yield return new WaitForSeconds(delay);

        // Показываем ToDo UI
        toDoUI.ShowPanel();

        // Ждём пока панель скроется
        yield return StartCoroutine(WaitForToDoHide());

        // Ждём 1.5 секунды перед запуском 14-й реплики
        yield return new WaitForSeconds(1.5f);

        // После скрытия панели запускаем только реплику 14
        if (!postToDoStarted && dialogueLines.Count > 14)
        {
            postToDoStarted = true;
            List<string> postLines = new List<string> { dialogueLines[14] };
            dialogueManager.StartDialogue(postLines, OnDialogueLineFinished, false, true);

            // Разблокируем кресло
            ChairSit chairSit = FindObjectOfType<ChairSit>();
            if (chairSit != null)
            {
                chairSit.secretaryLeft = false;
                Collider col = chairSit.GetComponent<Collider>();
                if (col != null) col.enabled = true;
                chairSit.isSecondSitAvailable = true;
            }
        }
    }

    private void ShowToDoListWithDelay(float delay)
    {
        if (toDoUI != null)
            StartCoroutine(PostToDoDialogue(delay));
        else
            Debug.LogWarning("DialogueTrigger: ToDoUI не привязан!");
    }

    private IEnumerator WaitForToDoHide()
    {
        while (toDoUI.panel != null && toDoUI.panel.gameObject.activeSelf)
            yield return null;
    }
}
