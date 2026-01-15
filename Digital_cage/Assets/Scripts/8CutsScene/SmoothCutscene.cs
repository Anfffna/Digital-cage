using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class SmoothCutscene : MonoBehaviour
{
    [Header("Позиции камеры")]
    public Transform phonePosition;          // 1. План на телефон
    public Transform monitorPosition;        // 2. Общий план монитора
    public Transform monitorZoomPosition;    // 3. Крупный зум на монитор

    [Header("Настройки движения")]
    public float moveToPhoneDuration = 2f;     // К телефону
    public float moveToMonitorDuration = 3f;   // К общему плану монитора
    public float moveToZoomDuration = 2f;      // К зуму на монитор
    public float phoneHoldDuration = 1f;       // Задержка у телефона

    [Header("Черный экран")]
    public Image blackScreen;                // UI Image для черного экрана
    public float blackScreenDelay = 2f;      // Задержка после диалога перед появлением черного
    public float blackScreenFadeInTime = 2f; // Время появления черного экрана
    public float blackScreenHoldTime = 3f;   // Время удержания черного экрана
    public float blackScreenFadeOutTime = 2f; // Время исчезновения черного экрана

    [Header("Диалог")]
    public ManagerDialogue8 dialogueManager;
    [TextArea(2, 5)]
    public List<string> dialogueLines;
    public int moveToMonitorLine = 5;      // На какой реплике двигаться к общему плану монитора
    public int zoomToMonitorLine = 8;      // На какой реплике зумировать на монитор

    private Camera mainCamera;
    private bool isMoving = false;
    private System.Action<int> onDialogueLineChanged;

    void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("SmoothCutscene: Не найдена основная камера!");
            return;
        }

        // Инициализируем черный экран
        InitializeBlackScreen();

        StartCoroutine(CutsceneSequence());
    }

    void InitializeBlackScreen()
    {
        if (blackScreen != null)
        {
            // Скрываем черный экран в начале
            blackScreen.gameObject.SetActive(true);
            Color color = blackScreen.color;
            color.a = 0f;
            color.r = color.g = color.b = 0f; // Чисто черный
            blackScreen.color = color;
            Debug.Log("SmoothCutscene: Черный экран инициализирован (прозрачный)");
        }
        else
        {
            Debug.LogWarning("SmoothCutscene: blackScreen не назначен!");
        }
    }

    IEnumerator CutsceneSequence()
    {
        Debug.Log("SmoothCutscene: Начинаю трёхточечную катсцену...");

        // 1. Начинаем с текущей позиции камеры
        Vector3 startPos = mainCamera.transform.position;
        Quaternion startRot = mainCamera.transform.rotation;

        // 2. Плавно двигаемся к телефону
        Debug.Log("SmoothCutscene: Двигаюсь к телефону...");
        yield return StartCoroutine(MoveCameraTo(
            mainCamera.transform,
            phonePosition.position,
            phonePosition.rotation,
            moveToPhoneDuration
        ));

        // 3. Ждем немного
        yield return new WaitForSeconds(phoneHoldDuration);

        // 4. Запускаем диалог
        if (dialogueManager != null && dialogueLines.Count > 0)
        {
            onDialogueLineChanged = (lineIndex) =>
            {
                Debug.Log($"SmoothCutscene: Реплика {lineIndex}");

                if (lineIndex == moveToMonitorLine && !isMoving)
                {
                    StartCoroutine(MoveToMonitor());
                }
                else if (lineIndex == zoomToMonitorLine && !isMoving)
                {
                    StartCoroutine(ZoomToMonitor());
                }
            };

            dialogueManager.OnDialogueIndexReached += onDialogueLineChanged;
            dialogueManager.StartDialogue(dialogueLines, OnDialogueEnd);
        }
        else
        {
            Debug.LogError("SmoothCutscene: Нет диалог-менеджера или реплик!");
        }
    }

    IEnumerator MoveToMonitor()
    {
        isMoving = true;
        Debug.Log($"SmoothCutscene: Начинаю движение к общему плану монитора (реплика {moveToMonitorLine})...");

        yield return StartCoroutine(MoveCameraTo(
            mainCamera.transform,
            monitorPosition.position,
            monitorPosition.rotation,
            moveToMonitorDuration
        ));

        isMoving = false;
        Debug.Log("SmoothCutscene: Движение к монитору завершено");
    }

    IEnumerator ZoomToMonitor()
    {
        isMoving = true;
        Debug.Log($"SmoothCutscene: Начинаю зум на монитор (реплика {zoomToMonitorLine})...");

        yield return StartCoroutine(MoveCameraTo(
            mainCamera.transform,
            monitorZoomPosition.position,
            monitorZoomPosition.rotation,
            moveToZoomDuration
        ));

        isMoving = false;
        Debug.Log("SmoothCutscene: Зум на монитор завершен");
    }

    IEnumerator MoveCameraTo(Transform cameraTransform, Vector3 targetPosition, Quaternion targetRotation, float duration)
    {
        Vector3 startPosition = cameraTransform.position;
        Quaternion startRotation = cameraTransform.rotation;

        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float progress = timer / duration;

            // Плавная интерполяция (smoothstep для кинематографичного движения)
            float smoothProgress = Mathf.SmoothStep(0f, 1f, progress);

            cameraTransform.position = Vector3.Lerp(startPosition, targetPosition, smoothProgress);
            cameraTransform.rotation = Quaternion.Slerp(startRotation, targetRotation, smoothProgress);

            yield return null;
        }

        // Гарантируем точное попадание в конечную точку
        cameraTransform.position = targetPosition;
        cameraTransform.rotation = targetRotation;
    }

    void OnDialogueEnd()
    {
        Debug.Log("SmoothCutscene: Диалог завершен!");

        // Отписываемся от событий
        if (dialogueManager != null && onDialogueLineChanged != null)
        {
            dialogueManager.OnDialogueIndexReached -= onDialogueLineChanged;
        }

        // Запускаем последовательность с черным экраном
        StartCoroutine(BlackScreenSequence());
    }

    IEnumerator BlackScreenSequence()
    {
        Debug.Log($"SmoothCutscene: Жду {blackScreenDelay} секунд перед черным экраном...");
        yield return new WaitForSeconds(blackScreenDelay);

        // 1. Плавное появление черного экрана
        Debug.Log($"SmoothCutscene: Плавное появление черного экрана ({blackScreenFadeInTime} сек)...");
        yield return StartCoroutine(FadeBlackScreen(0f, 1f, blackScreenFadeInTime));

        // 2. Удерживаем черный экран - В ЭТОТ МОМЕНТ ТЕЛЕПОРТИРУЕМ КАМЕРУ
        Debug.Log($"SmoothCutscene: Удерживаю черный экран ({blackScreenHoldTime} сек)...");

        // ТЕЛЕПОРТИРУЕМ КАМЕРУ СРАЗУ ЖЕ (когда экран полностью черный)
        TeleportCameraToTitles();

        yield return new WaitForSeconds(blackScreenHoldTime);

        // 3. Плавное исчезновение черного экрана
        Debug.Log($"SmoothCutscene: Плавное исчезновение черного экрана ({blackScreenFadeOutTime} сек)...");
        yield return StartCoroutine(FadeBlackScreen(1f, 0f, blackScreenFadeOutTime));

        Debug.Log("SmoothCutscene: Катсцена полностью завершена!");
    }

    // НОВЫЙ метод для телепортации камеры
    void TeleportCameraToTitles()
    {
        // Находим Titles в сцене
        Titles titles = FindObjectOfType<Titles>();
        if (titles != null && titles.doorPosition != null)
        {
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                // Мгновенная телепортация камеры к первой позиции Titles
                mainCamera.transform.position = titles.doorPosition.position;
                mainCamera.transform.rotation = titles.doorPosition.rotation;
                Debug.Log("SmoothCutscene: Камера телепортирована к doorPosition Titles");

                // Запускаем Titles сразу (но они начнут движение с задержкой)
                titles.StartTitles();
            }
        }
        else
        {
            Debug.LogWarning("SmoothCutscene: Titles или doorPosition не найдены!");
        }
    }

    IEnumerator FadeBlackScreen(float fromAlpha, float toAlpha, float duration)
    {
        if (blackScreen == null) yield break;

        float timer = 0f;
        Color color = blackScreen.color;
        color.a = fromAlpha;
        blackScreen.color = color;

        // Убеждаемся что черный экран активен
        blackScreen.gameObject.SetActive(true);

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float progress = timer / duration;
            float smoothProgress = Mathf.SmoothStep(0f, 1f, progress);

            color.a = Mathf.Lerp(fromAlpha, toAlpha, smoothProgress);
            blackScreen.color = color;
            yield return null;
        }

        color.a = toAlpha;
        blackScreen.color = color;

        // Если полностью прозрачный - можно скрыть
        if (toAlpha <= 0.01f)
        {
            blackScreen.gameObject.SetActive(false);
        }
    }

    [ContextMenu("Тест: К телефону")]
    public void TestMoveToPhone()
    {
        if (mainCamera == null) mainCamera = Camera.main;
        StartCoroutine(MoveCameraTo(mainCamera.transform, phonePosition.position, phonePosition.rotation, 2f));
    }

    [ContextMenu("Тест: К монитору")]
    public void TestMoveToMonitor()
    {
        if (mainCamera == null) mainCamera = Camera.main;
        StartCoroutine(MoveCameraTo(mainCamera.transform, monitorPosition.position, monitorPosition.rotation, 2f));
    }

    [ContextMenu("Тест: Зум на монитор")]
    public void TestZoomToMonitor()
    {
        if (mainCamera == null) mainCamera = Camera.main;
        StartCoroutine(MoveCameraTo(mainCamera.transform, monitorZoomPosition.position, monitorZoomPosition.rotation, 2f));
    }

    [ContextMenu("Тест: Черный экран")]
    public void TestBlackScreen()
    {
        StartCoroutine(BlackScreenSequence());
    }

    [ContextMenu("Тест: Показать черный")]
    public void TestShowBlack()
    {
        if (blackScreen != null)
        {
            Color color = blackScreen.color;
            color.a = 1f;
            blackScreen.color = color;
            blackScreen.gameObject.SetActive(true);
        }
    }

    [ContextMenu("Тест: Скрыть черный")]
    public void TestHideBlack()
    {
        if (blackScreen != null)
        {
            Color color = blackScreen.color;
            color.a = 0f;
            blackScreen.color = color;
            blackScreen.gameObject.SetActive(false);
        }
    }

    void OnDrawGizmos()
    {
        // Визуализация позиций камер в редакторе
        Gizmos.color = Color.green;
        DrawCameraGizmo(phonePosition, "Телефон");

        Gizmos.color = Color.blue;
        DrawCameraGizmo(monitorPosition, "Монитор");

        Gizmos.color = Color.red;
        DrawCameraGizmo(monitorZoomPosition, "Зум");

        // Линии между точками
        if (phonePosition != null && monitorPosition != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(phonePosition.position, monitorPosition.position);
        }

        if (monitorPosition != null && monitorZoomPosition != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(monitorPosition.position, monitorZoomPosition.position);
        }
    }

    void DrawCameraGizmo(Transform point, string label)
    {
        if (point != null)
        {
            Gizmos.DrawWireSphere(point.position, 0.3f);
            Gizmos.DrawLine(point.position, point.position + point.forward * 1f);

#if UNITY_EDITOR
            UnityEditor.Handles.Label(point.position + Vector3.up * 0.5f, label);
#endif
        }
    }
}