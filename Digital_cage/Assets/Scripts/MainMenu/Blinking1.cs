using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Blinking1 : MonoBehaviour
{
    [Header("Настройки мигания")]
    [SerializeField] private float blinkInterval = 1.0f;
    [SerializeField] private float fadeInTime = 0.3f;
    [SerializeField] private float fadeOutTime = 0.3f;
    [SerializeField] private bool startVisible = true;
    [SerializeField] private bool startAutomatically = true;

    private Image imageComponent;
    private CanvasGroup canvasGroup;

    void Start()
    {
        // Пытаемся получить Image компонент
        imageComponent = GetComponent<Image>();

        // Создаем или получаем CanvasGroup для плавного альфа-канала
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        // Устанавливаем начальную видимость
        if (startVisible)
        {
            canvasGroup.alpha = 1f;
        }
        else
        {
            canvasGroup.alpha = 0f;
        }

        // Запускаем мигание если нужно
        if (startAutomatically)
        {
            StartBlinking();
        }
    }

    // Метод для начала мигания
    public void StartBlinking()
    {
        StopAllCoroutines();
        StartCoroutine(BlinkLoop());
    }

    // Метод для остановки мигания
    public void StopBlinking()
    {
        StopAllCoroutines();
    }

    // Метод для установки видимости (true - видно, false - невидно)
    public void SetVisible(bool visible)
    {
        StopAllCoroutines();
        canvasGroup.alpha = visible ? 1f : 0f;
    }

    IEnumerator BlinkLoop()
    {
        while (true)
        {
            // Плавное появление
            yield return StartCoroutine(FadeAlpha(0f, 1f, fadeInTime));

            // Ждем перед исчезновением
            yield return new WaitForSeconds(blinkInterval - fadeInTime - fadeOutTime);

            // Плавное исчезновение
            yield return StartCoroutine(FadeAlpha(1f, 0f, fadeOutTime));
        }
    }

    IEnumerator FadeAlpha(float fromAlpha, float toAlpha, float duration)
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = Mathf.Lerp(fromAlpha, toAlpha, elapsedTime / duration);
            }
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Убедимся, что достигли конечного значения
        if (canvasGroup != null)
        {
            canvasGroup.alpha = toAlpha;
        }
    }
}