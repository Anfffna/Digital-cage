using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BlackScreenController : MonoBehaviour
{
    public Image blackScreenImage;
    public float fadeDuration = 2f;
    public float blackScreenDuration = 3f;

    public IEnumerator ShowBlackScreen()
    {
        Debug.Log("Начинаем черный экран...");

        // Убедимся, что черный экран активен
        blackScreenImage.gameObject.SetActive(true);

        // Плавное появление
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, timer / fadeDuration);
            Color newColor = blackScreenImage.color;
            newColor.a = alpha;
            blackScreenImage.color = newColor;
            yield return null;
        }

        // ПОИСК ДУБЛИКАТОВ КУРСОРА
        Debug.Log("?? Поиск дубликатов CursorUI...");
        CursorUI[] allCursors = FindObjectsOfType<CursorUI>();
        Debug.Log($"Найдено CursorUI: {allCursors.Length}");

        foreach (CursorUI cursor in allCursors)
        {
            Debug.Log($"CursorUI: {cursor.gameObject.name}, Active: {cursor.gameObject.activeInHierarchy}, Scene: {cursor.gameObject.scene.name}");
        }

        // ВЫКЛЮЧАЕМ ВСЕ КУРСОРЫ
        foreach (CursorUI cursor in allCursors)
        {
            cursor.HideCursor();
            Debug.Log($"Выключен курсор: {cursor.gameObject.name}");
        }

        // Полностью черный
        Color fullBlack = blackScreenImage.color;
        fullBlack.a = 1f;
        blackScreenImage.color = fullBlack;

        // Ждем 3 секунды
        yield return new WaitForSeconds(blackScreenDuration);

        // Плавное исчезновение
        timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, timer / fadeDuration);
            Color newColor = blackScreenImage.color;
            newColor.a = alpha;
            blackScreenImage.color = newColor;
            yield return null;
        }

        // Выключаем
        blackScreenImage.gameObject.SetActive(false);
        Debug.Log("Черный экран завершен");
    }
}