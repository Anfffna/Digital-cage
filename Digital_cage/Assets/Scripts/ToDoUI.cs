using UnityEngine;
using TMPro;
using System.Collections;

public class ToDoUI : MonoBehaviour
{
    [Header("UI References")]
    public CanvasGroup panel;
    public TextMeshProUGUI[] items;

    [Header("Fade Settings")]
    public float fadeDuration = 0.5f;

    // ====== Новая часть ======
    [Header("Task Progress Control")]
    [Tooltip("Какой пункт сейчас разрешён к выполнению (начинается с 0)")]
    public int currentTaskIndex = 0;

    void Start()
    {
        if (panel != null)
        {
            panel.alpha = 0f;
            panel.gameObject.SetActive(false);
        }
    }

    // ===== Проверка, можно ли выполнить пункт =====
    public bool CanCompleteTask(int index)
    {
        return index == currentTaskIndex;
    }

    // ===== Отметить пункт выполненным =====
    public void MarkItemDone(int index)
    {
        if (items == null || index < 0 || index >= items.Length) return;

        // Нельзя выполнять пункт, если предыдущий ещё не сделан
        if (!CanCompleteTask(index))
        {
            Debug.Log($"Пункт {index} недоступен. Сначала завершите пункт {currentTaskIndex}.");
            return;
        }

        // === КОМБИНИРОВАННЫЙ ВАРИАНТ: FontStyles + цвет ===
        TextMeshProUGUI item = items[index];

        // Проверяем, не выполнена ли уже эта задача
        if ((item.fontStyle & FontStyles.Strikethrough) == 0)
        {
            // Сохраняем оригинальный текст
            string originalText = item.text;

            // 1. Зачеркивание через FontStyles (для совместимости с проверкой)
            item.fontStyle |= FontStyles.Strikethrough;

            // 2. Серый цвет для красоты
            item.color = new Color(0.5f, 0.5f, 0.5f, 0.7f);

            Debug.Log($"ToDoUI: Пункт {index} выполнен - '{originalText}'");
        }

        // Разрешаем следующий пункт
        currentTaskIndex++;

        // Проверяем, все ли пункты выполнены (старая проверка работает)
        bool allDone = true;
        foreach (var itm in items)
        {
            if ((itm.fontStyle & FontStyles.Strikethrough) == 0)
            {
                allDone = false;
                break;
            }
        }

        if (allDone)
            StartCoroutine(FadeOutPanel());
    }

    // ===== Показ панели =====
    public void ShowPanel()
    {
        if (panel != null)
        {
            panel.gameObject.SetActive(true);
            StartCoroutine(FadeInPanel());
        }
    }

    public IEnumerator FadeInPanel()
    {
        if (panel == null) yield break;

        panel.gameObject.SetActive(true);
        float time = 0f;
        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            panel.alpha = Mathf.Lerp(0f, 1f, time / fadeDuration);
            yield return null;
        }
        panel.alpha = 1f;
    }

    public IEnumerator FadeOutPanel()
    {
        if (panel == null) yield break;

        float time = 0f;
        float startAlpha = panel.alpha;
        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            panel.alpha = Mathf.Lerp(startAlpha, 0f, time / fadeDuration);
            yield return null;
        }
        panel.alpha = 0f;
        panel.gameObject.SetActive(false);
    }
}
