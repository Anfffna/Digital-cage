using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SignatureDrawerTexture : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    public int textureWidth = 512;
    public int textureHeight = 256;
    public Color drawColor = Color.black;
    public int penSize = 4;

    private Texture2D signatureTexture;
    private RawImage rawImage;
    private bool isDrawing = false;
    private RectTransform rectTransform;
    private Vector2 lastPos;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        rawImage = GetComponent<RawImage>();

        // создаём чистую белую текстуру для подписи
        signatureTexture = new Texture2D(textureWidth, textureHeight, TextureFormat.RGBA32, false);
        ClearTexture();
        rawImage.texture = signatureTexture;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isDrawing = true;
        lastPos = Vector2.zero; // Сбрасываем предыдущую позицию
        DrawAt(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isDrawing)
            DrawAt(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isDrawing = false;
        lastPos = Vector2.zero; // Сбрасываем при отпускании
    }

    private void DrawAt(PointerEventData eventData)
    {
        // Конвертируем координаты курсора в координаты внутри RawImage
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, eventData.position, eventData.pressEventCamera, out Vector2 localPos))
        {
            float x = (localPos.x + rectTransform.rect.width / 2f) * (textureWidth / rectTransform.rect.width);
            float y = (localPos.y + rectTransform.rect.height / 2f) * (textureHeight / rectTransform.rect.height);

            Vector2 currentPos = new Vector2(x, y);

            // Если это первая точка (OnPointerDown) - рисуем круг
            if (lastPos == Vector2.zero)
            {
                DrawCircle((int)currentPos.x, (int)currentPos.y);
            }
            else
            {
                // Рисуем линию между предыдущей и текущей точками
                DrawLine((int)lastPos.x, (int)lastPos.y, (int)currentPos.x, (int)currentPos.y);
            }

            lastPos = currentPos;
            signatureTexture.Apply();
        }
    }

    private void DrawLine(int x0, int y0, int x1, int y1)
    {
        int dx = Mathf.Abs(x1 - x0);
        int dy = Mathf.Abs(y1 - y0);
        int sx = x0 < x1 ? 1 : -1;
        int sy = y0 < y1 ? 1 : -1;
        int err = dx - dy;

        while (true)
        {
            DrawCircle(x0, y0);
            if (x0 == x1 && y0 == y1) break;
            int e2 = 2 * err;
            if (e2 > -dy) { err -= dy; x0 += sx; }
            if (e2 < dx) { err += dx; y0 += sy; }
        }
    }

    private void DrawCircle(int cx, int cy)
    {
        for (int x = -penSize; x <= penSize; x++)
        {
            for (int y = -penSize; y <= penSize; y++)
            {
                if (x * x + y * y <= penSize * penSize)
                {
                    int px = cx + x;
                    int py = cy + y;
                    if (px >= 0 && px < textureWidth && py >= 0 && py < textureHeight)
                        signatureTexture.SetPixel(px, py, drawColor);
                }
            }
        }
    }

    public void ClearTexture()
    {
        Color32[] pixels = new Color32[textureWidth * textureHeight];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.white;
        signatureTexture.SetPixels32(pixels);
        signatureTexture.Apply();
    }

    public bool IsSigned()
    {
        Color32[] pixels = signatureTexture.GetPixels32();
        int blackCount = 0;
        foreach (var p in pixels)
        {
            if (p.r < 10 && p.g < 10 && p.b < 10) blackCount++;
        }

        float ratio = (float)blackCount / pixels.Length;

        // ДЕБАГ ИНФОРМАЦИЯ
        Debug.Log($"Подпись: {blackCount} черных пикселей из {pixels.Length} ({ratio * 100:F2}%) - {(ratio >= 0.05f ? "ПОДПИСАНО" : "НЕ ПОДПИСАНО")}");

        return ratio >= 0.15f; // 5% пикселей закрашено
    }
}