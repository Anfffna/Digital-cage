using System.Collections;
using UnityEngine;

public class EntryScene2 : MonoBehaviour
{
    [Header("Cursor Settings")]
    public CursorUI cursorManager;

    void Start()
    {
        // Автоматически находим курсор если не задан
        if (cursorManager == null)
        {
            cursorManager = FindObjectOfType<CursorUI>();
        }

        if (cursorManager != null)
        {
            // ПРИНУДИТЕЛЬНОЕ ВЫКЛЮЧЕНИЕ ДЛЯ ВТОРОЙ СЦЕНЫ
            cursorManager.HideCursor();

            // Двойная проверка через кадр
            StartCoroutine(ForceHideCursor());

            Debug.Log("EntryScene2: Курсор выключен для сцены 2");
        }
        else
        {
            Debug.LogError("EntryScene2: CursorUI не найден!");
        }
    }

    private IEnumerator ForceHideCursor()
    {
        yield return new WaitForEndOfFrame();

        // Еще раз выключаем на всякий случай
        if (cursorManager != null && cursorManager.IsActive())
        {
            cursorManager.HideCursor();
            Debug.Log("EntryScene2: Курсор принудительно выключен повторно");
        }
    }
}