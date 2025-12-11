using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class EntryScene0 : MonoBehaviour
{
    [Header("Cursor Settings")]
    public CursorUI cursorManager;

    [Header("Black Screen Settings")]
    public Image blackScreenImage;
    public float blackScreenDuration = 1f;
    public float fadeOutDuration = 2f;

    private Coroutine sceneSequenceCoroutine;
    private Coroutine fadeCoroutine;

    void Start()
    {
        // Запускаем последовательность: черный экран + выключение курсора
        sceneSequenceCoroutine = StartCoroutine(SceneStartSequence());
    }

    private IEnumerator SceneStartSequence()
    {
        // Шаг 1: Активируем черный экран
        if (blackScreenImage != null)
        {
            blackScreenImage.gameObject.SetActive(true);
            blackScreenImage.color = new Color(0, 0, 0, 1);
            Debug.Log("EntryScene0: Черный экран активирован");
        }

        // Шаг 2: Выключаем курсор
        if (cursorManager == null)
        {
            cursorManager = FindObjectOfType<CursorUI>();
        }

        if (cursorManager != null)
        {
            cursorManager.HideCursor();
            Debug.Log("EntryScene0: Курсор выключен");

            // Двойная проверка через кадр
            StartCoroutine(ForceHideCursor());
        }
        else
        {
            Debug.LogError("EntryScene0: CursorUI не найден!");
        }

        // Шаг 3: Ждем указанное время черного экрана
        yield return new WaitForSeconds(blackScreenDuration);

        // Шаг 4: Плавно убираем черный экран
        if (blackScreenImage != null)
        {
            fadeCoroutine = StartCoroutine(FadeOutBlackScreen());
            yield return fadeCoroutine;
        }

        Debug.Log("EntryScene0: Сцена инициализирована");
    }

    private IEnumerator ForceHideCursor()
    {
        yield return new WaitForEndOfFrame();

        if (cursorManager != null && cursorManager.IsActive())
        {
            cursorManager.HideCursor();
            Debug.Log("EntryScene0: Курсор принудительно выключен повторно");
        }
    }

    private IEnumerator FadeOutBlackScreen()
    {
        if (blackScreenImage == null) yield break; // Защита от null

        float timer = 0f;
        Color startColor = blackScreenImage.color;
        Color endColor = new Color(0, 0, 0, 0);

        while (timer < fadeOutDuration)
        {
            if (blackScreenImage == null) yield break; // Защита в цикле

            timer += Time.deltaTime;
            float progress = timer / fadeOutDuration;
            blackScreenImage.color = Color.Lerp(startColor, endColor, progress);
            yield return null;
        }

        if (blackScreenImage != null) // Финальная защита
        {
            blackScreenImage.gameObject.SetActive(false);
            Debug.Log("EntryScene0: Черный экран скрыт");
        }
    }

    void OnDestroy()
    {
        // Останавливаем все корутины при уничтожении объекта
        if (sceneSequenceCoroutine != null)
        {
            StopCoroutine(sceneSequenceCoroutine);
        }
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
    }
}