using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class LightTodoUIManager : MonoBehaviour
{
    [Header("Todo UI Settings")]
    public CanvasGroup todoPanel;
    public float fadeInDuration = 1.5f;
    public float fadeOutDuration = 1f;

    [Header("Todo Items")]
    public TextMeshProUGUI lightTaskText;
    public TextMeshProUGUI newTaskText; // Новый текст для замены

    private bool isShowing = false;
    private bool isCompleted = false;
    private Coroutine fadeCoroutine;

    void Start()
    {
        if (todoPanel != null)
        {
            todoPanel.alpha = 0f;
            todoPanel.gameObject.SetActive(false);
        }

        // Скрываем новый текст изначально
        if (newTaskText != null)
        {
            newTaskText.gameObject.SetActive(false);
            Debug.Log("LightTodoUIManager: newTaskText скрыт в Start()");
        }
    }

    // ДОБАВЬ ЭТОТ МЕТОД ДЛЯ ПРОВЕРКИ
    public bool IsNewTaskActive()
    {
        bool isActive = newTaskText != null && newTaskText.gameObject.activeInHierarchy;
        Debug.Log($"LightTodoUIManager: IsNewTaskActive() = {isActive}");
        return isActive;
    }

    public void ShowTodoList()
    {
        if (isShowing || todoPanel == null || isCompleted) return;

        Debug.Log("LightTodoUIManager: ShowTodoList() вызван");
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

    public void CompleteLightTask()
    {
        if (isCompleted || lightTaskText == null) return;

        Debug.Log("LightTodoUIManager: CompleteLightTask() вызван");
        isCompleted = true;
        StartCoroutine(ReplaceTaskText());
    }

    private IEnumerator ReplaceTaskText()
    {
        if (lightTaskText == null || newTaskText == null) yield break;

        Debug.Log("LightTodoUIManager: Начинаем замену текста");

        // Плавно исчезает старый текст
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

        Debug.Log("LightTodoUIManager: newTaskText активирован!");

        // Плавно появляется новый текст
        fadeTimer = 0f;
        while (fadeTimer < 0.5f)
        {
            fadeTimer += Time.deltaTime;
            float progress = fadeTimer / 0.5f;
            newTaskText.color = new Color(newTaskText.color.r, newTaskText.color.g, newTaskText.color.b, Mathf.Lerp(0f, 1f, progress));
            yield return null;
        }

        // УБРАНО скрытие панели - теперь она остается видимой с новым заданием
    }

    void OnDestroy()
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
    }
}