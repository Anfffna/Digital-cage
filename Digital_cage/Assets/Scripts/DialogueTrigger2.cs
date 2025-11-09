using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DialogueTrigger2 : MonoBehaviour
{
    [Header("Dialogue Settings")]
    public DialogueManager dialogueManager;

    [TextArea(2, 5)]
    public List<string> dialogueLines;

    [Header("Hand / Phone Controller")]
    public HandPhoneController handPhoneController;

    [Header("ToDo UI")]
    public ToDoUI toDoUI;

    private bool secondPartStarted = false;
    private bool postToDoStarted = false;

    public void StartSecondPartDialogue()
    {
        if (secondPartStarted) return;
        secondPartStarted = true;

        Debug.Log($"=== StartSecondPartDialogue ВЫЗВАН ===");

        // Вторая часть диалога: реплики с 9 по 13 включительно
        List<string> secondPartLines = dialogueLines.GetRange(0, 5); // 5 реплик: индексы 9,10,11,12,13
        StartCoroutine(StartNextDialogueAfterDelay(secondPartLines, 0.5f));

        // Показываем ToDo панель через 7 секунд
        ShowToDoListWithDelay(7f);
    }

    private IEnumerator StartNextDialogueAfterDelay(List<string> nextLines, float delay)
    {
        yield return new WaitForSeconds(delay);
        dialogueManager.StartDialogue(nextLines, OnDialogueLineFinished, false, true);
    }

    private void OnDialogueLineFinished(int lineIndex)
    {
        if (handPhoneController != null)
            handPhoneController.OnDialogueLineFinished(lineIndex);

        ChairSit chairSit = FindObjectOfType<ChairSit>();
        if (chairSit != null)
            chairSit.OnDialogueLineFinished(lineIndex);
    }

    private void ShowToDoListWithDelay(float delay)
    {
        if (toDoUI != null)
            StartCoroutine(PostToDoDialogue(delay));
        else
            Debug.LogWarning("DialogueTrigger2: ToDoUI не привязан!");
    }

    private IEnumerator PostToDoDialogue(float delay)
    {
        yield return new WaitForSeconds(delay);

        // Показываем ToDo UI
        toDoUI.ShowPanel();

        // Ждём пока панель скроется
        yield return StartCoroutine(WaitForToDoHide());

        // Ждём 1.5 секунды перед запуском финальной реплики
        yield return new WaitForSeconds(1.5f);

        // После скрытия панели запускаем только финальную реплику (индекс 5 - это 14-я реплика в общем списке)
        if (!postToDoStarted && dialogueLines.Count > 5)
        {
            postToDoStarted = true;
            List<string> postLines = new List<string> { dialogueLines[5] };
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

    private IEnumerator WaitForToDoHide()
    {
        while (toDoUI.panel != null && toDoUI.panel.gameObject.activeSelf)
            yield return null;
    }
}