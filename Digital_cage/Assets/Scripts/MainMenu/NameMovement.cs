using UnityEngine;
using System.Collections;

public class NameMovement : MonoBehaviour
{
    [Header("Основные настройки")]
    [SerializeField] private GameObject secondImage;
    [SerializeField] private float appearDuration = 0.5f;

    [Header("Настройки дрожания")]
    [SerializeField] private float shakeIntensity = 10f; // Временно увеличил для теста
    [SerializeField] private float shakeSpeed = 3.0f;
    [SerializeField] private float secondImageAlpha = 0.7f; // Прозрачность второй картинки

    private Vector3 originalSecondImagePosition;
    private CanvasGroup canvasGroup1;
    private CanvasGroup canvasGroup2;
    private bool isShaking = false;

    void Start()
    {
        Debug.Log("NameMovement Start вызван");

        canvasGroup1 = GetComponent<CanvasGroup>();
        if (canvasGroup1 == null)
            canvasGroup1 = gameObject.AddComponent<CanvasGroup>();

        if (secondImage == null)
        {
            Debug.LogError("Second image not assigned!");
            return;
        }

        canvasGroup2 = secondImage.GetComponent<CanvasGroup>();
        if (canvasGroup2 == null)
            canvasGroup2 = secondImage.AddComponent<CanvasGroup>();

        originalSecondImagePosition = secondImage.transform.localPosition;
        Debug.Log("Оригинальная позиция: " + originalSecondImagePosition);

        // Начальная невидимость
        canvasGroup1.alpha = 0f;
        canvasGroup2.alpha = 0f;

        StartCoroutine(StartAnimation());
    }

    IEnumerator StartAnimation()
    {
        // Плавное появление
        yield return StartCoroutine(FadeInBothImages());

        // Устанавливаем полупрозрачность для второй картинки
        if (canvasGroup2 != null)
        {
            canvasGroup2.alpha = secondImageAlpha;
        }

        // Запускаем дрожание
        isShaking = true;
        StartCoroutine(ShakeSecondImage());
    }

    IEnumerator FadeInBothImages()
    {
        float elapsedTime = 0f;

        while (elapsedTime < appearDuration)
        {
            float alpha = Mathf.Lerp(0f, 1f, elapsedTime / appearDuration);

            if (canvasGroup1 != null) canvasGroup1.alpha = alpha;
            if (canvasGroup2 != null) canvasGroup2.alpha = alpha;

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        if (canvasGroup1 != null) canvasGroup1.alpha = 1f;
        if (canvasGroup2 != null) canvasGroup2.alpha = 1f;
    }

    IEnumerator ShakeSecondImage()
    {
        Debug.Log("Дрожание началось! Интенсивность: " + shakeIntensity);

        while (isShaking && secondImage != null)
        {
            float offsetX = Mathf.Sin(Time.time * shakeSpeed) * shakeIntensity;
            float offsetY = Mathf.Cos(Time.time * shakeSpeed * 1.3f) * shakeIntensity;

            secondImage.transform.localPosition = originalSecondImagePosition +
                                                new Vector3(offsetX, offsetY, 0f);

            yield return null;
        }
    }
}