using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    [Header("UI References")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;

    [Header("Typing Settings")]
    public float typingSpeed = 0.03f;    // время между символами
    public float autoSkipDelay = 2f;

    [HideInInspector] public bool dialogueActive = false;

    private Queue<string> lines;
    private Coroutine typingCoroutine;
    private string currentLine;
    private bool isTyping = false;

    void Awake()
    {
        if (Instance == null) Instance = this;

        lines = new Queue<string>();
        dialoguePanel.SetActive(false);
    }

    public void StartDialogue(List<string> dialogueLines)
    {
        lines.Clear();
        foreach (string line in dialogueLines)
            lines.Enqueue(line);

        dialoguePanel.SetActive(true);
        dialogueActive = true;
        DisplayNextLine();
    }

    void Update()
    {
        if (!dialogueActive) return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (isTyping)
            {
                StopCoroutine(typingCoroutine);
                dialogueText.text = currentLine;
                isTyping = false;
            }
            else
            {
                DisplayNextLine();
            }
        }
    }

    public void DisplayNextLine()
    {
        if (lines.Count == 0)
        {
            EndDialogue();
            return;
        }

        currentLine = lines.Dequeue();

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(TypeLine(currentLine));
    }

    IEnumerator TypeLine(string line)
    {
        dialogueText.text = "";
        dialogueText.richText = true;
        isTyping = true;

        int i = 0;
        bool insideTag = false;

        while (i < line.Length)
        {
            char c = line[i];

            if (c == '<') insideTag = true;
            dialogueText.text += c;
            if (c == '>') insideTag = false;

            i++;
            if (!insideTag)
                yield return new WaitForSeconds(typingSpeed);
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(dialogueText.rectTransform);
        isTyping = false;
    }

    void EndDialogue()
    {
        dialoguePanel.SetActive(false);
        dialogueActive = false;
    }
}







