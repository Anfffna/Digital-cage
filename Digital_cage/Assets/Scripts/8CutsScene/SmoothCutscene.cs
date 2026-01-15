using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ThreePointCutscene : MonoBehaviour
{
    [Header("Позиции камеры")]
    public Transform phonePosition;          // 1. План на телефон
    public Transform monitorPosition;        // 2. Общий план монитора
    public Transform monitorZoomPosition;    // 3. Крупный зум на монитор (НОВАЯ!)

    [Header("Настройки движения")]
    public float moveToPhoneDuration = 2f;     // К телефону
    public float moveToMonitorDuration = 3f;   // К общему плану монитора
    public float moveToZoomDuration = 2f;      // К зуму на монитор
    public float phoneHoldDuration = 1f;       // Задержка у телефона

    [Header("Диалог")]
    public ManagerDialogue8 dialogueManager;
    [TextArea(2, 5)]
    public List<string> dialogueLines;
    public int moveToMonitorLine = 5;      // На какой реплике двигаться к общему плану монитора
    public int zoomToMonitorLine = 8;      // На какой реплике зумировать на монитор (НОВОЕ!)

    private Camera mainCamera;
    private bool isMoving = false;
    private System.Action<int> onDialogueLineChanged;

    void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("Cutscene: Не найдена основная камера!");
            return;
        }

        StartCoroutine(CutsceneSequence());
    }

    IEnumerator CutsceneSequence()
    {
        Debug.Log("Cutscene: Начинаю трёхточечную катсцену...");

        // 1. Начинаем с текущей позиции камеры
        Vector3 startPos = mainCamera.transform.position;
        Quaternion startRot = mainCamera.transform.rotation;

        // 2. Плавно двигаемся к телефону
        Debug.Log("Cutscene: Двигаюсь к телефону...");
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
                Debug.Log($"Cutscene: Реплика {lineIndex}");

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
            Debug.LogError("Cutscene: Нет диалог-менеджера или реплик!");
        }
    }

    IEnumerator MoveToMonitor()
    {
        isMoving = true;
        Debug.Log($"Cutscene: Начинаю движение к общему плану монитора (реплика {moveToMonitorLine})...");

        yield return StartCoroutine(MoveCameraTo(
            mainCamera.transform,
            monitorPosition.position,
            monitorPosition.rotation,
            moveToMonitorDuration
        ));

        isMoving = false;
        Debug.Log("Cutscene: Движение к монитору завершено");
    }

    IEnumerator ZoomToMonitor()
    {
        isMoving = true;
        Debug.Log($"Cutscene: Начинаю зум на монитор (реплика {zoomToMonitorLine})...");

        yield return StartCoroutine(MoveCameraTo(
            mainCamera.transform,
            monitorZoomPosition.position,
            monitorZoomPosition.rotation,
            moveToZoomDuration
        ));

        isMoving = false;
        Debug.Log("Cutscene: Зум на монитор завершен");
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
        Debug.Log("Cutscene: Диалог завершен!");

        // Отписываемся от событий
        if (dialogueManager != null && onDialogueLineChanged != null)
        {
            dialogueManager.OnDialogueIndexReached -= onDialogueLineChanged;
        }

        // Ждем 3 секунды на последнем плане и загружаем следующую сцену
        StartCoroutine(EndCutscene());
    }

    IEnumerator EndCutscene()
    {
        Debug.Log("Cutscene: Задержка перед завершением...");
        yield return new WaitForSeconds(3f);

        Debug.Log("Cutscene: Загружаю следующую сцену...");
        UnityEngine.SceneManagement.SceneManager.LoadScene("NextSceneName");
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