using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ManagerDialogue7 : MonoBehaviour
{
    [Header("UI References")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;

    [Header("Typing Settings")]
    public float typingSpeed = 0.05f;

    [Header("Todo Settings")]
    public TodoUI7 todoManager;
    public int showTodoAfterLine = 5; // После какой строки показывать туду (например, после 2-й строки = 2)

    [Header("Dialogue Events")]
    public System.Action<int> OnDialogueIndexReached;

    private Queue<string> lines;
    private Coroutine typingCoroutine;
    private string currentLine;
    private bool isTyping = false;
    private System.Action onDialogueEndCallback;
    private int currentLineIndex = 0;

    // Флаги для отслеживания состояния
    private bool todoTriggerCalled = false; // Был ли уже вызван триггер для туду

    void Awake()
    {
        lines = new Queue<string>();

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
        else
            Debug.LogError("ManagerDialogue7: dialoguePanel не назначен!");
    }

    // -------------------------------
    //        START DIALOGUE
    // -------------------------------
    public void StartDialogue(List<string> dialogueLines, System.Action onEndCallback = null)
    {
        Debug.Log($"ManagerDialogue7: StartDialogue вызван, строк: {dialogueLines?.Count}");

        // Сброс всех состояний при начале нового диалога
        ResetDialogueState();

        if (lines == null)
        {
            lines = new Queue<string>();
        }

        lines.Clear();

        foreach (string line in dialogueLines)
            lines.Enqueue(line);

        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        onDialogueEndCallback = onEndCallback;

        DisplayNextLine();
    }

    private void ResetDialogueState()
    {
        currentLineIndex = 0;
        todoTriggerCalled = false;

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        isTyping = false;
    }

    void Update()
    {
        if (PauseMenu.IsGamePaused) return;

        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            if (isTyping)
            {
                CompleteTyping();
            }
            else
            {
                DisplayNextLine();
            }
        }
    }

    private void CompleteTyping()
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        dialogueText.text = currentLine;
        isTyping = false;

        UpdateTextLayout();
    }

    // -------------------------------
    //      SHOW NEXT LINE
    // -------------------------------
    public void DisplayNextLine()
    {
        if (PauseMenu.IsGamePaused) return;

        if (lines == null)
        {
            Debug.LogError("ManagerDialogue7: lines NULL в DisplayNextLine!");
            return;
        }

        if (lines.Count == 0)
        {
            EndDialogue();
            return;
        }

        currentLine = lines.Dequeue();
        currentLineIndex++;

        OnDialogueIndexReached?.Invoke(currentLineIndex);

        // Вызываем триггер для туду ТОЛЬКО если он еще не был вызван
        if (!todoTriggerCalled && currentLineIndex == showTodoAfterLine)
        {
            TriggerTodoShow();
        }

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(TypeLine(currentLine));
    }

    // -------------------------------
    //   TRIGGER TODO SHOW (ОДНОРАЗОВО)
    // -------------------------------
    private void TriggerTodoShow()
    {
        // Помечаем, что триггер уже сработал
        todoTriggerCalled = true;

        if (todoManager != null)
        {
            Debug.Log($"ManagerDialogue7: Пытаюсь показать Todo после строки {showTodoAfterLine}");

            // Проверяем можно ли показать туду
            if (todoManager.CanShowPanel())
            {
                todoManager.ShowPanel();
                Debug.Log($"ManagerDialogue7: Todo показано успешно");
            }
            else
            {
                Debug.Log($"ManagerDialogue7: Todo нельзя показать (уже скрыто или не настроено)");
            }
        }
        else
        {
            Debug.LogWarning("ManagerDialogue7: todoManager не назначен!");
        }
    }

    // -------------------------------
    //     TYPING EFFECT
    // -------------------------------
    IEnumerator TypeLine(string line)
    {
        dialogueText.text = "";
        dialogueText.richText = true;
        isTyping = true;

        int i = 0;
        bool insideTag = false;
        string currentTag = "";

        while (i < line.Length)
        {
            if (PauseMenu.IsGamePaused)
            {
                yield return new WaitWhile(() => PauseMenu.IsGamePaused);
            }

            char c = line[i];

            if (c == '<')
            {
                insideTag = true;
                currentTag = "<";
            }
            else if (insideTag)
            {
                currentTag += c;
                if (c == '>')
                {
                    dialogueText.text += currentTag;
                    insideTag = false;
                    currentTag = "";
                }
            }
            else
            {
                dialogueText.text += c;
                yield return new WaitForSeconds(typingSpeed);
            }

            i++;
        }

        isTyping = false;
    }

    // -------------------------------
    //       END DIALOGUE
    // -------------------------------
    void EndDialogue()
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        onDialogueEndCallback?.Invoke();
        onDialogueEndCallback = null;

        Debug.Log("ManagerDialogue7: Диалог завершен");
    }

    public void ForceEndDialogue()
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        if (lines != null)
            lines.Clear();

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        onDialogueEndCallback = null;

        Debug.Log("ManagerDialogue7: Диалог принудительно завершен");
    }

    private void UpdateTextLayout()
    {
        dialogueText.ForceMeshUpdate();
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(dialogueText.rectTransform);

        if (dialoguePanel != null)
            LayoutRebuilder.ForceRebuildLayoutImmediate(dialoguePanel.GetComponent<RectTransform>());
    }

    // -------------------------------
    //        PUBLIC METHODS
    // -------------------------------

    /// <summary>
    /// Пропустить диалог
    /// </summary>
    public void SkipDialogue()
    {
        ForceEndDialogue();
    }

    /// <summary>
    /// Проверить, идет ли сейчас диалог
    /// </summary>
    public bool IsDialogueActive()
    {
        return dialoguePanel != null && dialoguePanel.activeSelf;
    }
}