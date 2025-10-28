using UnityEngine;
using System.Collections;

public class ChairSit : MonoBehaviour, IInteractable
{
    [Header("Setup")]
    public Transform seatPoint;
    public KeyCode sitKey = KeyCode.E;
    public float standForwardOffset = 0.6f;

    [Header("Camera Settings")]
    public Transform cameraPivot;
    public float cameraSitX = 10f;
    public float cameraStandX = 0f;
    public Vector3 cameraSitOffset = new Vector3(0f, -1.7f, 0f);
    public float cameraLerpSpeed = 5f;

    [Header("Player Settings")]
    public float rotationLerpSpeed = 5f;

    [HideInInspector]
    public bool secretaryLeft = false; // запрет садитьс€ после ухода секретарши

    [Header("References")]
    public DialogueTrigger dialogueTrigger;      // ссылка на диалог триггер

    private GameObject player;
    private CharacterController charController;
    private PlayerController playerController;
    private HandPhoneController handPhoneController;
    private bool playerNearby = false;
    private bool isSitting = false;
    public bool IsPlayerSitting => isSitting;

    // ¬ начале класса ChairSit
    public bool isSecondSitAvailable = false; // добавь этот флаг
    private HandPhoneController_Chair phoneControllerChair; // ссылка на контроллер руки

    private bool lockAfterSecondSit = false; // запрещает вставание после второй посадки


    private Vector3 cameraOriginalLocalPos;
    private Quaternion targetRotation;
    private float targetX;
    private float targetY;
    private bool isManualRotating = false;      // ƒќЅј¬Ћ≈Ќќ: флаг Ч корутина сейчас вручную вращает игрока (чтобы Update не конфликтовал)
    private bool panelLocked = false;           // ƒќЅј¬Ћ≈Ќќ: флаг Ч камера зафиксирована на панели пока игрок не встанет
    public float panelYaw = -70f;               // ƒќЅј¬Ћ≈Ќќ: целевой yaw (градусы) дл€ панели Ч можно поправить в инспекторе
    public float panelRotateDelay = 5f;         // ƒќЅј¬Ћ≈Ќќ: задержка перед началом поворота (в секундах)
    public float panelRotateDuration = 2f;      // ƒќЅј¬Ћ≈Ќќ: длительность самого поворота (в секундах)

    // —обытие, которое вызываетс€, когда игрок садитс€
    public System.Action OnPlayerSatDown;


    void Start()
    {
        phoneControllerChair = FindObjectOfType<HandPhoneController_Chair>();

        player = GameObject.FindWithTag("Player");
        if (player == null)
        {
            Debug.LogError("ChairSit: Player с тегом 'Player' не найден!");
            return;
        }

        charController = player.GetComponent<CharacterController>();
        playerController = player.GetComponent<PlayerController>();

        if (cameraPivot == null && playerController != null)
            cameraPivot = playerController.playerCamera;

        if (cameraPivot != null)
            cameraOriginalLocalPos = cameraPivot.localPosition;

        // === ƒќЅј¬Ћ≈Ќќ: получаем ссылку на HandPhoneController ===
        handPhoneController = FindObjectOfType<HandPhoneController>();
        if (handPhoneController == null)
        {
            Debug.LogWarning("ChairSit: HandPhoneController не найден, проверка телефона отключена!");
        }
    }

    void Update()
    {
        if (playerNearby && !isSitting && Input.GetKeyDown(sitKey))
            SitDown();
        else if (isSitting && Input.GetKeyDown(sitKey))
            StandUp();

        // плавное смещение камеры по позиции (Y и Z)
        if (cameraPivot != null)
        {
            Vector3 targetPos = isSitting
                ? cameraOriginalLocalPos + cameraSitOffset
                : cameraOriginalLocalPos;

            cameraPivot.localPosition = Vector3.Lerp(cameraPivot.localPosition, targetPos, Time.deltaTime * cameraLerpSpeed);
        }

        // ---------- «јћ≈Ќ≈Ќќ: плавный поворот игрока по Y к targetY (учЄт корутины) ----------
        if (isSitting && player != null && !isManualRotating) // изменено: пропускаем, когда корутина вручную вращает
        {
            Vector3 euler = player.transform.eulerAngles;
            float y = Mathf.LerpAngle(euler.y, targetY, Time.deltaTime * rotationLerpSpeed); // изменено: используем переменную targetY
            player.transform.rotation = Quaternion.Euler(euler.x, y, euler.z);
        }

        // плавное изменение вертикального угла камеры (X) к targetX
        if (playerController != null)
        {
            if (isSitting)
                playerController.CameraXRotation = Mathf.Lerp(playerController.CameraXRotation, targetX, Time.deltaTime * rotationLerpSpeed);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) playerNearby = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player")) playerNearby = false;
    }

    private void SitDown()
    {
        if (secretaryLeft)
        {
            Debug.Log("—адитьс€ пока нельз€: секретарша ушла!");
            return; // блокируем садение
        }

        if (player == null || seatPoint == null) return;

        if (charController != null) charController.enabled = false;

        // перемещаем игрока на точку сидени€
        player.transform.position = seatPoint.position;

        // целевые углы
        targetX = cameraSitX;
        targetY = -16f;
        targetRotation = Quaternion.Euler(0f, targetY, 0f);

        if (playerController != null)
        {
            playerController.OnSitDown();

            // блокировка мыши в пределах сид€чего угла
            playerController.restrictVerticalLook = true;
            playerController.minSitX = cameraSitX - 5f;
            playerController.maxSitX = cameraSitX + 5f;

            playerController.restrictHorizontalLook = true;
            playerController.minSitY = -16f - 90f;
            playerController.maxSitY = -16f + 90f;
        }

        isSitting = true;
        playerNearby = false;

        // ==== Ќовый код: уведомл€ем диалог, что игрок сел ====
        OnPlayerSatDown?.Invoke();

        // ==== Ќовый код: если это ¬“ќ–јя посадка, запускаем анимацию руки ====
        if (isSecondSitAvailable && phoneControllerChair != null)
        {
            Debug.Log("¬тора€ посадка: запускаем анимацию руки и возвращение секретарши.");
            StartCoroutine(DelayedPhonePut());

            SecretaryPath sec = FindObjectOfType<SecretaryPath>();
            if (sec != null)
            {
                sec.ReturnToOffice(); // ? просто вызываем без аргументов
            }

            lockAfterSecondSit = true; // запрещаем встать
        }
    }

    private void StandUp()
    {
        if (lockAfterSecondSit)
        {
            Debug.Log("»грок не может встать после второй посадки.");
            return;
        }

        if (player == null) return;

        // === ƒќЅј¬Ћ≈Ќќ: снимаем фиксацию панели и ограничени€ камеры, чтобы игрок мог встать ===
        panelLocked = false;
        isManualRotating = false;
        if (playerController != null)
        {
            playerController.restrictHorizontalLook = false;
            playerController.restrictVerticalLook = false;
        }

        // === ƒќЅј¬Ћ≈Ќќ: проверка, что телефон положен ===
        if (handPhoneController != null && !handPhoneController.hasPutPhone)
        {
            Debug.Log("»грок не может встать, пока не положит телефон.");
            return;
        }

        if (charController != null) charController.enabled = true;

        // двигаем игрока немного вперед
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

    // === ƒќЅј¬Ћ≈Ќќ: вызываетс€ извне (DialogueTrigger) после каждой реплики ===
    public void OnDialogueLineFinished(int lineIndex)
    {
        if (lineIndex == 8) // если это конец 8-й реплики
        {
            StartCoroutine(RotateCameraLeftDelayed()); // запускаем корутину поворота с задержкой
        }
    }

    // === ƒќЅј¬Ћ≈Ќќ: корутина Ч подождать, затем плавно повернуть игрока влево и "зафиксировать" камеру на панели ===
    private IEnumerator RotateCameraLeftDelayed()
    {
        yield return new WaitForSeconds(panelRotateDelay); // ƒќЅј¬Ћ≈Ќќ: ждем N секунд (panelRotateDelay)

        if (!isSitting || player == null) // если игрок уже не сидит или игрок пропал Ч отмен€ем
            yield break;

        isManualRotating = true; // ƒќЅј¬Ћ≈Ќќ: блокируем автоматический Lerp в Update, чтобы не было конфликта

        Quaternion startRot = player.transform.rotation; // стартовый поворот игрока
        Quaternion endRot = Quaternion.Euler(startRot.eulerAngles.x, panelYaw, startRot.eulerAngles.z); // целевой поворот по yaw

        float elapsed = 0f;
        while (elapsed < panelRotateDuration) // ƒќЅј¬Ћ≈Ќќ: плавный Slerp в течение panelRotateDuration
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / panelRotateDuration)); // плавность
            player.transform.rotation = Quaternion.Slerp(startRot, endRot, t); // примен€ем промежуточный поворот
            yield return null;
        }

        player.transform.rotation = endRot; // гарантируем точный целевой поворот
        targetY = panelYaw; // ƒќЅј¬Ћ≈Ќќ: синхронизируем переменную targetY с новым положением

        // ƒќЅј¬Ћ≈Ќќ: устанавливаем ограничение горизонтали вокруг панели (±90∞)
        if (playerController != null)
        {
            playerController.restrictHorizontalLook = true;
            playerController.minSitY = panelYaw - 90f;
            playerController.maxSitY = panelYaw + 90f;
        }

        panelLocked = true;      // ƒќЅј¬Ћ≈Ќќ: помечаем, что камера зафиксирована на панели
        isManualRotating = false; // ƒќЅј¬Ћ≈Ќќ: снимаем блокировку Ч дальше Update может поддерживать targetY плавно при необходимости
    }

    private IEnumerator DelayedPhonePut()
    {
        yield return new WaitForSeconds(1f); // задержка перед началом (если нужно)
        phoneControllerChair.StartPhonePutAnimation();
        isSecondSitAvailable = false; // чтобы не срабатывало повторно
    }

    public void DisableInteractionAfterSecretaryLeft()
    {
        secretaryLeft = true; // помечаем, что секретарша ушла

        // ќтключаем триггер-коллайдер, чтобы не мешал прицелу
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false;
            Debug.Log("ChairSit: триггер кресла отключЄн после ухода секретарши.");
        }
    }

    public string GetInteractionText()
    {
        return ""; // ничего не выводим Ч только курсор
    }

    public void Interact()
    {
        if (isSitting)
            StandUp();
        else
            SitDown();
    }
}