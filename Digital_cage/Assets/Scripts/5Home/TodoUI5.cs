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
    public TextMeshProUGUI lightTaskText;    // ПЕРВЫЙ пункт - выключить свет
    public TextMeshProUGUI newTaskText;      // ВТОРОЙ пункт - на замену первого
    public TextMeshProUGUI thirdTaskText;    // ТРЕТИЙ пункт - на замену второго (после сна)

    [Header("Replacement Settings")]
    public float replacementFadeDuration = 0.5f; // Длительность замены

    private bool isShowing = false;
    private bool isLightTaskCompleted = false;
    private bool isSecondTaskCompleted = false;
    private Coroutine fadeCoroutine;

    void Start()
    {
        if (todoPanel != null)
        {
            todoPanel.alpha = 0f;
            todoPanel.gameObject.SetActive(false);
        }

        // Второй пункт скрыт изначально - он появится ВМЕСТО первого
        if (newTaskText != null)
        {
            newTaskText.gameObject.SetActive(false);
        }

        // Третий пункт скрыт изначально - он появится ВМЕСТО второго
        if (thirdTaskText != null)
        {
            thirdTaskText.gameObject.SetActive(false);
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
    /// Завершить первую задачу (выключить свет)
    /// </summary>
    public void CompleteLightTask()
    {
        if (isLightTaskCompleted) return;

        Debug.Log("TodoUI5: CompleteLightTask() вызван");
        isLightTaskCompleted = true;

        // Зачеркиваем первый пункт и заменяем на второй
        StartCoroutine(ReplaceFirstWithSecondTask());
    }

    /// <summary>
    /// Завершить вторую задачу (после сна)
    /// </summary>
    public void CompleteSecondTask()
    {
        if (isSecondTaskCompleted || !isLightTaskCompleted) return;

        Debug.Log("TodoUI5: CompleteSecondTask() вызван (без анимации)");
        isSecondTaskCompleted = true;

        // БЫСТРАЯ ЗАМЕНА без плавной анимации (игрок не видит из-за черного экрана)
        StartCoroutine(ReplaceSecondWithThirdTaskFast());
    }

    /// <summary>
    /// Заменить первый пункт на второй
    /// </summary>
    private IEnumerator ReplaceFirstWithSecondTask()
    {
        if (lightTaskText == null) yield break;

        Debug.Log("TodoUI5: Заменяем первый пункт на второй");

        // 1. Зачеркиваем первый пункт
        lightTaskText.fontStyle |= FontStyles.Strikethrough;
        Color completedColor = new Color(0.5f, 0.5f, 0.5f, 0.7f);
        lightTaskText.color = completedColor;

        // 2. Ждем немного чтобы увидеть зачеркнутый пункт
        yield return new WaitForSeconds(0.3f);

        // 3. Если есть второй пункт - плавная замена
        if (newTaskText != null)
        {
            // Сохраняем позицию второго пункта
            Vector3 secondTaskOriginalPosition = newTaskText.transform.localPosition;

            // Устанавливаем второй пункт на позицию первого
            newTaskText.transform.localPosition = lightTaskText.transform.localPosition;

            // Показываем второй пункт прозрачным
            newTaskText.gameObject.SetActive(true);
            Color newTextColor = newTaskText.color;
            Color transparentColor = newTextColor;
            transparentColor.a = 0f;
            newTaskText.color = transparentColor;

            // Плавное исчезновение первого пункта
            float timer = 0f;
            Color firstStartColor = lightTaskText.color;
            Color firstEndColor = firstStartColor;
            firstEndColor.a = 0f;

            while (timer < replacementFadeDuration)
            {
                timer += Time.deltaTime;
                float progress = timer / replacementFadeDuration;

                // Исчезает первый
                lightTaskText.color = Color.Lerp(firstStartColor, firstEndColor, progress);

                // Появляется второй
                newTaskText.color = Color.Lerp(transparentColor, newTextColor, progress);

                yield return null;
            }

            // Полностью скрываем первый пункт
            lightTaskText.gameObject.SetActive(false);

            // Восстанавливаем цвет второго пункта
            newTaskText.color = newTextColor;

            Debug.Log("TodoUI5: Первый пункт заменен на второй");

            // НЕ скрываем панель - ждем выполнения сна
            // Панель останется видимой с вторым пунктом
        }
        else
        {
            // Если второго пункта нет - скрываем панель
            yield return new WaitForSeconds(1.5f);
            HidePanel();
        }
    }

    /// <summary>
    /// Заменить второй пункт на третий
    /// </summary>
    private IEnumerator ReplaceSecondWithThirdTaskFast()
    {
        // Проверяем, что второй пункт активен
        if (newTaskText == null || !newTaskText.gameObject.activeInHierarchy)
        {
            Debug.LogWarning("TodoUI5: Второй пункт не активен для замены");
            yield break;
        }

        Debug.Log("TodoUI5: Быстрая замена второго пункта на третий");

        // 1. Зачеркиваем второй пункт (игрок не видит - черный экран)
        newTaskText.fontStyle |= FontStyles.Strikethrough;
        newTaskText.color = new Color(0.5f, 0.5f, 0.5f, 0.7f);

        // 2. Минимальная задержка
        yield return new WaitForSeconds(0.1f);

        // 3. Если есть третий пункт - мгновенная замена
        if (thirdTaskText != null)
        {
            // Устанавливаем третий пункт на позицию второго
            thirdTaskText.transform.localPosition = newTaskText.transform.localPosition;

            // Скрываем второй, показываем третий
            newTaskText.gameObject.SetActive(false);
            thirdTaskText.gameObject.SetActive(true);

            // Устанавливаем нормальный цвет
            thirdTaskText.color = new Color(thirdTaskText.color.r, thirdTaskText.color.g, thirdTaskText.color.b, 1f);

            Debug.Log("TodoUI5: Второй пункт заменен на третий (быстро)");

            // НЕ скрываем панель сразу - она останется видимой когда исчезнет черный экран
        }
        else
        {
            // Если третьего пункта нет - скрываем панель
            yield return new WaitForSeconds(0.5f);
            HidePanel();
        }
    }

    /// <summary>
    /// Установить текст для второго пункта
    /// </summary>
    public void SetSecondTaskText(string text)
    {
        if (newTaskText != null)
        {
            newTaskText.text = text;
        }
    }

    /// <summary>
    /// Установить текст для третьего пункта
    /// </summary>
    public void SetThirdTaskText(string text)
    {
        if (thirdTaskText != null)
        {
            thirdTaskText.text = text;
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
    /// Проверка, активен ли третий пункт
    /// </summary>
    public bool IsThirdTaskActive()
    {
        return thirdTaskText != null && thirdTaskText.gameObject.activeInHierarchy;
    }

    /// <summary>
    /// Проверка, показана ли Todo панель
    /// </summary>
    public bool IsPanelShowing()
    {
        return isShowing;
    }

    /// <summary>
    /// Проверка, завершена ли первая задача
    /// </summary>
    public bool IsLightTaskCompleted()
    {
        return isLightTaskCompleted;
    }

    /// <summary>
    /// Проверка, завершена ли вторая задача
    /// </summary>
    public bool IsSecondTaskCompleted()
    {
        return isSecondTaskCompleted;
    }

    void OnDestroy()
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
    }
}