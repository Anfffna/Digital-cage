using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class ScaryToDo : MonoBehaviour
{
    [Header("UI Settings")]
    public GameObject taskListParent;
    public List<GameObject> taskItems = new List<GameObject>();

    [Header("Blink Settings")]
    public float blinkInterval = 0.5f;

    [Header("Glitch Settings")]
    public float glitchInterval = 2f;
    public float typeSpeed = 0.05f;
    public List<string> glitchSymbols = new List<string> { "#", "@", "%", "&", "*", "?", "!", "~" };

    [Header("Appearance Settings")]
    public float fadeInDuration = 1.5f; // Длительность появления плашки
    public AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1); // Кривая плавности

    private Coroutine blinkCoroutine;
    private Coroutine glitchCoroutine;
    private List<TMP_Text> taskTexts = new List<TMP_Text>();
    private List<string> originalTexts = new List<string>();
    private CanvasGroup canvasGroup;

    void Start()
    {
        // Получаем или создаем CanvasGroup для плавного появления
        if (taskListParent != null)
        {
            canvasGroup = taskListParent.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = taskListParent.AddComponent<CanvasGroup>();
            }

            // Скрываем плашку при старте
            canvasGroup.alpha = 0f;
            taskListParent.SetActive(false);
        }

        // Сохраняем оригинальные тексты
        foreach (var taskItem in taskItems)
        {
            if (taskItem != null)
            {
                TMP_Text textComponent = taskItem.GetComponentInChildren<TMP_Text>();
                if (textComponent != null)
                {
                    taskTexts.Add(textComponent);
                    originalTexts.Add(textComponent.text);
                }
            }
        }
    }

    public void ShowTaskList()
    {
        if (taskListParent != null)
        {
            taskListParent.SetActive(true);
            StartCoroutine(FadeInTaskList());
        }

        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
        }
        if (glitchCoroutine != null)
        {
            StopCoroutine(glitchCoroutine);
        }

        blinkCoroutine = StartCoroutine(BlinkTaskItems());
        glitchCoroutine = StartCoroutine(GlitchTextRoutine());
    }

    /// <summary>
    /// Плавное появление плашки
    /// </summary>
    private IEnumerator FadeInTaskList()
    {
        if (canvasGroup == null) yield break;

        float timer = 0f;

        while (timer < fadeInDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / fadeInDuration;
            canvasGroup.alpha = fadeCurve.Evaluate(progress);
            yield return null;
        }

        canvasGroup.alpha = 1f;
    }

    /// <summary>
    /// Плавное исчезновение плашки
    /// </summary>
    private IEnumerator FadeOutTaskList()
    {
        if (canvasGroup == null) yield break;

        float timer = 0f;
        float startAlpha = canvasGroup.alpha;

        while (timer < fadeInDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / fadeInDuration;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, progress);
            yield return null;
        }

        canvasGroup.alpha = 0f;
        taskListParent.SetActive(false);
    }

    private IEnumerator BlinkTaskItems()
    {
        // Ждем пока плашка полностью появится
        yield return new WaitForSeconds(fadeInDuration);

        while (true)
        {
            foreach (var taskItem in taskItems)
            {
                if (taskItem != null)
                    taskItem.SetActive(false);
            }
            yield return new WaitForSeconds(blinkInterval);

            foreach (var taskItem in taskItems)
            {
                if (taskItem != null)
                    taskItem.SetActive(true);
            }
            yield return new WaitForSeconds(blinkInterval);
        }
    }

    private IEnumerator GlitchTextRoutine()
    {
        // Ждем пока плашка полностью появится
        yield return new WaitForSeconds(fadeInDuration);

        while (true)
        {
            yield return new WaitForSeconds(glitchInterval);

            if (taskTexts.Count > 0)
            {
                int randomIndex = Random.Range(0, taskTexts.Count);
                yield return StartCoroutine(GlitchTextEffect(randomIndex));
            }
        }
    }

    private IEnumerator GlitchTextEffect(int textIndex)
    {
        if (textIndex >= taskTexts.Count || taskTexts[textIndex] == null) yield break;

        TMP_Text textComponent = taskTexts[textIndex];
        string originalText = originalTexts[textIndex];

        // Фаза 1: Стираем текст
        for (int i = originalText.Length; i >= 0; i--)
        {
            textComponent.text = originalText.Substring(0, i);
            yield return new WaitForSeconds(typeSpeed / 2);
        }

        // Фаза 2: Показываем глючные символы
        string glitchText = "";
        for (int i = 0; i < originalText.Length; i++)
        {
            if (glitchSymbols.Count > 0)
            {
                string randomSymbol = glitchSymbols[Random.Range(0, glitchSymbols.Count)];
                glitchText += randomSymbol;
                textComponent.text = glitchText;
                yield return new WaitForSeconds(typeSpeed / 3);
            }
        }

        yield return new WaitForSeconds(0.5f);

        // Фаза 3: Восстанавливаем текст
        for (int i = 0; i <= originalText.Length; i++)
        {
            textComponent.text = originalText.Substring(0, i);
            yield return new WaitForSeconds(typeSpeed);
        }
    }

    public void HideTaskList()
    {
        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
            blinkCoroutine = null;
        }
        if (glitchCoroutine != null)
        {
            StopCoroutine(glitchCoroutine);
            glitchCoroutine = null;
        }

        if (taskListParent != null && taskListParent.activeInHierarchy)
        {
            StartCoroutine(FadeOutAndHide());
        }
        else
        {
            // Если плашка не активна, просто восстанавливаем тексты
            RestoreTexts();
        }
    }

    private IEnumerator FadeOutAndHide()
    {
        yield return StartCoroutine(FadeOutTaskList());
        RestoreTexts();
    }

    private void RestoreTexts()
    {
        // Восстанавливаем оригинальные тексты
        for (int i = 0; i < taskTexts.Count; i++)
        {
            if (taskTexts[i] != null)
            {
                taskTexts[i].text = originalTexts[i];
            }
        }

        foreach (var taskItem in taskItems)
        {
            if (taskItem != null)
                taskItem.SetActive(true);
        }
    }

    [ContextMenu("Test Show Task List")]
    public void TestShowTaskList()
    {
        ShowTaskList();
    }

    [ContextMenu("Test Hide Task List")]
    public void TestHideTaskList()
    {
        HideTaskList();
    }

    void OnDestroy()
    {
        if (blinkCoroutine != null)
            StopCoroutine(blinkCoroutine);
        if (glitchCoroutine != null)
            StopCoroutine(glitchCoroutine);
    }
}