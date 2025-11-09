using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ManagerDialogue2 : MonoBehaviour
{
    [Header("UI References")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;

    [Header("Typing Settings")]
    public float typingSpeed = 0.05f;

    [Header("Todo Settings")]
    public TodoUIManager todoManager; // Ссылка на менеджер туду
    public int showTodoAfterLine = 5; // После какой строки показать туду (5 = после 5-й реплики)

    [Header("Dialogue Events")]
    public System.Action<int> OnDialogueIndexReached; // Событие при достижении индекса

    private Queue<string> lines;
    private Coroutine typingCoroutine;
    private string currentLine;
    private bool isTyping = false;
    private System.Action onDialogueEndCallback;
    private int currentLineIndex = 0; // Счетчик текущей строки

    void Awake()
    {
        lines = new Queue<string>();

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
        else
            Debug.LogError("ManagerDialogue2: dialoguePanel не назначен в инспекторе!");
    }

    /// <summary>
    /// Запуск диалога
    /// </summary>
    public void StartDialogue(List<string> dialogueLines, System.Action onEndCallback = null)
    {
        Debug.Log($"ManagerDialogue2: StartDialogue вызван, строк: {dialogueLines?.Count}");

        if (lines == null)
        {
            Debug.LogError("ManagerDialogue2: lines is NULL! Инициализирую экстренно...");
            lines = new Queue<string>();
        }

        // Сброс счетчика строк
        currentLineIndex = 0;

        lines.Clear();
        foreach (string line in dialogueLines)
            lines.Enqueue(line);

        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);
        else
            Debug.LogError("ManagerDialogue2: dialoguePanel is NULL!");

        onDialogueEndCallback = onEndCallback;

        DisplayNextLine();
    }

    void Update()
    {
        // ВАЖНО: проверяем, не открыта ли пауза
        if (PauseMenu.IsGamePaused) return;

        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            if (isTyping)
            {
                // Прерываем корутину печати
                StopCoroutine(typingCoroutine);
                dialogueText.text = currentLine;
                isTyping = false;

                // === FIX: принудительная перестройка Layout'а ===
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

    /// <summary>
    /// Отображение следующей реплики
    /// </summary>
    public void DisplayNextLine()
    {
        // Дополнительная проверка на паузу для надежности
        if (PauseMenu.IsGamePaused) return;

        if (lines == null)
        {
            Debug.LogError("ManagerDialogue2: lines is NULL in DisplayNextLine!");
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

        // Проверяем, нужно ли показать Todo после текущей строки
        CheckForTodoTrigger();

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(TypeLine(currentLine));
    }

    /// <summary>
    /// Проверка триггера для показа Todo списка
    /// </summary>
    private void CheckForTodoTrigger()
    {
        // Показываем Todo после того, как отобразилась строка с индексом showTodoAfterLine
        // currentLineIndex увеличивается ДО отображения строки, поэтому проверяем на showTodoAfterLine
        if (currentLineIndex == showTodoAfterLine + 1 && todoManager != null)
        {
            Debug.Log($"ManagerDialogue2: Показываем Todo после строки {showTodoAfterLine}");
            todoManager.ShowTodoList();
        }
    }

    /// <summary>
    /// Корутина плавной печати текста с поддержкой Rich Text
    /// </summary>
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
            // Проверка на паузу даже во время печати
            if (PauseMenu.IsGamePaused)
            {
                yield return new WaitWhile(() => PauseMenu.IsGamePaused);
            }

            char c = line[i];

            // Обработка тегов
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
                    // Добавляем завершенный тег сразу
                    dialogueText.text += currentTag;
                    insideTag = false;
                    currentTag = "";
                }
            }
            else
            {
                // Обычный текст - печатаем с задержкой
                dialogueText.text += c;
                yield return new WaitForSeconds(typingSpeed);
            }

            i++;
        }

        isTyping = false;
    }

    /// <summary>
    /// Завершение диалога
    /// </summary>
    void EndDialogue()
    {
        if (dialoguePanel != null)
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

        if (lines != null)
            lines.Clear();

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        onDialogueEndCallback = null;
    }

    private IEnumerator ForceLayoutNextFrame()
    {
        yield return null; // дождаться конца текущего кадра
        dialogueText.ForceMeshUpdate();
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(dialogueText.rectTransform);
        LayoutRebuilder.ForceRebuildLayoutImmediate(dialoguePanel.GetComponent<RectTransform>());
    }
}