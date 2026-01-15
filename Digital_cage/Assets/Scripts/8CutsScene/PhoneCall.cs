using UnityEngine;
using System.Collections;

public class PhoneCall : MonoBehaviour
{
    [Header("Телефон")]
    public Transform phoneTransform;    // Трансформ телефона
    public Vector3 vibrationIntensity = new Vector3(0.05f, 0.02f, 0f); // Сила вибрации
    [Range(1f, 100f)]
    public float vibrationSpeed = 30f;  // Скорость вибрации

    [Header("Режим вибрации с паузами")]
    public bool useVibrationPattern = true; // Использовать паттерн вибрации
    public float vibrateDuration = 4f;  // Длительность вибрации
    public float pauseDuration = 1f;    // Длительность паузы

    [Header("Звук")]
    public AudioSource audioSource;     // Аудиоисточник
    public AudioClip ringtone;          // Звонок
    public bool loopRingtone = true;    // Зациклить звонок
    public bool pauseSoundWithVibration = false; // Пауза звука вместе с вибрацией

    [Header("Диалог")]
    public ManagerDialogue8 dialogueManager; // Диалог менеджер 8
    public int stopAtLine = 5;          // На какой реплике остановить звонок

    [Header("Настройки")]
    public bool startOnAwake = true;    // Начать сразу при старте
    public float startDelay = 0.5f;     // Задержка перед началом

    private Vector3 originalPosition;   // Исходная позиция телефона
    private bool isRinging = false;     // Флаг состояния
    private bool subscribedToDialogue = false;
    private Coroutine vibrationPatternCoroutine;
    private float patternTimer = 0f;
    private bool isVibrationPhase = true;

    void Start()
    {
        InitializeComponents();

        if (startOnAwake)
        {
            StartCoroutine(StartPhoneCallWithDelay());
        }
    }

    void InitializeComponents()
    {
        // Сохраняем исходную позицию
        if (phoneTransform != null)
        {
            originalPosition = phoneTransform.localPosition;
        }
        else
        {
            Debug.LogError("PhoneCall: Не назначен phoneTransform!");
        }

        // Создаем AudioSource если нет
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }

        // Настраиваем аудио
        if (audioSource != null)
        {
            audioSource.playOnAwake = false;
            audioSource.loop = loopRingtone;
        }

        // Находим диалог менеджер если не назначен
        if (dialogueManager == null)
        {
            dialogueManager = FindObjectOfType<ManagerDialogue8>();
        }
    }

    IEnumerator StartPhoneCallWithDelay()
    {
        yield return new WaitForSeconds(startDelay);
        StartPhoneCall();
    }

    /// <summary>
    /// Начать звонок
    /// </summary>
    public void StartPhoneCall()
    {
        if (isRinging) return;

        isRinging = true;
        isVibrationPhase = true;
        patternTimer = 0f;

        Debug.Log($"PhoneCall: Начинаю звонок (вибрация: {vibrateDuration}с, пауза: {pauseDuration}с)");

        // Подписываемся на события диалога
        SubscribeToDialogue();

        // Запускаем звонок
        if (audioSource != null && ringtone != null)
        {
            audioSource.clip = ringtone;
            audioSource.loop = loopRingtone;
            audioSource.Play();
            Debug.Log("PhoneCall: Звонок начался");
        }

        // Запускаем вибрацию
        if (useVibrationPattern)
        {
            vibrationPatternCoroutine = StartCoroutine(VibrationPatternRoutine());
        }
        else
        {
            StartCoroutine(ContinuousVibrationRoutine());
        }
    }

    /// <summary>
    /// Остановить звонок
    /// </summary>
    public void StopPhoneCall()
    {
        if (!isRinging) return;

        isRinging = false;
        Debug.Log("PhoneCall: Останавливаю звонок...");

        // Останавливаем корутину паттерна
        if (vibrationPatternCoroutine != null)
        {
            StopCoroutine(vibrationPatternCoroutine);
            vibrationPatternCoroutine = null;
        }

        // Отписываемся от событий
        UnsubscribeFromDialogue();

        // Останавливаем звук
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }

        // Возвращаем телефон в исходное положение
        if (phoneTransform != null)
        {
            phoneTransform.localPosition = originalPosition;
        }

        Debug.Log("PhoneCall: Звонок остановлен");
    }

    // Паттерн вибрации: 4 сек вибрация, 1 сек пауза (ПРОСТОЙ И РАБОЧИЙ ВАРИАНТ)
    IEnumerator VibrationPatternRoutine()
    {
        while (isRinging)
        {
            // ФАЗА 1: ВИБРАЦИЯ
            isVibrationPhase = true;
            Debug.Log($"PhoneCall: === ВИБРАЦИЯ {vibrateDuration} секунд ===");

            float vibrateTimer = 0f;
            while (vibrateTimer < vibrateDuration && isRinging)
            {
                // Применяем вибрацию
                ApplyVibration();
                vibrateTimer += Time.deltaTime;
                yield return null;
            }

            // Возвращаем в исходное положение перед паузой
            if (phoneTransform != null)
            {
                phoneTransform.localPosition = originalPosition;
            }

            // Если нужно - приостанавливаем звук
            if (pauseSoundWithVibration && audioSource != null && audioSource.isPlaying)
            {
                audioSource.Pause();
                Debug.Log("PhoneCall: Звук на паузе");
            }

            // ФАЗА 2: ПАУЗА
            isVibrationPhase = false;
            Debug.Log($"PhoneCall: === ПАУЗА {pauseDuration} секунд ===");

            float pauseTimer = 0f;
            while (pauseTimer < pauseDuration && isRinging)
            {
                // Держим телефон неподвижно
                if (phoneTransform != null)
                {
                    phoneTransform.localPosition = originalPosition;
                }
                pauseTimer += Time.deltaTime;
                yield return null;
            }

            // Возобновляем звук если был на паузе
            if (pauseSoundWithVibration && audioSource != null)
            {
                audioSource.UnPause();
                Debug.Log("PhoneCall: Звук возобновлен");
            }
        }
    }

    // Непрерывная вибрация (старый режим)
    IEnumerator ContinuousVibrationRoutine()
    {
        while (isRinging)
        {
            ApplyVibration();
            yield return null;
        }

        // Возвращаем в исходное положение
        if (phoneTransform != null)
        {
            phoneTransform.localPosition = originalPosition;
        }
    }

    // Метод применения вибрации
    void ApplyVibration()
    {
        if (phoneTransform == null) return;

        // Создаем вибрацию
        float x = Mathf.Sin(Time.time * vibrationSpeed) * vibrationIntensity.x;
        float y = Mathf.Cos(Time.time * vibrationSpeed * 0.7f) * vibrationIntensity.y;
        float z = Mathf.Sin(Time.time * vibrationSpeed * 0.5f) * vibrationIntensity.z;

        // Применяем вибрацию к телефону
        phoneTransform.localPosition = originalPosition + new Vector3(x, y, z);
    }

    void SubscribeToDialogue()
    {
        if (dialogueManager != null && !subscribedToDialogue)
        {
            dialogueManager.OnDialogueIndexReached += OnDialogueLineChanged;
            subscribedToDialogue = true;
            Debug.Log("PhoneCall: Подписался на события диалога");
        }
    }

    void UnsubscribeFromDialogue()
    {
        if (dialogueManager != null && subscribedToDialogue)
        {
            dialogueManager.OnDialogueIndexReached -= OnDialogueLineChanged;
            subscribedToDialogue = false;
            Debug.Log("PhoneCall: Отписался от событий диалога");
        }
    }

    void OnDialogueLineChanged(int lineIndex)
    {
        Debug.Log($"PhoneCall: Реплика {lineIndex} (останавливаю на {stopAtLine})");

        if (lineIndex >= stopAtLine)
        {
            StopPhoneCall();
        }
    }

    void Update()
    {
        // Обновляем таймер для отладки
        if (isRinging && useVibrationPattern)
        {
            patternTimer += Time.deltaTime;
        }
    }

    void OnDestroy()
    {
        UnsubscribeFromDialogue();

        // Останавливаем все корутины
        if (vibrationPatternCoroutine != null)
        {
            StopCoroutine(vibrationPatternCoroutine);
        }
    }

    [ContextMenu("Тест: Начать звонок")]
    public void TestStartCall()
    {
        StartPhoneCall();
    }

    [ContextMenu("Тест: Остановить звонок")]
    public void TestStopCall()
    {
        StopPhoneCall();
    }

    void OnValidate()
    {
        // Предупреждения
        if (ringtone == null)
        {
            Debug.LogWarning("PhoneCall: Не назначен ringtone!");
        }

        if (dialogueManager == null)
        {
            Debug.LogWarning("PhoneCall: Не назначен dialogueManager!");
        }
    }

    void OnDrawGizmosSelected()
    {
        // Визуализация области вибрации
        if (phoneTransform != null)
        {
            Gizmos.color = isVibrationPhase ? Color.yellow : Color.gray;

            // Показываем диапазон вибрации
            Gizmos.DrawWireCube(
                phoneTransform.position,
                vibrationIntensity * 2
            );

            Gizmos.DrawWireSphere(phoneTransform.position, 0.1f);

#if UNITY_EDITOR
            string status = isRinging ? 
                (isVibrationPhase ? "?? ВИБРАЦИЯ" : "?? ПАУЗА") : 
                "?? Телефон";
            
            string pattern = useVibrationPattern ? 
                $"{vibrateDuration}с/{pauseDuration}с" : 
                "БЕЗ ПАУЗ";
            
            string timerInfo = isRinging && useVibrationPattern ? 
                $"\nТаймер: {patternTimer:F1}с" : "";
            
            string info = $"{status}\nПаттерн: {pattern}{timerInfo}";
            UnityEditor.Handles.Label(phoneTransform.position + Vector3.up * 0.5f, info);
#endif
        }
    }
}