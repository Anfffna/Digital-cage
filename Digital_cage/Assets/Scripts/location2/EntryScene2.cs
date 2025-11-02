using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class EntryScene2 : MonoBehaviour
{
    [Header("Cursor Settings")]
    public CursorUI cursorManager;

    [Header("Black Screen Settings")]
    public Image blackScreenImage;
    public float blackScreenDuration = 1f;
    public float fadeOutDuration = 2f;

    void Start()
    {
        // Запускаем последовательность: черный экран + выключение курсора
        StartCoroutine(SceneStartSequence());
    }

    private IEnumerator SceneStartSequence()
    {
        // Шаг 1: Активируем черный экран
        if (blackScreenImage != null)
        {
            blackScreenImage.gameObject.SetActive(true);
            blackScreenImage.color = new Color(0, 0, 0, 1);
            Debug.Log("EntryScene2: Черный экран активирован");
        }

        // Шаг 2: Выключаем курсор
        if (cursorManager == null)
        {
            cursorManager = FindObjectOfType<CursorUI>();
        }

        if (cursorManager != null)
        {
            cursorManager.HideCursor();
            Debug.Log("EntryScene2: Курсор выключен");

            // Двойная проверка через кадр
            StartCoroutine(ForceHideCursor());
        }
        else
        {
            Debug.LogError("EntryScene2: CursorUI не найден!");
        }

        // Шаг 3: Ждем указанное время черного экрана
        yield return new WaitForSeconds(blackScreenDuration);

        // Шаг 4: Плавно убираем черный экран
        if (blackScreenImage != null)
        {
            yield return StartCoroutine(FadeOutBlackScreen());
        }

        Debug.Log("EntryScene2: Сцена инициализирована");
    }

    private IEnumerator ForceHideCursor()
    {
        yield return new WaitForEndOfFrame();

        if (cursorManager != null && cursorManager.IsActive())
        {
            cursorManager.HideCursor();
            Debug.Log("EntryScene2: Курсор принудительно выключен повторно");
        }
    }

    private IEnumerator FadeOutBlackScreen()
    {
        float timer = 0f;
        Color startColor = blackScreenImage.color;
        Color endColor = new Color(0, 0, 0, 0);

        while (timer < fadeOutDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / fadeOutDuration;
            blackScreenImage.color = Color.Lerp(startColor, endColor, progress);
            yield return null;
        }

        blackScreenImage.gameObject.SetActive(false);
        Debug.Log("EntryScene2: Черный экран скрыт");
    }
}