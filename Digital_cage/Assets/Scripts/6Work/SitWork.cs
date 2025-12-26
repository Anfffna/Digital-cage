using UnityEngine;

public class SitWork : MonoBehaviour, IInteractable
{
    [Header("Setup")]
    public Transform seatPoint;
    public KeyCode sitKey = KeyCode.E;
    public float standForwardOffset = 0.6f;

    [Header("Camera Settings")]
    public Transform cameraPivot;
    public float cameraSitX = 10f;
    public Vector3 cameraSitOffset = new Vector3(0f, -1.7f, 0f);
    public float cameraLerpSpeed = 5f;

    [Header("Camera Zoom Settings")]
    public bool enableZoom = true;
    public float zoomAmount = 0.5f;
    public float zoomDuration = 1f;
    public bool lockRotationDuringZoom = true;
    public AnimationCurve zoomCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Rotation Settings")]
    public float rotationLerpSpeed = 3f;
    public float horizontalRotationLimit = 30f;
    public float mouseSensitivityMultiplier = 0.3f;
    public float inputSmoothing = 10f;

    [Header("Work Settings")]
    public Work workScript; // Ссылка на скрипт Work с документами
    public KeyCode standKey = KeyCode.Q; // Клавиша для вставания
    public bool canStandOnlyAfterWork = true; // Можно встать только после завершения работы

    public TodoUI6 todoUI6;

    private GameObject player;
    private CharacterController charController;
    private PlayerController playerController;
    private bool playerNearby = false;
    private bool isSitting = false;
    private bool hasSat = false;
    private bool workCompleted = false; // Флаг завершения работы
    public bool IsPlayerSitting => isSitting;

    private Vector3 cameraOriginalLocalPos;
    private float targetX;
    private float fixedYRotation;
    private float targetYRotation;
    private float currentYRotation;

    private Vector3 originalCameraLocalPos;
    private bool isZooming = false;
    private float zoomProgress = 0f;
    private bool rotationLocked = false;

    private float mouseXInput = 0f;
    private float smoothedMouseX = 0f;
    private bool isLookingAtMonitor = true;

    void Start()
    {
        player = GameObject.FindWithTag("Player");
        if (player == null)
        {
            Debug.LogError("SitWork: Player с тегом 'Player' не найден!");
            return;
        }

        charController = player.GetComponent<CharacterController>();
        playerController = player.GetComponent<PlayerController>();

        if (cameraPivot == null && playerController != null)
            cameraPivot = playerController.playerCamera;

        if (cameraPivot != null)
        {
            cameraOriginalLocalPos = cameraPivot.localPosition;
            originalCameraLocalPos = cameraPivot.localPosition;
        }

        // Инициализируем флаг завершения работы
        workCompleted = false;
    }

    void Update()
    {
        // Проверяем ввод для сидения (только если еще не сидели и работа не завершена)
        if (playerNearby && !isSitting && !hasSat && !workCompleted && Input.GetKeyDown(sitKey))
            SitDown();

        // Проверяем ввод для вставания
        if (isSitting && Input.GetKeyDown(standKey))
        {
            CheckStandUp();
        }

        HandleCameraZoom();

        if (cameraPivot != null && !isZooming)
        {
            Vector3 targetPos = isSitting
                ? cameraOriginalLocalPos + cameraSitOffset
                : cameraOriginalLocalPos;

            cameraPivot.localPosition = Vector3.Lerp(cameraPivot.localPosition, targetPos, Time.deltaTime * cameraLerpSpeed);
        }

        if (isSitting && playerController != null)
        {
            playerController.CameraXRotation = Mathf.Lerp(
                playerController.CameraXRotation,
                targetX,
                Time.deltaTime * rotationLerpSpeed
            );

            if (!rotationLocked)
            {
                HandleHorizontalRotation();
            }

            ApplySmoothRotation();
        }
    }

    void CheckStandUp()
    {
        // Проверяем можно ли встать
        if (canStandOnlyAfterWork)
        {
            // Проверяем завершена ли работа
            if (workScript != null)
            {
                // Предполагаем, что в Work есть метод IsWorkCompleted()
                // Если нет такого метода, используем другой способ проверки
                bool workDone = CheckIfWorkCompleted();

                if (workDone)
                {
                    ForceStandUp();
                    Debug.Log("SitWork: Работа завершена, можно встать");
                }
                else
                {
                    Debug.Log("SitWork: Работа еще не завершена, нельзя встать");
                    // Можно добавить звук или визуальную подсказку
                }
            }
            else
            {
                // Если скрипт Work не назначен, разрешаем встать всегда
                Debug.LogWarning("SitWork: Скрипт Work не назначен, разрешаю встать");
                ForceStandUp();
            }
        }
        else
        {
            // Если можно вставать в любое время
            ForceStandUp();
        }
    }

    bool CheckIfWorkCompleted()
    {
        // Способ 1: Если в Work есть публичное свойство или метод
        // Например: return workScript.IsWorkCompleted();

        // Способ 2: Проверяем через рефлексию (если нужно)
        // Но лучше добавить публичный метод в Work

        // Способ 3: Используем публичное поле (если оно есть)
        // return workScript.isWorkCompleted;

        // Временно возвращаем true для тестирования
        // В реальном коде нужно использовать один из способов выше
        return true;
    }

    void HandleCameraZoom()
    {
        if (!enableZoom || cameraPivot == null) return;

        if (isSitting && !isZooming && zoomProgress < 1f)
        {
            isZooming = true;
            if (lockRotationDuringZoom)
            {
                rotationLocked = true;
            }
        }
        else if (!isSitting && isZooming)
        {
            isZooming = false;
            zoomProgress = 0f;
            rotationLocked = false;
        }

        if (isZooming && zoomProgress < 1f)
        {
            zoomProgress += Time.deltaTime / zoomDuration;
            zoomProgress = Mathf.Clamp01(zoomProgress);

            float curveValue = zoomCurve.Evaluate(zoomProgress);
            Vector3 zoomOffset = cameraPivot.forward * zoomAmount * curveValue;

            Vector3 targetPos = cameraOriginalLocalPos + cameraSitOffset + zoomOffset;
            cameraPivot.localPosition = Vector3.Lerp(cameraPivot.localPosition, targetPos, Time.deltaTime * cameraLerpSpeed * 2f);

            if (zoomProgress >= 1f && rotationLocked)
            {
                rotationLocked = false;
                ResetToMonitorView();
            }
        }
        else if (!isZooming && zoomProgress > 0f)
        {
            zoomProgress -= Time.deltaTime / zoomDuration;
            zoomProgress = Mathf.Clamp01(zoomProgress);

            float curveValue = zoomCurve.Evaluate(zoomProgress);
            Vector3 zoomOffset = cameraPivot.forward * zoomAmount * curveValue;

            Vector3 targetPos = cameraOriginalLocalPos + zoomOffset;
            cameraPivot.localPosition = Vector3.Lerp(cameraPivot.localPosition, targetPos, Time.deltaTime * cameraLerpSpeed * 2f);
        }
    }

    void ResetToMonitorView()
    {
        mouseXInput = 0f;
        smoothedMouseX = 0f;
        targetYRotation = fixedYRotation;
        isLookingAtMonitor = true;
    }

    void HandleHorizontalRotation()
    {
        if (!isSitting || player == null) return;

        mouseXInput = Input.GetAxis("Mouse X");
        smoothedMouseX = Mathf.Lerp(smoothedMouseX, mouseXInput, Time.deltaTime * inputSmoothing);

        if (Mathf.Abs(smoothedMouseX) > 0.01f)
        {
            isLookingAtMonitor = false;
            float rotationChange = smoothedMouseX * playerController.mouseSensitivity * mouseSensitivityMultiplier;
            targetYRotation += rotationChange;

            float maxRotation = fixedYRotation + horizontalRotationLimit;
            float minRotation = fixedYRotation - horizontalRotationLimit;
            targetYRotation = Mathf.Clamp(targetYRotation, minRotation, maxRotation);
        }
        else if (!isLookingAtMonitor)
        {
            ReturnToMonitorView();
        }
    }

    void ReturnToMonitorView()
    {
        targetYRotation = Mathf.LerpAngle(targetYRotation, fixedYRotation, Time.deltaTime * rotationLerpSpeed * 0.5f);

        if (Mathf.Abs(Mathf.DeltaAngle(targetYRotation, fixedYRotation)) < 1f)
        {
            targetYRotation = fixedYRotation;
            isLookingAtMonitor = true;
        }
    }

    void ApplySmoothRotation()
    {
        if (!isSitting || player == null) return;

        currentYRotation = Mathf.LerpAngle(currentYRotation, targetYRotation, Time.deltaTime * rotationLerpSpeed);

        if (Mathf.Abs(Mathf.DeltaAngle(currentYRotation, player.transform.eulerAngles.y)) > 0.1f)
        {
            Vector3 euler = player.transform.eulerAngles;
            player.transform.rotation = Quaternion.Euler(euler.x, currentYRotation, euler.z);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !hasSat && !workCompleted)
            playerNearby = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            playerNearby = false;
    }

    private void SitDown()
    {
        if (player == null || seatPoint == null || hasSat || workCompleted)
            return;

        if (charController != null)
            charController.enabled = false;

        if (todoUI6 != null)
            todoUI6.HideTask1();

        player.transform.position = seatPoint.position;
        fixedYRotation = seatPoint.eulerAngles.y;
        targetYRotation = fixedYRotation;
        currentYRotation = fixedYRotation;
        isLookingAtMonitor = true;
        targetX = cameraSitX;

        if (playerController != null)
        {
            playerController.OnSitDown();
            playerController.restrictVerticalLook = true;
            playerController.minSitX = cameraSitX - 5f;
            playerController.maxSitX = cameraSitX + 5f;
            playerController.restrictHorizontalLook = false;
        }

        isSitting = true;
        hasSat = true;
        playerNearby = false;

        mouseXInput = 0f;
        smoothedMouseX = 0f;

        zoomProgress = 0f;
        isZooming = true;
        rotationLocked = lockRotationDuringZoom;

        Collider col = GetComponent<Collider>();
        if (col != null)
            col.enabled = false;

        Debug.Log($"SitWork: Игрок сел. Одноразовое взаимодействие активировано.");
    }

    public void ForceStandUp()
    {
        if (!isSitting) return;

        if (player == null) return;

        // Завершаем зум
        isZooming = false;
        rotationLocked = false;

        if (charController != null)
        {
            charController.enabled = true;

            // Двигаем игрока вперед
            Vector3 forward = player.transform.forward * standForwardOffset;
            charController.Move(forward);
        }

        if (playerController != null)
        {
            playerController.OnStandUp();
            playerController.restrictVerticalLook = false;
            playerController.restrictHorizontalLook = false;
        }

        isSitting = false;

        // Помечаем, что работу завершили (можно встать только если работа сделана)
        // Если установлена настройка canStandOnlyAfterWork, то при вставании работа считается завершенной
        if (canStandOnlyAfterWork)
        {
            workCompleted = true;
            Debug.Log("SitWork: Работа помечена как завершенная. Повторное сидение невозможно.");
        }
    }

    // Метод для принудительного завершения работы (например, из скрипта Work)
    public void SetWorkCompleted()
    {
        workCompleted = true;
        Debug.Log("SitWork: Работа отмечена как завершенная (вызвано извне)");
    }

    public string GetInteractionText()
    {
        if (hasSat || workCompleted)
        {
            if (isSitting && canStandOnlyAfterWork && !CheckIfWorkCompleted())
            {
                return "Закончите работу чтобы встать (Q)";
            }
            return "";
        }
        return "Сесть (E)";
    }

    public void Interact()
    {
        if (!hasSat && !workCompleted && !isSitting)
            SitDown();
    }

    public void SetMonitorDirection(float yRotation)
    {
        fixedYRotation = yRotation;
        if (isSitting && isLookingAtMonitor)
        {
            targetYRotation = fixedYRotation;
        }
    }

    void OnDrawGizmosSelected()
    {
        if (seatPoint != null)
        {
            Gizmos.color = Color.yellow;
            float arcLength = 3f;
            Vector3 center = seatPoint.position + Vector3.up * 1f;

            Gizmos.color = Color.green;
            Vector3 monitorDir = seatPoint.forward;
            Gizmos.DrawLine(center, center + monitorDir * arcLength);

            Gizmos.color = Color.yellow;
            Vector3 leftLimit = Quaternion.Euler(0, -horizontalRotationLimit, 0) * monitorDir;
            Vector3 rightLimit = Quaternion.Euler(0, horizontalRotationLimit, 0) * monitorDir;

            Gizmos.DrawLine(center, center + leftLimit * arcLength);
            Gizmos.DrawLine(center, center + rightLimit * arcLength);

            Gizmos.color = new Color(0, 0.5f, 1f, 0.3f);
            Gizmos.DrawLine(center + leftLimit * arcLength * 0.5f, center + rightLimit * arcLength * 0.5f);
        }
    }
}