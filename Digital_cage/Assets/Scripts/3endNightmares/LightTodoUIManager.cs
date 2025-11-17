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
    public TextMeshProUGUI newTaskText;
    public TextMeshProUGUI thirdTaskText;

    private bool isShowing = false;
    private bool isLightTaskCompleted = false;
    private bool isNewTaskCompleted = false;
    private Coroutine fadeCoroutine;

    void Start()
    {
        if (todoPanel != null)
        {
            todoPanel.alpha = 0f;
            todoPanel.gameObject.SetActive(false);
        }

        if (newTaskText != null)
        {
            newTaskText.gameObject.SetActive(false);
        }

        if (thirdTaskText != null)
        {
            thirdTaskText.gameObject.SetActive(false);
        }
    }

    public bool IsNewTaskActive()
    {
        return newTaskText != null && newTaskText.gameObject.activeInHierarchy;
    }

    public bool IsThirdTaskActive()
    {
        return thirdTaskText != null && thirdTaskText.gameObject.activeInHierarchy;
    }

    public void ShowTodoList()
    {
        if (isShowing || todoPanel == null) return;

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
        if (isLightTaskCompleted || lightTaskText == null || newTaskText == null) return;

        Debug.Log("LightTodoUIManager: CompleteLightTask() вызван");
        isLightTaskCompleted = true;
        StartCoroutine(ReplaceTaskText());
    }

    private IEnumerator ReplaceTaskText()
    {
        if (lightTaskText == null || newTaskText == null) yield break;

        Debug.Log("LightTodoUIManager: Начинаем замену текста");

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

        Debug.Log("LightTodoUIManager: newTaskText активирован!");

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

    public void CompleteThirdTask()
    {
        if (isNewTaskCompleted || newTaskText == null || thirdTaskText == null) return;

        Debug.Log("LightTodoUIManager: CompleteThirdTask() вызван");
        isNewTaskCompleted = true;
        StartCoroutine(ReplaceWithThirdTask());
    }

    private IEnumerator ReplaceWithThirdTask()
    {
        if (newTaskText == null || thirdTaskText == null) yield break;

        Debug.Log("LightTodoUIManager: Заменяем на третий пункт");

        // Исчезает второй текст
        float fadeTimer = 0f;
        Color startColor = newTaskText.color;

        while (fadeTimer < 0.5f)
        {
            fadeTimer += Time.deltaTime;
            float progress = fadeTimer / 0.5f;
            newTaskText.color = new Color(startColor.r, startColor.g, startColor.b, Mathf.Lerp(1f, 0f, progress));
            yield return null;
        }

        newTaskText.gameObject.SetActive(false);

        // Показываем третий текст
        thirdTaskText.gameObject.SetActive(true);
        thirdTaskText.color = new Color(thirdTaskText.color.r, thirdTaskText.color.g, thirdTaskText.color.b, 0f);

        Debug.Log("LightTodoUIManager: thirdTaskText активирован!");

        // Появляется третий текст
        fadeTimer = 0f;
        while (fadeTimer < 0.5f)
        {
            fadeTimer += Time.deltaTime;
            float progress = fadeTimer / 0.5f;
            thirdTaskText.color = new Color(thirdTaskText.color.r, thirdTaskText.color.g, thirdTaskText.color.b, Mathf.Lerp(0f, 1f, progress));
            yield return null;
        }
    }

    public void ReplaceCurrentTaskWithExitText(string exitText = "Exit text")
    {
        Debug.Log("LightTodoUIManager: ReplaceCurrentTaskWithExitText вызван");

        // Проверяем какой пункт сейчас активен и заменяем его
        if (IsThirdTaskActive() && thirdTaskText != null)
        {
            // Если третий пункт активен - заменяем его
            StartCoroutine(ReplaceTaskWithExitText(thirdTaskText, exitText));
            Debug.Log("LightTodoUIManager: Заменяем третий пункт на Exit text");
        }
        else if (IsNewTaskActive() && newTaskText != null)
        {
            // Если второй пункт активен - заменяем его
            StartCoroutine(ReplaceTaskWithExitText(newTaskText, exitText));
            Debug.Log("LightTodoUIManager: Заменяем второй пункт на Exit text");
        }
        else if (lightTaskText != null && lightTaskText.gameObject.activeInHierarchy)
        {
            // Если только первый пункт активен - заменяем его
            StartCoroutine(ReplaceTaskWithExitText(lightTaskText, exitText));
            Debug.Log("LightTodoUIManager: Заменяем первый пункт на Exit text");
        }
        else
        {
            Debug.LogWarning("LightTodoUIManager: Нет активных пунктов для замены!");
        }
    }

    private IEnumerator ReplaceTaskWithExitText(TextMeshProUGUI taskText, string exitText)
    {
        if (taskText == null) yield break;

        Debug.Log($"LightTodoUIManager: Заменяем текст на '{exitText}'");

        // Исчезает текущий текст
        float fadeTimer = 0f;
        Color startColor = taskText.color;

        while (fadeTimer < 0.5f)
        {
            fadeTimer += Time.deltaTime;
            float progress = fadeTimer / 0.5f;
            taskText.color = new Color(startColor.r, startColor.g, startColor.b, Mathf.Lerp(1f, 0f, progress));
            yield return null;
        }

        // Меняем текст на Exit text
        taskText.text = exitText;
        taskText.color = new Color(taskText.color.r, taskText.color.g, taskText.color.b, 0f);

        // Появляется новый текст
        fadeTimer = 0f;
        while (fadeTimer < 0.5f)
        {
            fadeTimer += Time.deltaTime;
            float progress = fadeTimer / 0.5f;
            taskText.color = new Color(taskText.color.r, taskText.color.g, taskText.color.b, Mathf.Lerp(0f, 1f, progress));
            yield return null;
        }

        Debug.Log("LightTodoUIManager: Текст успешно заменен на Exit text");
    }

    void OnDestroy()
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
    }
}