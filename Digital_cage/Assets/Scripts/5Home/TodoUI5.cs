using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class TodoUI5 : MonoBehaviour
{
    [Header("Todo UI Settings")]
    public CanvasGroup todoPanel;
    public float fadeInDuration = 1.5f;
    public float fadeOutDuration = 1f;

    [Header("Todo Items")]
    public TextMeshProUGUI lightTaskText;
    public TextMeshProUGUI newTaskText; // Второй пункт (опциональный)

    private bool isShowing = false;
    private bool isLightTaskCompleted = false;
    private Coroutine fadeCoroutine;

    void Start()
    {
        if (todoPanel != null)
        {
            todoPanel.alpha = 0f;
            todoPanel.gameObject.SetActive(false);
        }

        // Скрываем второй пункт если он назначен, но не обязателен
        if (newTaskText != null)
        {
            newTaskText.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Показать Todo панель
    /// </summary>
    public void ShowPanel()
    {
        if (isShowing || todoPanel == null) return;

        Debug.Log("TodoUI5: ShowPanel() вызван");

        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(FadeInTodo());
    }

    /// <summary>
    /// Скрыть Todo панель
    /// </summary>
    public void HidePanel()
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

        while (timer < fadeOutDuration)
        {
            timer += Time.deltaTime;
            todoPanel.alpha = Mathf.Lerp(startAlpha, 0f, timer / fadeOutDuration);
            yield return null;
        }

        todoPanel.alpha = 0f;
        todoPanel.gameObject.SetActive(false);
        isShowing = false;
    }

    /// <summary>
    /// Завершить первую задачу (включить свет)
    /// </summary>
    public void CompleteLightTask()
    {
        if (isLightTaskCompleted) return;

        Debug.Log("TodoUI5: CompleteLightTask() вызван");
        isLightTaskCompleted = true;

        if (newTaskText != null)
        {
            // Если есть второй пункт - меняем на него
            StartCoroutine(ReplaceWithSecondTask());
        }
        else
        {
            // Если второго пункта нет - скрываем UI плашку
            HidePanel();
        }
    }

    private IEnumerator ReplaceWithSecondTask()
    {
        if (lightTaskText == null || newTaskText == null) yield break;

        Debug.Log("TodoUI5: Заменяем на второй пункт");

        // Исчезает старый текст
        float fadeTimer = 0f;
        Color startColor = lightTaskText.color;

        while (fadeTimer < 0.5f)
        {
            fadeTimer += Time.deltaTime;
            float progress = fadeTimer / 0.5f;
            lightTaskText.color = new Color(startColor.r, startColor.g, startColor.b, Mathf.Lerp(1f, 0f, progress));
            yield return null;
        }

        lightTaskText.gameObject.SetActive(false);

        // Показываем новый текст
        newTaskText.gameObject.SetActive(true);
        newTaskText.color = new Color(newTaskText.color.r, newTaskText.color.g, newTaskText.color.b, 0f);

        Debug.Log("TodoUI5: Второй пункт активирован");

        // Появляется новый текст
        fadeTimer = 0f;
        while (fadeTimer < 0.5f)
        {
            fadeTimer += Time.deltaTime;
            float progress = fadeTimer / 0.5f;
            newTaskText.color = new Color(newTaskText.color.r, newTaskText.color.g, newTaskText.color.b, Mathf.Lerp(0f, 1f, progress));
            yield return null;
        }
    }

    /// <summary>
    /// Проверка, активен ли второй пункт
    /// </summary>
    public bool IsSecondTaskActive()
    {
        return newTaskText != null && newTaskText.gameObject.activeInHierarchy;
    }

    /// <summary>
    /// Проверка, показана ли Todo панель
    /// </summary>
    public bool IsPanelShowing()
    {
        return isShowing;
    }

    void OnDestroy()
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
    }
}