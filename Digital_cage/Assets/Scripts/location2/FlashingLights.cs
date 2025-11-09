using UnityEngine;
using System.Collections;

public class FlashingLights : MonoBehaviour
{
    [Header("Light Settings")]
    public Light[] lights;
    public float minIntensity = 0.05f; // Стало темнее
    public float maxIntensity = 0.8f;  // Стало темнее
    public float minFlashInterval = 0.05f;
    public float maxFlashInterval = 0.3f;

    [Header("Scene Lighting")]
    public bool changeAmbientLight = true;
    public Color blueAmbientColor = new Color(0.1f, 0.15f, 0.3f, 1f); // Стало намного темнее и синее
    public float ambientTransitionDuration = 2f;

    [Header("Light Dimming")]
    public bool dimLightsPermanently = true; // Постоянно уменьшить свет
    public float lightDimMultiplier = 0.3f; // Насколько уменьшить свет

    [Header("Timing")]
    public float startDelay = 0f;

    private float[] originalIntensities;
    private Color originalAmbientColor;
    private Coroutine flashingCoroutine;
    private Coroutine ambientCoroutine;
    private bool isFlashing = false;

    void Start()
    {
        SaveOriginalSettings();
    }

    private void SaveOriginalSettings()
    {
        // Сохраняем исходные интенсивности света
        if (lights != null && lights.Length > 0)
        {
            originalIntensities = new float[lights.Length];
            for (int i = 0; i < lights.Length; i++)
            {
                if (lights[i] != null)
                {
                    originalIntensities[i] = lights[i].intensity;
                }
            }
        }

        // Сохраняем исходный окружающий свет
        originalAmbientColor = RenderSettings.ambientLight;
    }

    public void StartFlashing()
    {
        if (isFlashing) return;

        isFlashing = true;
        Debug.Log("FlashingLights: Запуск мигания света");

        if (startDelay > 0)
        {
            flashingCoroutine = StartCoroutine(StartFlashingWithDelay());
        }
        else
        {
            StartFlashingImmediately();
        }
    }

    private IEnumerator StartFlashingWithDelay()
    {
        yield return new WaitForSeconds(startDelay);
        StartFlashingImmediately();
    }

    private void StartFlashingImmediately()
    {
        // Постоянно уменьшаем свет если включено
        if (dimLightsPermanently && lights != null)
        {
            DimLightsPermanently();
        }

        // Запускаем мигание света
        if (lights != null && lights.Length > 0)
        {
            flashingCoroutine = StartCoroutine(FlashingRoutine());
        }

        // Запускаем изменение окружающего света
        if (changeAmbientLight)
        {
            ambientCoroutine = StartCoroutine(AmbientLightRoutine());
        }
    }

    /// <summary>
    /// Постоянное уменьшение интенсивности света
    /// </summary>
    private void DimLightsPermanently()
    {
        for (int i = 0; i < lights.Length; i++)
        {
            if (lights[i] != null)
            {
                // Уменьшаем исходную интенсивность
                originalIntensities[i] *= lightDimMultiplier;
                lights[i].intensity = originalIntensities[i];
                Debug.Log($"FlashingLights: Свет #{i} уменьшен до {lights[i].intensity}");
            }
        }
    }

    private IEnumerator FlashingRoutine()
    {
        while (isFlashing)
        {
            for (int i = 0; i < lights.Length; i++)
            {
                if (lights[i] != null && lights[i].gameObject.activeInHierarchy)
                {
                    StartCoroutine(FlashSingleLight(lights[i]));
                }
            }
            yield return new WaitForSeconds(Random.Range(0.1f, 0.5f));
        }
    }

    private IEnumerator FlashSingleLight(Light light)
    {
        if (light == null) yield break;

        float flashDuration = Random.Range(minFlashInterval, maxFlashInterval);

        // Вспышка - временно увеличиваем яркость
        float originalIntensity = light.intensity;
        light.intensity = Random.Range(minIntensity, maxIntensity);

        yield return new WaitForSeconds(flashDuration);

        if (light != null)
        {
            // Возвращаем к уменьшенной интенсивности (не к нулю!)
            light.intensity = originalIntensity;
        }
    }

    /// <summary>
    /// Корутина изменения окружающего света
    /// </summary>
    private IEnumerator AmbientLightRoutine()
    {
        float timer = 0f;

        while (timer < ambientTransitionDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / ambientTransitionDuration;

            // Плавно меняем окружающий свет на темно-синий
            RenderSettings.ambientLight = Color.Lerp(originalAmbientColor, blueAmbientColor, progress);

            yield return null;
        }

        // Убеждаемся, что цвет установлен точно
        RenderSettings.ambientLight = blueAmbientColor;
        Debug.Log("FlashingLights: Окружающий свет стал темно-синим");
    }

    public void StopFlashing()
    {
        if (!isFlashing) return;

        isFlashing = false;
        Debug.Log("FlashingLights: Остановка мигания света");

        if (flashingCoroutine != null) StopCoroutine(flashingCoroutine);
        if (ambientCoroutine != null) StopCoroutine(ambientCoroutine);

        RestoreOriginalSettings();
    }

    private void RestoreOriginalSettings()
    {
        // Восстанавливаем интенсивности света (оригинальные, без уменьшения)
        if (lights != null && originalIntensities != null)
        {
            for (int i = 0; i < lights.Length; i++)
            {
                if (lights[i] != null)
                {
                    // Восстанавливаем оригинальную интенсивность
                    lights[i].intensity = originalIntensities[i] / lightDimMultiplier;
                }
            }
        }

        // Восстанавливаем исходный окружающий свет
        RenderSettings.ambientLight = originalAmbientColor;
        Debug.Log("FlashingLights: Освещение восстановлено");
    }

    [ContextMenu("Test Flashing")]
    public void TestFlashing()
    {
        StartFlashing();
    }

    [ContextMenu("Stop Flashing")]
    public void TestStopFlashing()
    {
        StopFlashing();
    }

    void OnDestroy()
    {
        if (isFlashing) StopFlashing();
    }
}