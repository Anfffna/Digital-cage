using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.UI;

public class TodoUI6 : MonoBehaviour
{
    [Header("Task 1 Settings")]
    public TextMeshProUGUI task1Text;    // Первый пункт который нужно скрыть

    [Header("Task 2 Settings")]
    public TextMeshProUGUI task2Text;    // Второй пункт - появится после звонка
    public float task2ShowDelay = 0.5f;  // Задержка перед показом второго пункта

    [Header("UI Panel")]
    public CanvasGroup todoPanel;        // Панель Todo

    [Header("Completion Settings")]
    public float hideDelay = 0.3f;       // Задержка перед скрытием

    private bool task1Hidden = false;
    private bool task2Shown = false;
    private bool isPanelShowing = false;

    void Start()
    {
        // Инициализация панели
        if (todoPanel != null)
        {
            todoPanel.alpha = 0f;
            todoPanel.gameObject.SetActive(false);
        }

        // Скрываем оба пункта изначально
        if (task1Text != null)
        {
            task1Text.gameObject.SetActive(true);
        }

        if (task2Text != null)
        {
            task2Text.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Показать Todo панель (для ManagerDialogue6) - ПОКАЗЫВАЕТ ТОЛЬКО ПЕРВЫЙ ПУНКТ
    /// </summary>
    public void ShowPanel()
    {
        if (isPanelShowing || todoPanel == null || task1Text == null) return;

        isPanelShowing = true;
        todoPanel.gameObject.SetActive(true);

        // Показываем только первый пункт
        task1Text.gameObject.SetActive(true);
        task1Text.alpha = 1f;
        task1Text.color = Color.white;
        task1Text.fontStyle = FontStyles.Normal; // Сбрасываем зачеркивание

        // Скрываем второй пункт
        if (task2Text != null)
        {
            task2Text.gameObject.SetActive(false);
        }

        StartCoroutine(FadeInPanel());
    }

    /// <summary>
    /// Показать только второй пункт (вызывается из SmsMama после handDown)
    /// </summary>
    public void ShowTask2Only()
    {
        if (task2Shown || todoPanel == null || task2Text == null) return;

        StartCoroutine(ShowTask2Coroutine());
    }

    private IEnumerator ShowTask2Coroutine()
    {
        if (task2Shown) yield break;

        task2Shown = true;

        // Ждем задержку
        yield return new WaitForSeconds(task2ShowDelay);

        // Активируем панель
        isPanelShowing = true;
        todoPanel.gameObject.SetActive(true);

        // Показываем ТОЛЬКО второй пункт (первый уже должен быть скрыт)
        task2Text.gameObject.SetActive(true);
        task2Text.alpha = 0f;

        // Плавное появление панели и второго пункта одновременно
        float timer = 0f;
        float fadeDuration = 0.7f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / fadeDuration;

            // Фейдим панель
            todoPanel.alpha = progress;

            // Фейдим второй пункт
            task2Text.alpha = progress;

            yield return null;
        }

        todoPanel.alpha = 1f;
        task2Text.alpha = 1f;
        Debug.Log("TodoUI6: Второй пункт показан");
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
    }

    /// <summary>
    /// Скрыть первую задачу (вызывается из SitWork когда игрок садится) - ТЕПЕРЬ СКРЫВАЕТ И ПАНЕЛЬ
    /// </summary>
    public void HideTask1()
    {
        if (task1Hidden || task1Text == null) return;

        task1Hidden = true;
        StartCoroutine(HideTask1Coroutine());
    }

    private IEnumerator HideTask1Coroutine()
    {
        if (task1Text == null) yield break;

        // 1. Зачеркиваем текст
        task1Text.fontStyle |= FontStyles.Strikethrough;

        // 2. Меняем цвет на серый
        Color completedColor = new Color(0.5f, 0.5f, 0.5f, 0.7f);
        task1Text.color = completedColor;

        // 3. Ждем немного
        yield return new WaitForSeconds(hideDelay);

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

        // 5. Полностью скрываем текст
        task1Text.gameObject.SetActive(false);

        // 6. Плавно скрываем ВСЮ ПАНЕЛЬ (вместе с первым пунктом)
        yield return new WaitForSeconds(0.5f); // Маленькая пауза

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

        Debug.Log("TodoUI6: Первый пункт и панель скрыты");
    }

    private IEnumerator FadeOutPanel()
    {
        float timer = 0f;
        float startAlpha = todoPanel.alpha;

        while (timer < 1f)
        {
            timer += Time.deltaTime;
            todoPanel.alpha = Mathf.Lerp(startAlpha, 0f, timer);
            yield return null;
        }

        todoPanel.alpha = 0f;
        todoPanel.gameObject.SetActive(false);
        isPanelShowing = false;
    }

    /// <summary>
    /// Зачеркнуть второй пункт (если понадобится в будущем)
    /// </summary>
    public void CompleteTask2()
    {
        if (task2Text == null) return;

        StartCoroutine(CompleteTask2Coroutine());
    }

    private IEnumerator CompleteTask2Coroutine()
    {
        // 1. Зачеркиваем текст
        task2Text.fontStyle |= FontStyles.Strikethrough;

        // 2. Меняем цвет на серый
        Color completedColor = new Color(0.5f, 0.5f, 0.5f, 0.7f);
        Color startColor = task2Text.color;

        float timer = 0f;
        float fadeDuration = 0.5f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / fadeDuration;
            task2Text.color = Color.Lerp(startColor, completedColor, progress);
            yield return null;
        }

        task2Text.color = completedColor;
    }

    /// <summary>
    /// Проверка, скрыта ли уже первая задача
    /// </summary>
    public bool IsTask1Hidden()
    {
        return task1Hidden;
    }

    /// <summary>
    /// Проверка, показан ли второй пункт
    /// </summary>
    public bool IsTask2Shown()
    {
        return task2Shown;
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

        // Скрываем второй пункт
        if (task2Text != null)
        {
            task2Text.gameObject.SetActive(false);
        }
    }
}