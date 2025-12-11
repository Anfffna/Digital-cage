using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DialogueManager0 : MonoBehaviour
{
    [Header("UI References")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;

    [Header("Typing Settings")]
    public float typingSpeed = 0.05f;

    [Header("Todo Settings")]
    public ToDoUI todoManager;
    public int showTodoAfterLine = 5;

    public EntryDialogue entryDialogue;

    [Header("Dialogue Events")]
    public System.Action<int> OnDialogueIndexReached;

    private Queue<string> lines;
    private Coroutine typingCoroutine;
    private string currentLine;
    private bool isTyping = false;
    private System.Action onDialogueEndCallback;
    private int currentLineIndex = 0;

    void Awake()
    {
        lines = new Queue<string>();

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
        else
            Debug.LogError("DialogueManager0: dialoguePanel не назначен!");
    }

    // -------------------------------
    //        START DIALOGUE
    // -------------------------------
    public void StartDialogue(List<string> dialogueLines, System.Action onEndCallback = null)
    {
        Debug.Log($"DialogueManager0: StartDialogue вызван, строк: {dialogueLines?.Count}");

        if (lines == null)
        {
            Debug.LogError("DialogueManager0: lines NULL, пересоздаю...");
            lines = new Queue<string>();
        }

        currentLineIndex = 0;
        lines.Clear();

        foreach (string line in dialogueLines)
            lines.Enqueue(line);

        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        if (entryDialogue != null)
        {
            entryDialogue.OnSignatureCompleted += ShowTodoAfterSignature;
        }

        onDialogueEndCallback = onEndCallback;

        DisplayNextLine();
    }

    void Update()
    {
        if (PauseMenu.IsGamePaused) return;

        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            if (isTyping)
            {
                StopCoroutine(typingCoroutine);
                dialogueText.text = currentLine;
                isTyping = false;

                dialogueText.ForceMeshUpdate();
                Canvas.ForceUpdateCanvases();

                LayoutRebuilder.ForceRebuildLayoutImmediate(dialogueText.rectTransform);
                LayoutRebuilder.ForceRebuildLayoutImmediate(dialoguePanel.GetComponent<RectTransform>());

                StartCoroutine(ForceLayoutNextFrame());
            }
            else
            {
                DisplayNextLine();
            }
        }
    }

    // -------------------------------
    //      SHOW NEXT LINE
    // -------------------------------
    public void DisplayNextLine()
    {
        if (PauseMenu.IsGamePaused) return;

        if (lines == null)
        {
            Debug.LogError("DialogueManager0: lines NULL в DisplayNextLine!");
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

        CheckForTodoTrigger();

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(TypeLine(currentLine));
    }

    // -------------------------------
    //   CHECK TODO LIST TRIGGER
    // -------------------------------
    private void CheckForTodoTrigger()
    {
        if (currentLineIndex == showTodoAfterLine + 1 && todoManager != null)
        {
            Debug.Log($"DialogueManager0: ѕоказываем Todo список после строки {showTodoAfterLine}");
            todoManager.ShowPanel(); // <- правильный метод
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

    private void ShowTodoAfterSignature()
    {
        if (todoManager != null)
            todoManager.ShowPanel();
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
    }

    private IEnumerator ForceLayoutNextFrame()
    {
        yield return null;
        dialogueText.ForceMeshUpdate();
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(dialogueText.rectTransform);
        LayoutRebuilder.ForceRebuildLayoutImmediate(dialoguePanel.GetComponent<RectTransform>());
    }
}
