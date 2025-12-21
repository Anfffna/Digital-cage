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

    [Header("Rotation Settings")]
    public float rotationLerpSpeed = 5f;
    public float horizontalRotationLimit = 30f; // Лимит поворота влево-вправо в градусах

    private GameObject player;
    private CharacterController charController;
    private PlayerController playerController;
    private bool playerNearby = false;
    private bool isSitting = false;
    private bool hasSat = false; // Флаг одноразового использования

    private Vector3 cameraOriginalLocalPos;
    private float targetX;
    private float initialYRotation; // Начальный поворот при посадке
    private float currentYRotation; // Текущий поворот по Y

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
            cameraOriginalLocalPos = cameraPivot.localPosition;
    }

    void Update()
    {
        // Взаимодействие только если игрок рядом и еще не садился
        if (playerNearby && !isSitting && !hasSat && Input.GetKeyDown(sitKey))
            SitDown();

        // Плавное смещение камеры
        if (cameraPivot != null)
        {
            Vector3 targetPos = isSitting
                ? cameraOriginalLocalPos + cameraSitOffset
                : cameraOriginalLocalPos;

            cameraPivot.localPosition = Vector3.Lerp(cameraPivot.localPosition, targetPos, Time.deltaTime * cameraLerpSpeed);
        }

        // Если сидим - обрабатываем поворот головы
        if (isSitting && playerController != null)
        {
            // Плавный поворот вертикали камеры
            playerController.CameraXRotation = Mathf.Lerp(
                playerController.CameraXRotation,
                targetX,
                Time.deltaTime * rotationLerpSpeed
            );

            // Обработка горизонтального поворота
            HandleHorizontalRotation();
        }
    }

    void HandleHorizontalRotation()
    {
        if (!isSitting || player == null) return;

        // Получаем ввод мыши для горизонтального поворота
        float mouseX = Input.GetAxis("Mouse X");

        // Если есть ввод - изменяем текущий поворот
        if (Mathf.Abs(mouseX) > 0.01f)
        {
            currentYRotation += mouseX * playerController.mouseSensitivity;

            // Ограничиваем поворот в пределах лимита
            currentYRotation = Mathf.Clamp(
                currentYRotation,
                initialYRotation - horizontalRotationLimit,
                initialYRotation + horizontalRotationLimit
            );
        }

        // Плавный поворот игрока
        Vector3 euler = player.transform.eulerAngles;
        float targetY = Mathf.LerpAngle(euler.y, currentYRotation, Time.deltaTime * rotationLerpSpeed);
        player.transform.rotation = Quaternion.Euler(euler.x, targetY, euler.z);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            playerNearby = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            playerNearby = false;
    }

    private void SitDown()
    {
        if (player == null || seatPoint == null || hasSat)
            return;

        if (charController != null)
            charController.enabled = false;

        // Перемещаем игрока на точку сидения
        player.transform.position = seatPoint.position;

        // Запоминаем начальный поворот
        initialYRotation = player.transform.eulerAngles.y;
        currentYRotation = initialYRotation;

        // Устанавливаем целевой угол камеры по вертикали
        targetX = cameraSitX;

        if (playerController != null)
        {
            playerController.OnSitDown();

            // Блокировка вертикальной камеры
            playerController.restrictVerticalLook = true;
            playerController.minSitX = cameraSitX - 5f;
            playerController.maxSitX = cameraSitX + 5f;

            // Снимаем ограничения по горизонтали (будем обрабатывать сами)
            playerController.restrictHorizontalLook = false;
        }

        isSitting = true;
        hasSat = true; // Помечаем что уже сидели
        playerNearby = false;

        // Отключаем коллайдер чтобы нельзя было взаимодействовать снова
        Collider col = GetComponent<Collider>();
        if (col != null)
            col.enabled = false;

        Debug.Log("SitWork: Игрок сел. Вставание отключено. Можно поворачивать головой влево-вправо.");
    }

    // Метод для принудительного вставания (если понадобится)
    public void ForceStandUp()
    {
        if (!isSitting) return;

        if (player == null) return;

        if (charController != null)
            charController.enabled = true;

        // Двигаем игрока немного вперед
        Vector3 forward = player.transform.forward * standForwardOffset;
        charController.Move(forward);

        if (playerController != null)
        {
            playerController.OnStandUp();
            playerController.restrictVerticalLook = false;
            playerController.restrictHorizontalLook = false;
        }

        isSitting = false;
    }

    // Реализация интерфейса IInteractable
    public string GetInteractionText()
    {
        if (hasSat) return "";
        return "Сесть";
    }

    public void Interact()
    {
        if (!hasSat && !isSitting)
            SitDown();
    }

    // Вспомогательный метод для отладки - показывает лимиты поворота
    void OnDrawGizmosSelected()
    {
        if (seatPoint != null)
        {
            Gizmos.color = Color.yellow;

            // Рисуем дуги ограничения поворота
            float arcLength = 2f;
            Vector3 center = seatPoint.position + Vector3.up * 1f;

            // Левая граница
            Vector3 leftDir = Quaternion.Euler(0, -horizontalRotationLimit, 0) * seatPoint.forward;
            Gizmos.DrawLine(center, center + leftDir * arcLength);

            // Правая граница
            Vector3 rightDir = Quaternion.Euler(0, horizontalRotationLimit, 0) * seatPoint.forward;
            Gizmos.DrawLine(center, center + rightDir * arcLength);

            // Центральная линия
            Gizmos.color = Color.green;
            Gizmos.DrawLine(center, center + seatPoint.forward * arcLength);
        }
    }
}