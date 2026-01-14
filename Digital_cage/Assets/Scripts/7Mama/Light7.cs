using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Light7 : MonoBehaviour
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

        [HideInInspector]
        public float currentTime = 0f;
        [HideInInspector]
        public Coroutine blinkCoroutine;
        [HideInInspector]
        public float timeOffset = 0f;
    }

    [Header("Light Settings")]
    public List<BlinkingLight> lights = new List<BlinkingLight>(); // Список мигающих светов

    [Header("Global Settings")]
    public bool startOnAwake = true;      // Начать мигать при старте
    public bool randomizeAll = true;      // Случайное смещение всех огней

    private bool isActive = false;
    private Coroutine masterCoroutine;

    void Start()
    {
        InitializeLights();

        if (startOnAwake)
        {
            StartBlinking();
        }
    }

    void InitializeLights()
    {
        foreach (var lightData in lights)
        {
            if (lightData.lightSource == null)
            {
                Debug.LogWarning("Light7: Найден источник света без назначенного Light компонента!");
                continue;
            }

            // Сохраняем оригинальную интенсивность если maxIntensity не настроен
            if (lightData.maxIntensity <= 0)
            {
                lightData.maxIntensity = lightData.lightSource.intensity;
            }

            // Устанавливаем случайное смещение если нужно
            if (lightData.randomizeOffset || randomizeAll)
            {
                lightData.timeOffset = Random.Range(0f, lightData.blinkInterval);
            }

            // Устанавливаем начальную интенсивность
            lightData.lightSource.intensity = lightData.minIntensity;
        }

        Debug.Log($"Light7: Инициализировано {lights.Count} источников света");
    }

    public void StartBlinking()
    {
        if (isActive) return;

        isActive = true;
        Debug.Log("Light7: Начинаю мигание всех источников света");

        // Запускаем мигание для каждого источника
        foreach (var lightData in lights)
        {
            if (lightData.lightSource != null && lightData.blinkCoroutine == null)
            {
                lightData.blinkCoroutine = StartCoroutine(BlinkLightCoroutine(lightData));
            }
        }
    }

    public void StopBlinking()
    {
        if (!isActive) return;

        isActive = false;
        Debug.Log("Light7: Останавливаю мигание всех источников света");

        // Останавливаем все корутины
        foreach (var lightData in lights)
        {
            if (lightData.blinkCoroutine != null)
            {
                StopCoroutine(lightData.blinkCoroutine);
                lightData.blinkCoroutine = null;
            }

            // Возвращаем свет в минимальную интенсивность
            if (lightData.lightSource != null)
            {
                lightData.lightSource.intensity = lightData.minIntensity;
            }
        }

        if (masterCoroutine != null)
        {
            StopCoroutine(masterCoroutine);
            masterCoroutine = null;
        }
    }

    IEnumerator BlinkLightCoroutine(BlinkingLight lightData)
    {
        // Ждем смещение если есть
        if (lightData.timeOffset > 0)
        {
            yield return new WaitForSeconds(lightData.timeOffset);
        }

        while (isActive)
        {
            // Включаем свет
            yield return StartCoroutine(FadeLight(lightData, lightData.minIntensity, lightData.maxIntensity, lightData.blinkDuration / 2));

            // Ждем паузу на полной яркости
            yield return new WaitForSeconds(lightData.blinkDuration / 4);

            // Выключаем свет
            yield return StartCoroutine(FadeLight(lightData, lightData.maxIntensity, lightData.minIntensity, lightData.blinkDuration / 2));

            // Ждем до следующего мигания
            float waitTime = lightData.blinkInterval - lightData.blinkDuration;
            if (waitTime > 0)
            {
                yield return new WaitForSeconds(waitTime);
            }
            else
            {
                yield return null; // Если интервал меньше длительности, ждем кадр
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

    // Метод для ручного запуска мигания конкретного света
    public void StartLightBlinking(int lightIndex)
    {
        if (lightIndex < 0 || lightIndex >= lights.Count)
        {
            Debug.LogError($"Light7: Неверный индекс света: {lightIndex}");
            return;
        }

        var lightData = lights[lightIndex];

        if (lightData.lightSource == null)
        {
            Debug.LogError($"Light7: Свет с индексом {lightIndex} не назначен!");
            return;
        }

        if (lightData.blinkCoroutine != null)
        {
            StopCoroutine(lightData.blinkCoroutine);
        }

        lightData.blinkCoroutine = StartCoroutine(BlinkLightCoroutine(lightData));
        Debug.Log($"Light7: Запущено мигание света {lightIndex}");
    }

    // Метод для изменения параметров света во время работы
    public void UpdateLightSettings(int lightIndex, float newInterval, float newMinIntensity, float newMaxIntensity)
    {
        if (lightIndex < 0 || lightIndex >= lights.Count)
        {
            Debug.LogError($"Light7: Неверный индекс света: {lightIndex}");
            return;
        }

        var lightData = lights[lightIndex];
        lightData.blinkInterval = Mathf.Max(0.1f, newInterval);
        lightData.minIntensity = Mathf.Max(0f, newMinIntensity);
        lightData.maxIntensity = Mathf.Max(lightData.minIntensity, newMaxIntensity);

        Debug.Log($"Light7: Обновлены настройки света {lightIndex}");
    }

    // Тестовые методы
    [ContextMenu("Тест: Запустить все огни")]
    public void TestStartAll()
    {
        StartBlinking();
    }

    [ContextMenu("Тест: Остановить все огни")]
    public void TestStopAll()
    {
        StopBlinking();
    }

    [ContextMenu("Тест: Перезапустить все")]
    public void TestRestart()
    {
        StopBlinking();
        StartBlinking();
    }

    [ContextMenu("Тест: Увеличить скорость всех в 2 раза")]
    public void TestDoubleSpeed()
    {
        foreach (var lightData in lights)
        {
            lightData.blinkInterval /= 2f;
        }
        Debug.Log("Light7: Скорость всех огней увеличена в 2 раза");
    }

    [ContextMenu("Тест: Уменьшить скорость всех в 2 раза")]
    public void TestHalfSpeed()
    {
        foreach (var lightData in lights)
        {
            lightData.blinkInterval *= 2f;
        }
        Debug.Log("Light7: Скорость всех огней уменьшена в 2 раза");
    }

    [ContextMenu("Тест: Сделать все огни ярче")]
    public void TestBrighter()
    {
        foreach (var lightData in lights)
        {
            lightData.maxIntensity *= 1.5f;
        }
        Debug.Log("Light7: Все огни стали ярче");
    }

    [ContextMenu("Тест: Сделать все огни тусклее")]
    public void TestDimmer()
    {
        foreach (var lightData in lights)
        {
            lightData.maxIntensity *= 0.5f;
        }
        Debug.Log("Light7: Все огни стали тусклее");
    }

    [ContextMenu("Тест: Случайные параметры")]
    public void TestRandomizeAll()
    {
        foreach (var lightData in lights)
        {
            lightData.blinkInterval = Random.Range(0.3f, 3f);
            lightData.maxIntensity = Random.Range(0.5f, 3f);
            lightData.blinkDuration = Random.Range(0.1f, 0.5f);
        }
        Debug.Log("Light7: Все параметры рандомизированы");
    }

    [ContextMenu("Тест: Информация о всех огнях")]
    public void TestPrintInfo()
    {
        Debug.Log($"Light7: Всего источников: {lights.Count}");

        for (int i = 0; i < lights.Count; i++)
        {
            var lightData = lights[i];
            string lightName = lightData.lightSource != null ? lightData.lightSource.name : "NULL";
            Debug.Log($"Огонь {i}: {lightName}, Интервал: {lightData.blinkInterval}с, Яркость: {lightData.minIntensity}-{lightData.maxIntensity}");
        }
    }

    void OnDestroy()
    {
        StopBlinking();
    }

    void OnValidate()
    {
        // Проверка значений в редакторе
        foreach (var lightData in lights)
        {
            lightData.blinkInterval = Mathf.Max(0.1f, lightData.blinkInterval);
            lightData.blinkDuration = Mathf.Clamp(lightData.blinkDuration, 0.01f, lightData.blinkInterval);
            lightData.minIntensity = Mathf.Max(0f, lightData.minIntensity);
            lightData.maxIntensity = Mathf.Max(lightData.minIntensity, lightData.maxIntensity);
        }
    }

    void OnDrawGizmosSelected()
    {
        // Рисуем визуализацию для каждого источника света
        foreach (var lightData in lights)
        {
            if (lightData.lightSource != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(lightData.lightSource.transform.position, 0.5f);

                // Линия до родительского объекта
                if (lightData.lightSource.transform.parent != transform)
                {
                    Gizmos.DrawLine(transform.position, lightData.lightSource.transform.position);
                }

                // Текст с информацией
                GUIStyle style = new GUIStyle();
                style.normal.textColor = Color.yellow;
                style.alignment = TextAnchor.MiddleCenter;

#if UNITY_EDITOR
                string info = $"{lightData.blinkInterval:F1}с\n{lightData.minIntensity:F1}-{lightData.maxIntensity:F1}";
                UnityEditor.Handles.Label(lightData.lightSource.transform.position + Vector3.up * 0.7f, info, style);
#endif
            }
        }
    }
}