using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Light6 : MonoBehaviour
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
        public float timeOffset = 0f;
        [HideInInspector]
        public Coroutine blinkCoroutine;
    }

    [Header("Light Settings")]
    public List<BlinkingLight> lights = new List<BlinkingLight>();

    [Header("Activation Settings")]
    public bool waitForTodoUpdate = true;  // Ждать обновления Todo
    public float checkInterval = 0.5f;     // Интервал проверки Todo

    private bool isActive = false;
    private bool todoUpdated = false;
    private Coroutine checkTodoCoroutine;

    void Start()
    {
        InitializeLights();

        if (waitForTodoUpdate)
        {
            StartCheckingTodo();
        }
        else
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
                Debug.LogWarning("Light6: Найден источник света без назначенного Light компонента!");
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

            // Устанавливаем нормальную интенсивность (не мигаем пока не разрешено)
            lightData.lightSource.intensity = lightData.maxIntensity;
        }

        Debug.Log($"Light6: Инициализировано {lights.Count} источников света");
    }

    void StartCheckingTodo()
    {
        if (checkTodoCoroutine != null)
            StopCoroutine(checkTodoCoroutine);

        checkTodoCoroutine = StartCoroutine(CheckTodoRoutine());
    }

    IEnumerator CheckTodoRoutine()
    {
        Debug.Log("Light6: Начинаю проверку второго пункта Todo...");

        while (!todoUpdated)
        {
            TodoUI6 todoUI = FindObjectOfType<TodoUI6>();

            if (todoUI != null && todoUI.IsTask2Shown())
            {
                todoUpdated = true;
                Debug.Log("Light6: Второй пункт Todo появился! Запускаю мигание ламп...");
                StartBlinking();
                break;
            }

            yield return new WaitForSeconds(checkInterval);
        }

        checkTodoCoroutine = null;
    }

    public void StartBlinking()
    {
        if (isActive) return;

        isActive = true;
        Debug.Log("Light6: Начинаю мигание всех источников света");

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
        Debug.Log("Light6: Останавливаю мигание всех источников света");

        // Останавливаем все корутины
        foreach (var lightData in lights)
        {
            if (lightData.blinkCoroutine != null)
            {
                StopCoroutine(lightData.blinkCoroutine);
                lightData.blinkCoroutine = null;
            }

            // Возвращаем свет в нормальную интенсивность
            if (lightData.lightSource != null)
            {
                lightData.lightSource.intensity = lightData.maxIntensity;
            }
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
            // Включаем свет (плавно)
            yield return StartCoroutine(FadeLight(lightData, lightData.maxIntensity, lightData.minIntensity, lightData.blinkDuration / 2));

            // Ждем паузу на минимальной яркости
            yield return new WaitForSeconds(lightData.blinkDuration / 4);

            // Выключаем свет (плавно)
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
        if (!isActive)
            StartBlinking();
    }

    [ContextMenu("Тест: Остановить мигание")]
    public void TestStopBlinking()
    {
        StopBlinking();
    }

    [ContextMenu("Тест: Проверить Todo")]
    public void TestCheckTodo()
    {
        TodoUI6 todoUI = FindObjectOfType<TodoUI6>();
        if (todoUI != null)
        {
            Debug.Log($"Light6: Todo найден. IsTask2Shown = {todoUI.IsTask2Shown()}");
            if (todoUI.IsTask2Shown() && !isActive)
            {
                StartBlinking();
            }
        }
        else
        {
            Debug.LogWarning("Light6: TodoUI6 не найден!");
        }
    }

    [ContextMenu("Сбросить")]
    public void ResetLights()
    {
        if (checkTodoCoroutine != null)
        {
            StopCoroutine(checkTodoCoroutine);
            checkTodoCoroutine = null;
        }

        StopBlinking();

        isActive = false;
        todoUpdated = false;

        // Возвращаем все огни в нормальное состояние
        foreach (var lightData in lights)
        {
            if (lightData.lightSource != null)
            {
                lightData.lightSource.intensity = lightData.maxIntensity;
            }
        }

        // Снова начинаем проверять если нужно
        if (waitForTodoUpdate)
        {
            StartCheckingTodo();
        }

        Debug.Log("Light6: Сброшено");
    }

    void OnDestroy()
    {
        if (checkTodoCoroutine != null)
        {
            StopCoroutine(checkTodoCoroutine);
        }
        StopBlinking();
    }

    void OnValidate()
    {
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
        foreach (var lightData in lights)
        {
            if (lightData.lightSource != null)
            {
                Gizmos.color = isActive ? Color.yellow : Color.white;
                Gizmos.DrawWireSphere(lightData.lightSource.transform.position, 0.3f);

                GUIStyle style = new GUIStyle();
                style.normal.textColor = isActive ? Color.yellow : Color.gray;
                style.alignment = TextAnchor.MiddleCenter;

#if UNITY_EDITOR
                string state = isActive ? "МИГАЕТ" : (todoUpdated ? "ГОТОВ" : "ЖДЕТ TODO");
                UnityEditor.Handles.Label(lightData.lightSource.transform.position + Vector3.up * 0.5f, state, style);
#endif
            }
        }
    }
}