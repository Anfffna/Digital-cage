using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class TodoUIManager : MonoBehaviour
{
    [Header("Todo UI Settings")]
    public CanvasGroup todoPanel; // Перетащите вашу Todo панель сюда
    public float fadeInDuration = 1.5f;

    [Header("Todo Items")]
    public TextMeshProUGUI[] todoItems; // Перетащите текстовые элементы ваших 3 дел

    private bool isShowing = false;
    private Coroutine fadeCoroutine;
    private int completedTasks = 0;
    private int totalTasks = 0;

    void Start()
    {
        // Гарантируем, что туду панель не видна при старте
        if (todoPanel != null)
        {
            todoPanel.alpha = 0f;
            todoPanel.gameObject.SetActive(false);
        }

        // Определяем общее количество задач
        if (todoItems != null)
        {
            totalTasks = todoItems.Length;
        }

        Debug.Log($"TodoUIManager: Всего задач - {totalTasks}");
    }

    /// <summary>
    /// Плавно показывает Todo панель
    /// </summary>
    public void ShowTodoList()
    {
        if (isShowing || todoPanel == null) return;

        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(FadeInTodo());
    }

    /// <summary>
    /// Плавно скрывает Todo панель
    /// </summary>
    public void HideTodoList()
    {
        if (!isShowing || todoPanel == null) return;

        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(FadeOutTodo());
    }

    private IEnumerator FadeInTodo()
    {
        isShowing = true;
        todoPanel.gameObject.SetActive(true);

        float timer = 0f;
        while (timer < fadeInDuration)
        {
            timer += Time.deltaTime;
            todoPanel.alpha = Mathf.Lerp(0f, 1f, timer / fadeInDuration);
            yield return null;
        }

        todoPanel.alpha = 1f;
        Debug.Log("TodoUIManager: Todo list полностью показан");
    }

    private IEnumerator FadeOutTodo()
    {
        float timer = 0f;
        float startAlpha = todoPanel.alpha;

        while (timer < fadeInDuration)
        {
            timer += Time.deltaTime;
            todoPanel.alpha = Mathf.Lerp(startAlpha, 0f, timer / fadeInDuration);
            yield return null;
        }

        todoPanel.alpha = 0f;
        todoPanel.gameObject.SetActive(false);
        isShowing = false;

        Debug.Log("TodoUIManager: Todo list скрыт");
    }

    /// <summary>
    /// Обновить текст конкретного todo item
    /// </summary>
    public void UpdateTodoItem(int index, string newText)
    {
        if (todoItems != null && index >= 0 && index < todoItems.Length && todoItems[index] != null)
        {
            todoItems[index].text = newText;
        }
    }

    /// <summary>
    /// Отметить todo item как выполненный (зачеркнуть текст)
    /// </summary>
    public void CompleteTodoItem(int index)
    {
        if (todoItems != null && index >= 0 && index < todoItems.Length && todoItems[index] != null)
        {
            string currentText = todoItems[index].text;

            // Проверяем, не выполнена ли уже эта задача
            if (!currentText.StartsWith("<s>"))
            {
                // Запускаем эффект моргания
                StartCoroutine(SimpleBlinkEffect(index, currentText));
            }
        }
    }

    /// <summary>
    /// Простой эффект моргания
    /// </summary>
    private IEnumerator SimpleBlinkEffect(int index, string originalText)
    {
        if (todoPanel == null) yield break;

        float originalAlpha = todoPanel.alpha;

        // Быстрое моргание: исчезновение-появление
        float timer = 0f;
        while (timer < 0.15f)
        {
            timer += Time.deltaTime;
            todoPanel.alpha = Mathf.Lerp(originalAlpha, 0.2f, timer / 0.15f);
            yield return null;
        }

        // Зачеркиваем задачу в середине эффекта
        todoItems[index].text = $"<s>{originalText}</s>";
        todoItems[index].color = new Color(0.5f, 0.5f, 0.5f, 0.7f);
        completedTasks++;

        // Возвращаем прозрачность
        timer = 0f;
        while (timer < 0.15f)
        {
            timer += Time.deltaTime;
            todoPanel.alpha = Mathf.Lerp(0.2f, originalAlpha, timer / 0.15f);
            yield return null;
        }

        todoPanel.alpha = originalAlpha;

        Debug.Log($"TodoUIManager: Задача {index} выполнена. Выполнено: {completedTasks}/{totalTasks}");
        CheckAllTasksCompleted();
    }

    /// <summary>
    /// Проверяет, все ли задачи выполнены
    /// </summary>
    private void CheckAllTasksCompleted()
    {
        if (completedTasks >= totalTasks)
        {
            Debug.Log("TodoUIManager: Все задачи выполнены! Скрываем панель через 2 секунды...");
            StartCoroutine(HideAfterDelay(2f));
        }
    }

    /// <summary>
    /// Скрывает панель после задержки (когда все задачи выполнены)
    /// </summary>
    private IEnumerator HideAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        HideTodoList();
    }

    /// <summary>
    /// Принудительно завершить все задачи и скрыть панель
    /// </summary>
    public void ForceCompleteAllTasks()
    {
        for (int i = 0; i < totalTasks; i++)
        {
            CompleteTodoItem(i);
        }
    }

    /// <summary>
    /// Проверить, все ли задачи выполнены
    /// </summary>
    public bool AreAllTasksCompleted()
    {
        return completedTasks >= totalTasks;
    }

    /// <summary>
    /// Получить прогресс выполнения задач
    /// </summary>
    public int GetCompletedTasksCount()
    {
        return completedTasks;
    }

    /// <summary>
    /// Получить общее количество задач
    /// </summary>
    public int GetTotalTasksCount()
    {
        return totalTasks;
    }
}