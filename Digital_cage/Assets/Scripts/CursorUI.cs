using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CursorUI : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public RectTransform cursorRect;
    public Canvas canvas;

    private bool _isActive = true;

    // ДОБАВЬ ЭТОТ МЕТОД
    public bool IsActive()
    {
        return _isActive;
    }

    void Start()
    {
        DontDestroyOnLoad(gameObject);
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.None;

        if (cursorRect != null)
        {
            cursorRect.gameObject.SetActive(true);
            var image = cursorRect.GetComponent<Image>();
            if (image != null) image.raycastTarget = true;
        }
    }

    void Update()
    {
        // ПРИНУДИТЕЛЬНАЯ ПРОВЕРКА В БИЛДЕ
#if !UNITY_EDITOR
        if (!_isActive && cursorRect != null && cursorRect.gameObject.activeInHierarchy)
        {
            Debug.Log("?? БИЛД: Принудительно выключаем курсор!");
            cursorRect.gameObject.SetActive(false);
            return;
        }
#endif

        if (cursorRect == null || canvas == null || !_isActive)
            return;

        // Обычная логика движения курсора...
        Vector2 mousePos = Input.mousePosition;
        Vector2 localPos;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            mousePos,
            canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
            out localPos
        );

        cursorRect.anchoredPosition = localPos;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!_isActive) return;

        var allCanvases = FindObjectsOfType<Canvas>();
        foreach (var canvas in allCanvases)
        {
            if (canvas.gameObject.activeInHierarchy && canvas != this.canvas)
            {
                ExecuteEvents.ExecuteHierarchy(canvas.gameObject, eventData, ExecuteEvents.pointerDownHandler);
            }
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!_isActive) return;

        var allCanvases = FindObjectsOfType<Canvas>();
        foreach (var canvas in allCanvases)
        {
            if (canvas.gameObject.activeInHierarchy && canvas != this.canvas)
            {
                ExecuteEvents.ExecuteHierarchy(canvas.gameObject, eventData, ExecuteEvents.pointerUpHandler);
                ExecuteEvents.ExecuteHierarchy(canvas.gameObject, eventData, ExecuteEvents.pointerClickHandler);
            }
        }
    }

    public void HideCursor()
    {
        _isActive = false;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        if (cursorRect != null)
        {
            cursorRect.gameObject.SetActive(false);
            var image = cursorRect.GetComponent<Image>();
            if (image != null) image.raycastTarget = false;
        }
    }

    public void ShowCursor()
    {
        _isActive = true;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.None;

        if (cursorRect != null)
        {
            cursorRect.gameObject.SetActive(true);
            var image = cursorRect.GetComponent<Image>();
            if (image != null) image.raycastTarget = true;
        }
    }
}