using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Light8 : MonoBehaviour
{
    [System.Serializable]
    public class BlinkingLight
    {
        public Light lightSource;          // Источник света
        public float blinkInterval = 1f;   // Интервал мигания (секунды)
        public float minIntensity = 0f;    // Минимальная интенсивность
        public float maxIntensity = 1f;    // Максимальная интенсивность
        public float blinkDuration = 0.2f; // Длительность мигания
        public bool randomizeOffset = true; // Случайное смещение начала
        public bool startOnAwake = true;   // Начинать сразу при старте

        [HideInInspector]
        public float timeOffset = 0f;
        [HideInInspector]
        public Coroutine blinkCoroutine;
    }

    [Header("Light Settings")]
    public List<BlinkingLight> lights = new List<BlinkingLight>();

    [Header("Global Control")]
    public bool autoStart = true;          // Автоматически начать при старте
    public float startDelay = 0f;          // Задержка перед стартом
    public bool neverStop = true;          // НИКОГДА не останавливать мигание

    private bool isActive = false;
    private Coroutine startDelayCoroutine;

    void Start()
    {
        InitializeLights();

        if (autoStart)
        {
            if (startDelay > 0)
            {
                startDelayCoroutine = StartCoroutine(StartWithDelay());
            }
            else
            {
                StartBlinking();
            }
        }
    }

    void InitializeLights()
    {
        foreach (var lightData in lights)
        {
            if (lightData.lightSource == null)
            {
                Debug.LogWarning("Light8: Найден источник света без назначенного Light компонента!");
                continue;
            }

            // Сохраняем оригинальную интенсивность
            if (lightData.maxIntensity <= 0)
            {
                lightData.maxIntensity = lightData.lightSource.intensity;
            }

            // Устанавливаем случайное смещение если нужно
            if (lightData.randomizeOffset)
            {
                lightData.timeOffset = Random.Range(0f, lightData.blinkInterval);
            }

            // Устанавливаем начальную интенсивность
            lightData.lightSource.intensity = lightData.maxIntensity;
        }

        Debug.Log($"Light8: Инициализировано {lights.Count} источников света");
    }

    IEnumerator StartWithDelay()
    {
        Debug.Log($"Light8: Жду {startDelay} секунд перед началом мигания...");
        yield return new WaitForSeconds(startDelay);
        StartBlinking();
    }

    /// <summary>
    /// Начать мигание всех источников света (НАВСЕГДА)
    /// </summary>
    public void StartBlinking()
    {
        if (isActive) return;

        isActive = true;
        Debug.Log("Light8: Начинаю мигание всех источников света (никогда не остановится)");

        // Запускаем мигание для каждого источника
        foreach (var lightData in lights)
        {
            if (lightData.lightSource != null && lightData.blinkCoroutine == null && lightData.startOnAwake)
            {
                lightData.blinkCoroutine = StartCoroutine(BlinkLightCoroutine(lightData));
            }
        }
    }

    /// <summary>
    /// Остановить мигание всех источников света
    /// ВАЖНО: если neverStop = true, этот метод НЕ БУДЕТ РАБОТАТЬ!
    /// </summary>
    public void StopBlinking()
    {
        if (!isActive || neverStop)
        {
            Debug.LogWarning("Light8: Мигание НЕ остановлено (neverStop = true)");
            return;
        }

        isActive = false;
        Debug.Log("Light8: Останавливаю мигание всех источников света");

        // Останавливаем все корутины
        foreach (var lightData in lights)
        {
            if (lightData.blinkCoroutine != null)
            {
                StopCoroutine(lightData.blinkCoroutine);
                lightData.blinkCoroutine = null;
            }

            // Возвращаем свет в максимальную интенсивность
            if (lightData.lightSource != null)
            {
                lightData.lightSource.intensity = lightData.maxIntensity;
            }
        }
    }

    /// <summary>
    /// Включить/выключить конкретный источник света
    /// </summary>
    public void ToggleLight(int index, bool enable)
    {
        if (index < 0 || index >= lights.Count)
        {
            Debug.LogError($"Light8: Неверный индекс света: {index}");
            return;
        }

        var lightData = lights[index];

        if (enable && lightData.blinkCoroutine == null)
        {
            lightData.blinkCoroutine = StartCoroutine(BlinkLightCoroutine(lightData));
        }
        else if (!enable && lightData.blinkCoroutine != null && !neverStop)
        {
            StopCoroutine(lightData.blinkCoroutine);
            lightData.blinkCoroutine = null;
            lightData.lightSource.intensity = lightData.maxIntensity;
        }
        else if (!enable && neverStop)
        {
            Debug.LogWarning("Light8: Не могу остановить свет (neverStop = true)");
        }
    }

    IEnumerator BlinkLightCoroutine(BlinkingLight lightData)
    {
        // Ждем смещение если есть
        if (lightData.timeOffset > 0)
        {
            yield return new WaitForSeconds(lightData.timeOffset);
        }

        // ВЕЧНЫЙ цикл
        while (true)
        {
            // Плавно выключаем свет
            yield return StartCoroutine(FadeLight(lightData, lightData.maxIntensity, lightData.minIntensity, lightData.blinkDuration / 2));

            // Ждем паузу на минимальной яркости
            if (lightData.blinkDuration / 4 > 0)
            {
                yield return new WaitForSeconds(lightData.blinkDuration / 4);
            }

            // Плавно включаем свет
            yield return StartCoroutine(FadeLight(lightData, lightData.minIntensity, lightData.maxIntensity, lightData.blinkDuration / 2));

            // Ждем до следующего мигания
            float waitTime = lightData.blinkInterval - lightData.blinkDuration;
            if (waitTime > 0)
            {
                yield return new WaitForSeconds(waitTime);
            }
            else
            {
                yield return null;
            }
        }
    }

    IEnumerator FadeLight(BlinkingLight lightData, float fromIntensity, float toIntensity, float duration)
    {
        if (duration <= 0 || lightData.lightSource == null) yield break;

        float timer = 0f;
        float startIntensity = lightData.lightSource.intensity;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float progress = timer / duration;
            lightData.lightSource.intensity = Mathf.Lerp(startIntensity, toIntensity, progress);
            yield return null;
        }

        lightData.lightSource.intensity = toIntensity;
    }

    [ContextMenu("Тест: Запустить мигание")]
    public void TestStartBlinking()
    {
        StartBlinking();
    }

    [ContextMenu("Тест: Попытаться остановить")]
    public void TestTryStopBlinking()
    {
        StopBlinking();
    }

    [ContextMenu("Тест: Случайно изменить интенсивность")]
    public void TestRandomizeIntensity()
    {
        foreach (var lightData in lights)
        {
            if (lightData.lightSource != null)
            {
                lightData.minIntensity = Random.Range(0f, 0.5f);
                lightData.maxIntensity = Random.Range(0.5f, 1.5f);
                Debug.Log($"Light {lightData.lightSource.name}: min={lightData.minIntensity:F2}, max={lightData.maxIntensity:F2}");
            }
        }
    }

    [ContextMenu("Сбросить (с сохранением мигания)")]
    public void ResetLights()
    {
        if (startDelayCoroutine != null)
        {
            StopCoroutine(startDelayCoroutine);
            startDelayCoroutine = null;
        }

        // Если neverStop = true, НЕ останавливаем мигание
        if (!neverStop)
        {
            StopBlinking();
        }

        isActive = false;

        // Возвращаем все огни в нормальное состояние, но они продолжат мигать
        foreach (var lightData in lights)
        {
            if (lightData.lightSource != null)
            {
                lightData.lightSource.intensity = lightData.maxIntensity;
            }
        }

        Debug.Log("Light8: Сброшено (мигание продолжается)");

        // Снова начинаем если autoStart = true
        if (autoStart)
        {
            if (startDelay > 0)
            {
                startDelayCoroutine = StartCoroutine(StartWithDelay());
            }
            else if (!isActive)
            {
                StartBlinking();
            }
        }
    }

    void OnDestroy()
    {
        if (startDelayCoroutine != null)
        {
            StopCoroutine(startDelayCoroutine);
        }

        // Даже при уничтожении объекта, мигание остановится автоматически
        // т.к. корутины привязаны к GameObject
    }

    void OnValidate()
    {
        // Автопроверка настроек
        foreach (var lightData in lights)
        {
            if (lightData.lightSource == null) continue;

            lightData.blinkInterval = Mathf.Max(0.1f, lightData.blinkInterval);
            lightData.blinkDuration = Mathf.Clamp(lightData.blinkDuration, 0.01f, lightData.blinkInterval);
            lightData.minIntensity = Mathf.Max(0f, lightData.minIntensity);
            lightData.maxIntensity = Mathf.Max(lightData.minIntensity, lightData.maxIntensity);
        }
    }

    void OnDrawGizmosSelected()
    {
        foreach (var lightData in lights)
        {
            if (lightData.lightSource != null)
            {
                Gizmos.color = isActive ? Color.yellow : Color.white;
                Gizmos.DrawWireSphere(lightData.lightSource.transform.position, 0.5f);

                // Показываем направление света
                if (lightData.lightSource.type == LightType.Spot || lightData.lightSource.type == LightType.Directional)
                {
                    Gizmos.DrawRay(lightData.lightSource.transform.position, lightData.lightSource.transform.forward * 1f);
                }

                GUIStyle style = new GUIStyle();
                style.normal.textColor = isActive ? Color.yellow : Color.gray;
                style.alignment = TextAnchor.MiddleCenter;

#if UNITY_EDITOR
                string stopInfo = neverStop ? "?? ВЫКЛ" : "";
                string info = $"{(isActive ? "?" : "??")}\n{lightData.blinkInterval:F1}s\n{stopInfo}";
                UnityEditor.Handles.Label(lightData.lightSource.transform.position + Vector3.up * 0.8f, info, style);
#endif
            }
        }
    }
}