using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.UI;

public class TodoUI6 : MonoBehaviour
{
    [Header("Task 1 Settings")]
    public TextMeshProUGUI task1Text;    // Первый пункт который нужно скрыть

    [Header("UI Panel")]
    public CanvasGroup todoPanel;        // Панель Todo (если есть)

    [Header("Completion Settings")]
    public float hideDelay = 0.3f;       // Задержка перед скрытием

    private bool task1Hidden = false;
    private bool isPanelShowing = false;

    void Start()
    {
        // Если есть панель - скрываем ее
        if (todoPanel != null)
        {
            todoPanel.alpha = 0f;
            todoPanel.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Показать Todo панель (для ManagerDialogue6)
    /// </summary>
    public void ShowPanel()
    {
        if (isPanelShowing || todoPanel == null) return;

        isPanelShowing = true;
        todoPanel.gameObject.SetActive(true);
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
    }

    /// <summary>
    /// Скрыть первую задачу (вызывается из SitWork когда игрок садится)
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

        // 1. Зачеркиваем текст
        task1Text.fontStyle |= FontStyles.Strikethrough;

        // 2. Меняем цвет на серый
        Color originalColor = task1Text.color;
        Color completedColor = new Color(0.5f, 0.5f, 0.5f, 0.7f);
        task1Text.color = completedColor;

        // 3. Ждем немного
        yield return new WaitForSeconds(hideDelay);

        // 4. Плавно исчезаем
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

        // 5. Полностью скрываем
        task1Text.gameObject.SetActive(false);

        // 6. Если есть панель - скрываем ее через секунду
        if (todoPanel != null)
        {
            yield return new WaitForSeconds(1f);
            StartCoroutine(FadeOutPanel());
        }
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
    /// Проверка, скрыта ли уже задача
    /// </summary>
    public bool IsTask1Hidden()
    {
        return task1Hidden;
    }
}