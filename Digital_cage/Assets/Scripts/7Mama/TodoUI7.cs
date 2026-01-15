using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.UI;

public class TodoUI7 : MonoBehaviour
{
    [Header("Task Settings")]
    public TextMeshProUGUI task1Text;    // Первый пункт

    [Header("UI Panel")]
    public CanvasGroup todoPanel;        // Панель Todo

    [Header("Completion Settings")]
    public float hideDelay = 0.3f;       // Задержка перед скрытием после зачеркивания

    [Header("Debug")]
    public bool debugLogs = true;

    // Состояния
    private bool task1Completed = false;   // Задача завершена (зачеркнута)
    private bool task1Hidden = false;      // Задача скрыта с панели
    private bool isPanelVisible = false;   // Панель видима сейчас
    private bool panelEverShown = false;   // Панель показывалась хоть раз

    void Start()
    {
        InitializeUI();
    }

    void InitializeUI()
    {
        // Всегда скрываем панель при старте
        if (todoPanel != null)
        {
            todoPanel.alpha = 0f;
            todoPanel.gameObject.SetActive(false);
            isPanelVisible = false;
        }
        else
        {
            Debug.LogError("TodoUI7: todoPanel не назначен в инспекторе!");
        }

        // Настраиваем текст задачи
        if (task1Text != null)
        {
            // Сбрасываем состояние текста
            task1Text.gameObject.SetActive(true);
            task1Text.alpha = 1f;
            task1Text.fontStyle = FontStyles.Normal;

            if (debugLogs) Log("TodoUI7: Текст задачи инициализирован");
        }
        else
        {
            Debug.LogWarning("TodoUI7: task1Text не назначен!");
        }
    }

    // -------------------------------
    //        PANEL MANAGEMENT
    // -------------------------------

    /// <summary>
    /// Показать панель туду (ОДНОРАЗОВО)
    /// </summary>
    public void ShowPanel()
    {
        // ПРОВЕРКИ ПЕРЕД ПОКАЗОМ:
        // 1. Панель уже видима
        // 2. Задача уже скрыта (нельзя показывать скрытое)
        // 3. Панель уже показывалась ранее
        // 4. Нет необходимых компонентов

        if (isPanelVisible)
        {
            if (debugLogs) Log("TodoUI7: Панель уже видима, не показываем снова");
            return;
        }

        if (task1Hidden)
        {
            if (debugLogs) Log("TodoUI7: Задача уже скрыта, нельзя показывать панель");
            return;
        }

        if (panelEverShown)
        {
            if (debugLogs) Log("TodoUI7: Панель уже показывалась ранее, не показываем снова");
            return;
        }

        if (todoPanel == null || task1Text == null)
        {
            Debug.LogError("TodoUI7: Не могу показать панель - не назначены компоненты");
            return;
        }

        // Устанавливаем флаги
        panelEverShown = true;
        isPanelVisible = true;

        // Активируем панель
        todoPanel.gameObject.SetActive(true);

        // Убеждаемся, что текст видим и в нормальном состоянии
        task1Text.gameObject.SetActive(true);
        task1Text.alpha = 1f;
        task1Text.fontStyle = FontStyles.Normal;

        StartCoroutine(FadeInPanel());

        if (debugLogs) Log("TodoUI7: Панель показана (впервые)");
    }

    /// <summary>
    /// Можно ли показать панель?
    /// </summary>
    public bool CanShowPanel()
    {
        return !isPanelVisible && !task1Hidden && !panelEverShown && todoPanel != null && task1Text != null;
    }

    private IEnumerator FadeInPanel()
    {
        float timer = 0f;
        float duration = 0.5f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            todoPanel.alpha = Mathf.Lerp(0f, 1f, timer / duration);
            yield return null;
        }

        todoPanel.alpha = 1f;
    }

    /// <summary>
    /// Скрыть панель
    /// </summary>
    public void HidePanel()
    {
        if (!isPanelVisible || todoPanel == null) return;

        StartCoroutine(HidePanelCoroutine());
    }

    private IEnumerator HidePanelCoroutine()
    {
        float timer = 0f;
        float duration = 0.5f;
        float startAlpha = todoPanel.alpha;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            todoPanel.alpha = Mathf.Lerp(startAlpha, 0f, timer / duration);
            yield return null;
        }

        todoPanel.alpha = 0f;
        todoPanel.gameObject.SetActive(false);
        isPanelVisible = false;

        if (debugLogs) Log("TodoUI7: Панель скрыта");
    }

    // -------------------------------
    //        TASK MANAGEMENT
    // -------------------------------

    /// <summary>
    /// Завершить и скрыть задачу (с зачеркиванием и скрытием панели)
    /// </summary>
    public void CompleteAndHideTask()
    {
        if (task1Completed || task1Hidden || task1Text == null) return;

        StartCoroutine(CompleteAndHideTaskCoroutine());
    }

    private IEnumerator CompleteAndHideTaskCoroutine()
    {
        task1Completed = true;

        if (debugLogs) Log("TodoUI7: Начинаю завершение задачи...");

        // 1. Зачеркиваем текст
        task1Text.fontStyle = FontStyles.Strikethrough;

        // 2. Плавно меняем цвет на серый
        float timer = 0f;
        float colorDuration = 0.5f;
        Color startColor = task1Text.color;
        Color grayColor = new Color(0.5f, 0.5f, 0.5f, 1f);

        while (timer < colorDuration)
        {
            timer += Time.deltaTime;
            task1Text.color = Color.Lerp(startColor, grayColor, timer / colorDuration);
            yield return null;
        }

        task1Text.color = grayColor;

        // 3. Ждем задержку
        yield return new WaitForSeconds(hideDelay);

        // 4. Плавно скрываем текст
        timer = 0f;
        float fadeDuration = 0.5f;
        startColor = task1Text.color;
        Color transparentColor = startColor;
        transparentColor.a = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            task1Text.color = Color.Lerp(startColor, transparentColor, timer / fadeDuration);
            yield return null;
        }

        task1Text.gameObject.SetActive(false);
        task1Hidden = true;

        if (debugLogs) Log("TodoUI7: Задача скрыта");

        // 5. Скрываем панель через секунду
        yield return new WaitForSeconds(1f);
        HidePanel();
    }

    /// <summary>
    /// Просто зачеркнуть задачу (без скрытия панели)
    /// </summary>
    public void CompleteTaskOnly()
    {
        if (task1Completed || task1Text == null) return;

        StartCoroutine(CompleteTaskOnlyCoroutine());
    }

    private IEnumerator CompleteTaskOnlyCoroutine()
    {
        task1Completed = true;

        // Зачеркиваем
        task1Text.fontStyle = FontStyles.Strikethrough;

        // Меняем цвет на серый
        float timer = 0f;
        float duration = 0.5f;
        Color startColor = task1Text.color;
        Color grayColor = new Color(0.5f, 0.5f, 0.5f, 1f);

        while (timer < duration)
        {
            timer += Time.deltaTime;
            task1Text.color = Color.Lerp(startColor, grayColor, timer / duration);
            yield return null;
        }

        if (debugLogs) Log("TodoUI7: Задача зачеркнута (панель остается)");
    }

    // -------------------------------
    //        STATUS CHECKS
    // -------------------------------

    /// <summary>
    /// Задача уже завершена?
    /// </summary>
    public bool IsTaskCompleted()
    {
        return task1Completed;
    }

    /// <summary>
    /// Задача уже скрыта?
    /// </summary>
    public bool IsTaskHidden()
    {
        return task1Hidden;
    }

    /// <summary>
    /// Панель показывалась ранее?
    /// </summary>
    public bool WasPanelEverShown()
    {
        return panelEverShown;
    }

    /// <summary>
    /// Панель видима сейчас?
    /// </summary>
    public bool IsPanelVisible()
    {
        return isPanelVisible;
    }

    // -------------------------------
    //        DEBUG & TESTS
    // -------------------------------

    private void Log(string message)
    {
        if (debugLogs)
        {
            Debug.Log(message);
        }
    }

    [ContextMenu("Тест: Показать панель")]
    public void TestShowPanel()
    {
        ShowPanel();
    }

    [ContextMenu("Тест: Завершить и скрыть задачу")]
    public void TestCompleteAndHide()
    {
        CompleteAndHideTask();
    }

    [ContextMenu("Тест: Только зачеркнуть")]
    public void TestCompleteOnly()
    {
        CompleteTaskOnly();
    }

    [ContextMenu("Тест: Скрыть панель")]
    public void TestHidePanel()
    {
        HidePanel();
    }

    [ContextMenu("Сбросить состояние")]
    public void ResetState()
    {
        task1Completed = false;
        task1Hidden = false;
        isPanelVisible = false;
        panelEverShown = false;

        if (todoPanel != null)
        {
            todoPanel.alpha = 0f;
            todoPanel.gameObject.SetActive(false);
        }

        if (task1Text != null)
        {
            task1Text.gameObject.SetActive(true);
            task1Text.alpha = 1f;
            task1Text.color = Color.white;
            task1Text.fontStyle = FontStyles.Normal;
        }

        Debug.Log("TodoUI7: Состояние полностью сброшено");
    }

    void OnValidate()
    {
        // В редакторе проверяем настройки
        if (task1Text != null && task1Text.text == "")
        {
            Debug.LogWarning("TodoUI7: Текст задачи пустой! Заполните в инспекторе.");
        }
    }
}