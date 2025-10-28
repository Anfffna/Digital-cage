using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    [Header("UI References")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;

    [Header("Typing Settings")]
    public float typingSpeed = 0.05f; // Время между символами
    public float autoNextDelay = 5f;  // Задержка автоматической смены диалога

    [Header("Signature Check")]
    public SignatureDrawerTexture signatureDrawer; // Поле подписи
    public int requireSignatureForLine = 5;       // Индекс линии, ждущей подписи


    [HideInInspector] public bool dialogueActive = false;

    private Queue<string> lines;                   // Очередь реплик
    private Coroutine typingCoroutine;             // Корутина печати текста
    private Coroutine autoNextCoroutine;           // Корутина авто-смены
    private string currentLine;                    // Текущая реплика
    private bool isTyping = false;                 // Флаг печати
    private System.Action<int> onLineFinishedCallback; // Колбэк после каждой реплики
    private int currentLineIndex = 0;              // Индекс текущей реплики
    private bool allowAutoContinueForAll = false;

    // Блокировка мыши после линии 2
    private bool blockMouseAfterLine2 = false;
    private bool allowBlockAfterLine2 = true;

    private bool waitForSecretary = false;

    private ChairSit chairSit;

    void Awake()
    {
        if (Instance == null)
            Instance = this;

        lines = new Queue<string>();
        dialoguePanel.SetActive(false);

        // Подписка на событие посадки игрока в кресло
        chairSit = FindObjectOfType<ChairSit>();
        if (chairSit != null)
        {
            chairSit.OnPlayerSatDown += OnPlayerSatDown;
        }
    }

    /// <summary>
    /// Запуск диалога
    /// </summary>
    /// <param name="dialogueLines">Список реплик</param>
    /// <param name="onLineFinished">Колбэк после каждой реплики</param>
    /// <param name="allowBlockAfterLine2">Разрешить блокировку после линии 2</param>
    public void StartDialogue(List<string> dialogueLines, System.Action<int> onLineFinished = null, bool allowBlockAfterLine2 = true, bool allowAutoContinueForAll = false)
    {
        Debug.Log($"=== StartDialogue вызван ===");
        Debug.Log($"Колбэк: {onLineFinished != null}, AllowAutoContinue: {allowAutoContinueForAll}");

        // Очистка очереди
        lines.Clear();
        foreach (string line in dialogueLines)
            lines.Enqueue(line);

        dialoguePanel.SetActive(true);
        dialogueActive = true;
        currentLineIndex = 0;
        onLineFinishedCallback = onLineFinished;

        // Сбрасываем блокировку при новом диалоге
        blockMouseAfterLine2 = false;
        this.allowBlockAfterLine2 = allowBlockAfterLine2;
        this.allowAutoContinueForAll = allowAutoContinueForAll; // ? НОВАЯ ПЕРЕМЕННАЯ

        DisplayNextLine();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            if (blockMouseAfterLine2)
            {
                Debug.Log("Mouse0 заблокирован до посадки в кресло");
                return;
            }

            if (isTyping)
            {
                // Если печать текста продолжается — мгновенно показываем всю строку
                StopCoroutine(typingCoroutine);
                dialogueText.text = currentLine;
                isTyping = false;

                // Останавливаем авто-смену если была запущена
                if (autoNextCoroutine != null)
                {
                    StopCoroutine(autoNextCoroutine);
                    autoNextCoroutine = null;
                }

                Canvas.ForceUpdateCanvases();
                LayoutRebuilder.ForceRebuildLayoutImmediate(dialoguePanel.GetComponent<RectTransform>());
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
        Debug.Log($"DisplayNextLine: осталось строк {lines.Count}");

        if (waitForSecretary)
        {
            Debug.Log("Диалог заблокирован: ждём завершения пути секретарши...");
            return;
        }

        // Останавливаем предыдущую корутину авто-смены
        if (autoNextCoroutine != null)
        {
            StopCoroutine(autoNextCoroutine);
            autoNextCoroutine = null;
        }

        if (lines.Count == 0)
        {
            EndDialogue();
            return;
        }

        // Берём следующую строку из очереди
        currentLine = lines.Dequeue();

        // Вызываем колбэк с текущим индексом
        onLineFinishedCallback?.Invoke(currentLineIndex);

        // Блокировка мыши после линии 2
        if (allowBlockAfterLine2 && currentLineIndex == 2)
        {
            // Проверяем, сидит ли игрок уже
            if (chairSit != null && chairSit.IsPlayerSitting)
            {
                blockMouseAfterLine2 = false; // не блокируем, игрок уже сидит
                Debug.Log("Игрок уже сидит — Mouse0 не блокируется");
            }
            else
            {
                blockMouseAfterLine2 = true; // блокируем, ждем посадки
                Debug.Log("Mouse0 заблокирован до посадки в кресло");
            }
        }

        currentLineIndex++;

        // Остановка предыдущей корутины печати, если она есть
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(TypeLine(currentLine));
    }

    /// <summary>
    /// Корутина плавной печати реплики
    /// </summary>
    /// <param name="line"></param>
    /// <returns></returns>
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

        // === УМНОЕ АВТО-ПРОДОЛЖЕНИЕ ===
        // Авто-продолжение если: 
        // 1. Нет колбэка ИЛИ 
        // 2. Есть колбэк но очередь не пуста (значит это не последняя реплика)
        //bool shouldAutoContinue = (onLineFinishedCallback == null || allowAutoContinueForAll) && lines.Count > 0 && !blockMouseAfterLine2;

        //if (shouldAutoContinue)
        //{
        //    autoNextCoroutine = StartCoroutine(AutoNextLine());
        //}
    }

    /// <summary>
    /// Автоматическая смена диалога через заданное время
    /// </summary>
    IEnumerator AutoNextLine()
    {
        yield return new WaitForSeconds(autoNextDelay);

        // Проверяем, что диалог все еще активен и не заблокирован
        if (dialogueActive && !blockMouseAfterLine2)
        {
            DisplayNextLine();
        }
    }

    /// <summary>
    /// Завершение диалога
    /// </summary>
    void EndDialogue()
    {
        // Останавливаем все корутины
        if (autoNextCoroutine != null)
        {
            StopCoroutine(autoNextCoroutine);
            autoNextCoroutine = null;
        }

        dialoguePanel.SetActive(false);
        dialogueActive = false;
        onLineFinishedCallback = null;
    }

    /// <summary>
    /// Срабатывает после того, как игрок сел на кресло
    /// </summary>
    private void OnPlayerSatDown()
    {
        if (blockMouseAfterLine2)
        {
            blockMouseAfterLine2 = false;
            Debug.Log("Игрок сел — Mouse0 разблокирован");

            // Останавливаем авто-смену если была запущена
            if (autoNextCoroutine != null)
            {
                StopCoroutine(autoNextCoroutine);
                autoNextCoroutine = null;
            }

            // Продолжаем диалог только если очередь не пуста
            if (lines.Count > 0)
            {
                DisplayNextLine();
            }
        }
    }

    /// <summary>
    /// Заблокировать показ следующей строки, пока секретарь не закончит путь
    /// </summary>
    public void WaitForSecretaryToFinish()
    {
        waitForSecretary = true;
    }

    /// <summary>
    /// Разрешить показ строки (секретарь закончила путь)
    /// </summary>
    public void ContinueAfterSecretary()
    {
        waitForSecretary = false;

        // если очередь не пуста — сразу показываем следующую реплику
        if (dialogueActive && lines.Count > 0)
        {
            DisplayNextLine();
        }
    }

    /// <summary>
    /// Показать одну реплику без использования очереди (для финальных сцен)
    /// </summary>
    public void ShowSingleLine(string line)
    {
        // Останавливаем все корутины
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);
        if (autoNextCoroutine != null)
            StopCoroutine(autoNextCoroutine);

        // Очищаем очередь, чтобы предотвратить повторение
        lines.Clear();

        // Активируем панель если выключена
        if (!dialoguePanel.activeSelf)
            dialoguePanel.SetActive(true);

        dialogueActive = true;
        currentLine = line;

        // Запускаем печать одной строки
        typingCoroutine = StartCoroutine(TypeSingleLine(currentLine));
    }

    /// <summary>
    /// Корутина печати для одиночной строки (без авто-продолжения)
    /// </summary>
    private IEnumerator TypeSingleLine(string line)
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

        // НЕ запускаем авто-продолжение для одиночной строки
    }

    /// <summary>
    /// Скрыть диалог без завершения
    /// </summary>
    public void HideDialogue()
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);
        if (autoNextCoroutine != null)
            StopCoroutine(autoNextCoroutine);

        dialoguePanel.SetActive(false);
        // НЕ сбрасываем dialogueActive = false, чтобы диалог считался активным
    }
}