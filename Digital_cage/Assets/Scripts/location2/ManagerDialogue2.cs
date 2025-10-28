using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ManagerDialogue2 : MonoBehaviour
{
    public static ManagerDialogue2 Instance;

    [Header("UI References")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;

    [Header("Typing Settings")]
    public float typingSpeed = 0.05f;

    private Queue<string> lines;
    private Coroutine typingCoroutine;
    private string currentLine;
    private bool isTyping = false;
    private System.Action onDialogueEndCallback;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        lines = new Queue<string>();
        dialoguePanel.SetActive(false);
    }

    /// <summary>
    /// Запуск диалога
    /// </summary>
    public void StartDialogue(List<string> dialogueLines, System.Action onEndCallback = null)
    {
        lines.Clear();
        foreach (string line in dialogueLines)
            lines.Enqueue(line);

        dialoguePanel.SetActive(true);
        onDialogueEndCallback = onEndCallback;

        DisplayNextLine();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            if (isTyping)
            {
                // Пропуск анимации печати
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

    /// <summary>
    /// Отображение следующей реплики
    /// </summary>
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

    /// <summary>
    /// Корутина плавной печати текста
    /// </summary>
    IEnumerator TypeLine(string line)
    {
        dialogueText.text = "";
        isTyping = true;

        foreach (char letter in line.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
    }

    /// <summary>
    /// Завершение диалога
    /// </summary>
    void EndDialogue()
    {
        dialoguePanel.SetActive(false);
        onDialogueEndCallback?.Invoke();
        onDialogueEndCallback = null;
    }

    /// <summary>
    /// Принудительно завершить диалог
    /// </summary>
    public void ForceEndDialogue()
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        lines.Clear();
        dialoguePanel.SetActive(false);
        onDialogueEndCallback = null;
    }
}