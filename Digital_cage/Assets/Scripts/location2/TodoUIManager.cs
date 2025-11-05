using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class TodoUIManager : MonoBehaviour
{
    [Header("Todo UI Settings")]
    public CanvasGroup todoPanel;
    public float fadeInDuration = 1.5f;

    [Header("Todo Items")]
    public TextMeshProUGUI[] todoItems;

    [Header("Task Completion Check")]
    public Stereo stereo;
    public PhotoHall1 photoHall1;
    public PhotoHall2 photoHall2;
    public PhotoHall3 photoHall3;

    private bool isShowing = false;
    private Coroutine fadeCoroutine;
    private int completedTasks = 0;
    private int totalTasks = 0;
    private bool stereoCompleted = false;
    private bool photoHall1Completed = false;
    private bool photoHall2Completed = false;
    private bool photoHall3Completed = false;

    void Start()
    {
        if (todoPanel != null)
        {
            todoPanel.alpha = 0f;
            todoPanel.gameObject.SetActive(false);
        }

        if (todoItems != null)
        {
            totalTasks = todoItems.Length;
        }

        Debug.Log($"TodoUIManager: Всего задач - {totalTasks}");

        AutoFindObjects();
    }

    void Update()
    {
        CheckAllSpecialTasksCompleted();
    }

    private void AutoFindObjects()
    {
        if (stereo == null)
            stereo = FindObjectOfType<Stereo>();

        if (photoHall1 == null)
            photoHall1 = FindObjectOfType<PhotoHall1>();

        if (photoHall2 == null)
            photoHall2 = FindObjectOfType<PhotoHall2>();

        if (photoHall3 == null)
            photoHall3 = FindObjectOfType<PhotoHall3>();
    }

    private void CheckAllSpecialTasksCompleted()
    {
        if (!stereoCompleted && stereo != null && stereo.hasBeenUsed)
        {
            stereoCompleted = true;
            Debug.Log("? Stereo выполнен!");
        }

        if (!photoHall1Completed && photoHall1 != null && photoHall1.hasBeenUsed)
        {
            photoHall1Completed = true;
            Debug.Log("? PhotoHall1 выполнен!");
        }

        if (!photoHall2Completed && photoHall2 != null && photoHall2.hasBeenUsed)
        {
            photoHall2Completed = true;
            Debug.Log("? PhotoHall2 выполнен!");
        }

        if (!photoHall3Completed && photoHall3 != null && photoHall3.hasBeenUsed)
        {
            photoHall3Completed = true;
            Debug.Log("? PhotoHall3 выполнен!");
        }

        if (stereoCompleted && photoHall1Completed && photoHall2Completed && photoHall3Completed)
        {
            if (!IsTodoItemCompleted(1))
            {
                CompleteTodoItem(1);
                Debug.Log("?? Все 4 специальные задачи выполнены! Зачеркиваем индекс 1");
            }
        }
    }

    private bool IsTodoItemCompleted(int index)
    {
        if (todoItems == null || index < 0 || index >= todoItems.Length) return false;
        return todoItems[index].text.StartsWith("<s>");
    }

    public void ShowTodoList()
    {
        if (isShowing || todoPanel == null) return;

        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(FadeInTodo());
    }

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
    }

    public void CompleteTodoItem(int index)
    {
        if (todoItems != null && index >= 0 && index < todoItems.Length && todoItems[index] != null)
        {
            string currentText = todoItems[index].text;

            if (!currentText.StartsWith("<s>"))
            {
                StartCoroutine(SimpleBlinkEffect(index, currentText));
            }
        }
    }

    private IEnumerator SimpleBlinkEffect(int index, string originalText)
    {
        if (todoPanel == null) yield break;

        // Запоминаем исходную прозрачность панели (должна быть 1)
        float originalAlpha = todoPanel.alpha;

        // Первое моргание - исчезаем до 20%
        float timer = 0f;
        while (timer < 0.1f)
        {
            timer += Time.deltaTime;
            todoPanel.alpha = Mathf.Lerp(originalAlpha, 0.2f, timer / 0.1f);
            yield return null;
        }

        // Зачеркиваем текст в середине эффекта
        todoItems[index].text = $"<s>{originalText}</s>";
        todoItems[index].color = new Color(0.5f, 0.5f, 0.5f, 0.7f);

        // ДОБАВЛЕНО: Правильно считаем выполненные задачи
        completedTasks = CountCompletedTasks();
        Debug.Log($"TodoUIManager: Задача {index} выполнена. Всего выполнено: {completedTasks}/{totalTasks}");

        // Второе моргание - возвращаемся к 100%
        timer = 0f;
        while (timer < 0.1f)
        {
            timer += Time.deltaTime;
            todoPanel.alpha = Mathf.Lerp(0.2f, 1f, timer / 0.1f);
            yield return null;
        }

        // Гарантируем что панель вернулась к полной прозрачности
        todoPanel.alpha = 1f;

        // ДОБАВЛЕНО: Проверяем все ли задачи выполнены и скрываем панель
        HideAfterAllTasksCompleted();
    }

    /// <summary>
    /// ДОБАВЛЕНО: Правильно подсчитывает количество выполненных задач
    /// </summary>
    private int CountCompletedTasks()
    {
        int count = 0;
        if (todoItems != null)
        {
            foreach (TextMeshProUGUI item in todoItems)
            {
                if (item != null && item.text.StartsWith("<s>"))
                {
                    count++;
                }
            }
        }
        return count;
    }

    /// <summary>
    /// Скрывает панель после задержки когда все задачи выполнены
    /// </summary>
    public void HideAfterAllTasksCompleted()
    {
        if (completedTasks >= totalTasks)
        {
            Debug.Log($"TodoUIManager: Все задачи выполнены ({completedTasks}/{totalTasks})! Скрываем панель...");
            StartCoroutine(HideAfterDelay(1f));
        }
    }

    private IEnumerator HideAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        HideTodoList();
    }

    // УДАЛИ или ЗАКОММЕНТИРУЙ эти методы чтобы панель не скрывалась:
    /*
    private void CheckAllTasksCompleted()
    {
        if (completedTasks >= totalTasks)
        {
            Debug.Log("TodoUIManager: Все задачи выполнены! Скрываем панель через 2 секунды...");
            StartCoroutine(HideAfterDelay(2f));
        }
    }

    private IEnumerator HideAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        HideTodoList();
    }
    */
}