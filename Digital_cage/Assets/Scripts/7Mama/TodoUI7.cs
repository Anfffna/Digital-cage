using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.UI;

public class TodoUI7 : MonoBehaviour
{
    [Header("Task Settings")]
    public TextMeshProUGUI task1Text;    // Первый пункт (активен с начала)

    [Header("UI Panel")]
    public CanvasGroup todoPanel;        // Панель Todo

    [Header("Completion Settings")]
    public float hideDelay = 0.3f;       // Задержка перед скрытием после зачеркивания

    private bool task1Hidden = false;
    private bool isPanelShowing = false;

    void Start()
    {
        // Инициализация панели
        if (todoPanel != null)
        {
            todoPanel.alpha = 0f;
            todoPanel.gameObject.SetActive(false);
        }

        // ПЕРВЫЙ ПУНКТ АКТИВЕН С НАЧАЛА
        if (task1Text != null)
        {
            task1Text.gameObject.SetActive(true);
            task1Text.alpha = 1f;
            task1Text.fontStyle = FontStyles.Normal;
        }
        else
        {
            Debug.LogWarning("TodoUI7: task1Text не назначен!");
        }
    }

    /// <summary>
    /// Показать Todo панель (для ManagerDialogue7) - ПОКАЗЫВАЕТ ПЕРВЫЙ ПУНКТ
    /// </summary>
    public void ShowPanel()
    {
        if (isPanelShowing || todoPanel == null || task1Text == null) return;

        isPanelShowing = true;
        todoPanel.gameObject.SetActive(true);

        // Убеждаемся что первый пункт активен и видим
        task1Text.gameObject.SetActive(true);
        task1Text.alpha = 1f;
        task1Text.fontStyle = FontStyles.Normal;

        StartCoroutine(FadeInPanel());
    }

    private IEnumerator FadeInPanel()
    {
        float timer = 0f;
        while (timer < 1f)
        {
            timer += Time.deltaTime;
            todoPanel.alpha = timer;
            yield return null;
        }
        todoPanel.alpha = 1f;

        Debug.Log("TodoUI7: Панель показана с первым пунктом");
    }

    /// <summary>
    /// Скрыть первую задачу (зачеркнуть и скрыть с панелью)
    /// </summary>
    public void HideTask1()
    {
        if (task1Hidden || task1Text == null) return;

        task1Hidden = true;
        StartCoroutine(HideTaskCoroutine());
    }

    private IEnumerator HideTaskCoroutine()
    {
        if (task1Text == null) yield break;

        Debug.Log("TodoUI7: Начинаю скрытие первой задачи...");

        // 1. Зачеркиваем текст
        task1Text.fontStyle |= FontStyles.Strikethrough;
        Debug.Log("TodoUI7: Текст зачеркнут");

        // 2. Меняем цвет на серый
        Color completedColor = new Color(0.5f, 0.5f, 0.5f, 0.7f);
        task1Text.color = completedColor;
        Debug.Log("TodoUI7: Цвет изменен на серый");

        // 3. Ждем немного
        yield return new WaitForSeconds(hideDelay);
        Debug.Log("TodoUI7: Задержка перед исчезновением завершена");

        // 4. Плавно исчезаем текст
        float timer = 0f;
        float fadeDuration = 0.5f;
        Color startColor = task1Text.color;
        Color endColor = startColor;
        endColor.a = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / fadeDuration;
            task1Text.color = Color.Lerp(startColor, endColor, progress);
            yield return null;
        }

        task1Text.color = endColor;
        Debug.Log("TodoUI7: Текст полностью прозрачный");

        // 5. Полностью скрываем текст
        task1Text.gameObject.SetActive(false);
        Debug.Log("TodoUI7: Текст деактивирован");

        // 6. Плавно скрываем ВСЮ ПАНЕЛЬ
        yield return new WaitForSeconds(0.3f); // Короткая пауза

        timer = 0f;
        float panelFadeDuration = 0.5f;
        float panelStartAlpha = todoPanel.alpha;

        while (timer < panelFadeDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / panelFadeDuration;
            todoPanel.alpha = Mathf.Lerp(panelStartAlpha, 0f, progress);
            yield return null;
        }

        todoPanel.alpha = 0f;
        todoPanel.gameObject.SetActive(false);
        isPanelShowing = false;

        Debug.Log("TodoUI7: Первый пункт и панель скрыты");
    }

    /// <summary>
    /// Просто зачеркнуть задачу без скрытия панели
    /// </summary>
    public void CompleteTask1WithoutHiding()
    {
        if (task1Hidden || task1Text == null) return;

        task1Hidden = true;
        StartCoroutine(CompleteTaskCoroutine());
    }

    private IEnumerator CompleteTaskCoroutine()
    {
        if (task1Text == null) yield break;

        // 1. Зачеркиваем текст
        task1Text.fontStyle |= FontStyles.Strikethrough;

        // 2. Меняем цвет на серый
        Color completedColor = new Color(0.5f, 0.5f, 0.5f, 0.7f);
        Color startColor = task1Text.color;

        float timer = 0f;
        float fadeDuration = 0.5f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / fadeDuration;
            task1Text.color = Color.Lerp(startColor, completedColor, progress);
            yield return null;
        }

        task1Text.color = completedColor;

        Debug.Log("TodoUI7: Задача зачеркнута (панель остается)");
    }

    /// <summary>
    /// Полностью скрыть панель Todo
    /// </summary>
    public void HidePanel()
    {
        if (todoPanel == null) return;

        StartCoroutine(HidePanelCoroutine());
    }

    private IEnumerator HidePanelCoroutine()
    {
        float timer = 0f;
        float fadeDuration = 0.5f;
        float startAlpha = todoPanel.alpha;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / fadeDuration;
            todoPanel.alpha = Mathf.Lerp(startAlpha, 0f, progress);
            yield return null;
        }

        todoPanel.alpha = 0f;
        todoPanel.gameObject.SetActive(false);
        isPanelShowing = false;

        Debug.Log("TodoUI7: Панель скрыта");
    }

    /// <summary>
    /// Показать панель снова (после скрытия)
    /// </summary>
    public void ShowPanelAgain()
    {
        if (isPanelShowing || todoPanel == null) return;

        isPanelShowing = true;
        todoPanel.gameObject.SetActive(true);

        if (task1Text != null && !task1Hidden)
        {
            task1Text.gameObject.SetActive(true);
            task1Text.alpha = 1f;
        }

        StartCoroutine(FadeInPanel());
    }

    /// <summary>
    /// Проверка, скрыта ли уже первая задача
    /// </summary>
    public bool IsTask1Hidden()
    {
        return task1Hidden;
    }

    /// <summary>
    /// Проверка, показана ли панель
    /// </summary>
    public bool IsPanelShowing()
    {
        return isPanelShowing;
    }

    /// <summary>
    /// Получить текст задачи (для отладки)
    /// </summary>
    public string GetTaskText()
    {
        if (task1Text != null)
            return task1Text.text;
        return "Нет текста";
    }

    /// <summary>
    /// Принудительно сбросить состояние (для тестов)
    /// </summary>
    [ContextMenu("Тест: Сбросить состояние")]
    public void ResetState()
    {
        task1Hidden = false;
        isPanelShowing = false;

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

        Debug.Log("TodoUI7: Состояние сброшено");
    }

    /// <summary>
    /// Тест: Показать панель
    /// </summary>
    [ContextMenu("Тест: Показать панель")]
    public void TestShowPanel()
    {
        ShowPanel();
    }

    /// <summary>
    /// Тест: Зачеркнуть и скрыть задачу
    /// </summary>
    [ContextMenu("Тест: Зачеркнуть и скрыть")]
    public void TestHideTask()
    {
        HideTask1();
    }

    /// <summary>
    /// Тест: Просто зачеркнуть
    /// </summary>
    [ContextMenu("Тест: Только зачеркнуть")]
    public void TestCompleteTask()
    {
        CompleteTask1WithoutHiding();
    }
}