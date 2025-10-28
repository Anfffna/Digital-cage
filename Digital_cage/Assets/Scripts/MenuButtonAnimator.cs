using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;

public class MenuButtonAnimator : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Animation Settings")]
    public float animationDuration = 0.3f;
    public float scaleMultiplier = 1.2f;
    public Color hoverColor = Color.yellow;

    [Header("References")]
    public TextMeshProUGUI text;

    private Color originalColor;
    private float originalFontSize;
    private Button button;

    // Для плавной анимации
    private bool isAnimating = false;
    private float animationTime = 0f;
    private bool isHovered = false;

    void Start()
    {
        // Получаем ссылки на компоненты
        button = GetComponent<Button>();
        if (text == null)
            text = GetComponent<TextMeshProUGUI>();

        // Сохраняем оригинальные значения
        originalColor = text.color;
        originalFontSize = text.fontSize;

        // Если это кнопка, делаем ненажимаемой визуально
        if (button != null)
        {
            // Убираем стандартные transition у кнопки
            button.transition = Selectable.Transition.None;
        }
    }

    void Update()
    {
        // Плавная анимация при помощи Lerp
        if (isAnimating)
        {
            animationTime += Time.deltaTime;
            float progress = Mathf.Clamp01(animationTime / animationDuration);

            if (isHovered)
            {
                // Анимация при наведении
                text.color = Color.Lerp(originalColor, hoverColor, progress);
                text.fontSize = Mathf.Lerp(originalFontSize, originalFontSize * scaleMultiplier, progress);
            }
            else
            {
                // Анимация при уходе курсора
                text.color = Color.Lerp(hoverColor, originalColor, progress);
                text.fontSize = Mathf.Lerp(originalFontSize * scaleMultiplier, originalFontSize, progress);
            }

            // Завершаем анимацию
            if (progress >= 1f)
            {
                isAnimating = false;
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (button == null || button.interactable)
        {
            StartHoverAnimation();
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (button == null || button.interactable)
        {
            EndHoverAnimation();
        }
    }

    private void StartHoverAnimation()
    {
        isHovered = true;
        isAnimating = true;
        animationTime = 0f;
    }

    private void EndHoverAnimation()
    {
        isHovered = false;
        isAnimating = true;
        animationTime = 0f;
    }

    // Опционально: сброс при отключении кнопки
    void OnDisable()
    {
        // Возвращаем оригинальные значения при отключении объекта
        if (text != null)
        {
            text.color = originalColor;
            text.fontSize = originalFontSize;
        }
        isAnimating = false;
    }
}