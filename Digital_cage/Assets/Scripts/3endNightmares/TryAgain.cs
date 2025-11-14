using UnityEngine;
using System.Collections;

public class TryAgain : MonoBehaviour
{
    [Header("Glitch Effect Settings")]
    public float minGlitchInterval = 0.1f;
    public float maxGlitchInterval = 0.5f;
    public float glitchChance = 0.3f;

    [Header("Intensity Settings")]
    public float minIntensity = 0.3f;
    public float maxIntensity = 1f;
    public float intensityChangeSpeed = 2f;

    [Header("Scale Glitch Settings")]
    public float maxScaleGlitch = 0.05f; // Сделал очень маленьким
    public float scaleGlitchChance = 0.2f;

    [Header("Position Glitch Settings")]
    public float maxPositionGlitch = 1f; // Сделал очень маленьким
    public float positionGlitchChance = 0.15f;

    private CanvasGroup canvasGroup;
    private Vector3 originalScale;
    private Vector3 originalPosition;
    private Coroutine glitchCoroutine;
    private Coroutine intensityCoroutine;

    void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        originalScale = transform.localScale;
        originalPosition = transform.localPosition;

        // Запускаем эффекты
        intensityCoroutine = StartCoroutine(IntensityPulse());
        glitchCoroutine = StartCoroutine(GlitchEffect());
    }

    private IEnumerator IntensityPulse()
    {
        float timer = 0f;
        bool increasing = true;
        float currentIntensity = minIntensity;

        while (true)
        {
            if (increasing)
            {
                currentIntensity = Mathf.Lerp(minIntensity, maxIntensity, timer / intensityChangeSpeed);
                if (timer >= intensityChangeSpeed)
                {
                    increasing = false;
                    timer = 0f;
                }
            }
            else
            {
                currentIntensity = Mathf.Lerp(maxIntensity, minIntensity, timer / intensityChangeSpeed);
                if (timer >= intensityChangeSpeed)
                {
                    increasing = true;
                    timer = 0f;
                }
            }

            canvasGroup.alpha = currentIntensity;
            timer += Time.deltaTime;
            yield return null;
        }
    }

    private IEnumerator GlitchEffect()
    {
        while (true)
        {
            float glitchDelay = Random.Range(minGlitchInterval, maxGlitchInterval);
            yield return new WaitForSeconds(glitchDelay);

            if (Random.value <= glitchChance)
            {
                yield return StartCoroutine(ExecuteGlitch());
            }
        }
    }

    private IEnumerator ExecuteGlitch()
    {
        Vector3 startScale = transform.localScale;
        Vector3 startPosition = transform.localPosition;
        float startAlpha = canvasGroup.alpha;

        int glitchType = Random.Range(0, 3);

        switch (glitchType)
        {
            case 0: // Scale glitch - ОЧЕНЬ слабый
                if (Random.value <= scaleGlitchChance)
                {
                    Vector3 glitchScale = new Vector3(
                        originalScale.x + Random.Range(-maxScaleGlitch, maxScaleGlitch),
                        originalScale.y + Random.Range(-maxScaleGlitch, maxScaleGlitch),
                        originalScale.z // Z НЕ МЕНЯЕМ ВООБЩЕ!
                    );
                    transform.localScale = glitchScale;
                }
                break;

            case 1: // Position glitch - ОЧЕНЬ слабый и БЕЗ Z!
                if (Random.value <= positionGlitchChance)
                {
                    Vector3 glitchPosition = new Vector3(
                        originalPosition.x + Random.Range(-maxPositionGlitch, maxPositionGlitch),
                        originalPosition.y + Random.Range(-maxPositionGlitch, maxPositionGlitch),
                        originalPosition.z // Z НЕ МЕНЯЕМ ВООБЩЕ!
                    );
                    transform.localPosition = glitchPosition;
                }
                break;

            case 2: // Alpha glitch
                canvasGroup.alpha = Random.Range(0.1f, 0.5f);
                break;
        }

        float glitchDuration = Random.Range(0.05f, 0.15f);
        yield return new WaitForSeconds(glitchDuration);

        // Возвращаем к нормальным значениям
        transform.localScale = originalScale;
        transform.localPosition = originalPosition;
        canvasGroup.alpha = startAlpha;
    }

    public void StartEffects()
    {
        if (intensityCoroutine != null) StopCoroutine(intensityCoroutine);
        if (glitchCoroutine != null) StopCoroutine(glitchCoroutine);

        intensityCoroutine = StartCoroutine(IntensityPulse());
        glitchCoroutine = StartCoroutine(GlitchEffect());
    }

    public void StopEffects()
    {
        if (intensityCoroutine != null) StopCoroutine(intensityCoroutine);
        if (glitchCoroutine != null) StopCoroutine(glitchCoroutine);

        canvasGroup.alpha = 1f;
        transform.localScale = originalScale;
        transform.localPosition = originalPosition;
    }

    void OnDestroy()
    {
        if (intensityCoroutine != null) StopCoroutine(intensityCoroutine);
        if (glitchCoroutine != null) StopCoroutine(glitchCoroutine);
    }
}