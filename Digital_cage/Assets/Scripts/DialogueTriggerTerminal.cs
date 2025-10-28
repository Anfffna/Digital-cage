using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Collider))]
public class DialogueTriggerTerminal : MonoBehaviour, IInteractable
{
    [Header("Dialogue Settings")]
    public DialogueManager dialogueManager;
    [TextArea(2, 5)] public List<string> dialogueLines; // 0 и 1 реплики

    [Header("Secretary")]
    public SecretaryPath secretary;

    [Header("Interaction / Animation")]
    public Transform player;
    public Transform playerHand;
    public Transform handPivotTarget;

    [Header("Player Position at Terminal")]
    public Transform playerTargetPosition;
    public float movePlayerDuration = 0.5f;

    [Header("Camera Control During Interaction")]
    public Transform cameraPivot;              // Камера игрока
    public PlayerController playerController;  // Скрипт управления игроком
    public float cameraCenterYaw = 0f;         // Центр по Y (в сторону терминала)
    public float cameraYawLimit = 0f;          // Ограничение ±10°
    public float cameraPitch = 0f;             // Наклон камеры вниз
    public float cameraLerpSpeed = 0f;

    [Header("Scan Line Animation")]
    public Animator scanLineAnimator;   // Animator полоски ScanLine

    [Header("Phone Condition")]
    public HandPhoneController phoneController; // ссылка на HandPhoneController (у игрока)

    private Animator playerAnimator;

    [Header("Collider Control")]
    public bool enableCollidersOnlyAfterPhonePut = true;

    private bool playerInRange = false;
    private bool dialogueStarted = false;
    private bool canInteract = false;
    private bool hasInteracted = false;
    private bool hasActivatedOnce = false; // чтобы терминал сработал только один раз
    public bool isActivated = false; // ?? Новый флаг — активирован ли терминал

    // Время всех анимаций вместе (TouchTerminal + NotTouchTerminal)
    public float totalAnimationTime = 8.5f;

    void Start()
    {
        // ВЫКЛЮЧАЕМ РОДИТЕЛЬСКИЙ КОЛЛАЙДЕР если телефон не положен
        if (enableCollidersOnlyAfterPhonePut)
        {
            Collider parentCollider = GetComponent<Collider>();
            if (parentCollider != null)
            {
                parentCollider.enabled = false;
            }
        }

        if (player == null)
        {
            var p = GameObject.FindWithTag("Player");
            if (p != null) player = p.transform;
        }

        if (playerHand != null)
        {
            playerAnimator = playerHand.GetComponent<Animator>();
            if (playerAnimator == null)
                Debug.LogWarning("Animator на playerHand не найден!");
        }

        if (dialogueManager == null)
            Debug.LogWarning("DialogueManager не назначен!");
    }

    void Update()
    {
        // Включаем родительский коллайдер когда телефон положен
        if (enableCollidersOnlyAfterPhonePut &&
            phoneController != null &&
            phoneController.hasPutPhone)
        {
            Collider parentCollider = GetComponent<Collider>();
            if (parentCollider != null && !parentCollider.enabled)
            {
                parentCollider.enabled = true;
            }
        }

        // Если уже активировали терминал — не повторять
        if (hasActivatedOnce)
        {
            return;
        }

        // Запуск первой реплики диалога (если нужно оставить эту логику)
        if (!dialogueStarted && dialogueManager != null && playerInRange)
        {
            Debug.Log("Update: Запускаем первую реплику диалога");
            dialogueStarted = true;
            dialogueManager.StartDialogue(dialogueLines, OnDialogueLineFinished, true, false);
        }

        // Оставляем E для взаимодействия, но теперь оно вызывает тот же Interact()
        if (canInteract && !hasInteracted && Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("Update: Нажата E - вызываем Interact()");
            Interact();
        }
    }

    private void OnDialogueLineFinished(int lineIndex)
    {
        if (lineIndex == 0)
        {
            canInteract = true;
        }
    }

    private IEnumerator PlayPalmInteractionCoroutine()
    {
        hasActivatedOnce = true; // терминал активируется только один раз

        // Блокируем движение игрока
        if (playerController != null)
            playerController.canMove = false;

        // Ограничиваем вращение камеры
        if (playerController != null)
        {
            playerController.restrictHorizontalLook = true;
            playerController.restrictVerticalLook = true;

            playerController.minSitY = cameraCenterYaw - cameraYawLimit;
            playerController.maxSitY = cameraCenterYaw + cameraYawLimit;
            playerController.minSitX = cameraPitch - 0f;
            playerController.maxSitX = cameraPitch + 0f;
        }

        // Перемещаем игрока к терминалу
        if (player != null && playerTargetPosition != null)
            yield return StartCoroutine(MovePlayerToTargetCoroutine(player, playerTargetPosition, movePlayerDuration));

        // Анимация прикосновения
        if (playerAnimator != null)
            playerAnimator.SetTrigger("TouchTerminal");

        // Анимация сканлайна
        if (scanLineAnimator != null)
            scanLineAnimator.SetTrigger("GoScanLine");

        // Плавное движение руки
        if (playerHand != null && handPivotTarget != null)
            yield return StartCoroutine(MoveHandToPivotCoroutine(playerHand, handPivotTarget, 0.6f));

        // Ждём первую половину
        yield return new WaitForSeconds(totalAnimationTime / 2f);

        // Анимация убирания руки
        if (playerAnimator != null)
            playerAnimator.SetTrigger("NotTouchTerminal");

        // Ждём вторую половину
        yield return new WaitForSeconds(totalAnimationTime / 2f);

        // Показываем финальную реплику
        if (dialogueManager != null && dialogueLines.Count > 2)
        {
            dialogueManager.StartDialogue(new List<string> { dialogueLines[2] }, null);

            // Секретарь начинает движение
            if (secretary != null)
                secretary.StartMovingAlongPath();
        }

        // Возвращаем управление игроку
        yield return new WaitForSeconds(0.1f);
        if (playerController != null)
        {
            playerController.canMove = true;
            playerController.restrictHorizontalLook = false;
            playerController.restrictVerticalLook = false;
        }
    }

    public void ShowDialogueLine1()
    {
        if (dialogueManager != null && dialogueLines.Count > 1)
            dialogueManager.StartDialogue(new List<string> { dialogueLines[1] }, null);
    }

    private IEnumerator MoveHandToPivotCoroutine(Transform hand, Transform pivot, float duration)
    {
        Vector3 startPos = hand.position;
        Quaternion startRot = hand.rotation;
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, timer / duration);
            hand.position = Vector3.Lerp(startPos, pivot.position, t);
            hand.rotation = Quaternion.Slerp(startRot, pivot.rotation, t);
            yield return null;
        }

        hand.position = pivot.position;
        hand.rotation = pivot.rotation;
    }

    private IEnumerator MovePlayerToTargetCoroutine(Transform player, Transform target, float duration)
    {
        Vector3 startPos = player.position;
        Quaternion startRot = player.rotation;
        Vector3 endPos = target.position;
        Quaternion endRot = target.rotation;

        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, timer / duration);
            player.position = Vector3.Lerp(startPos, endPos, t);
            player.rotation = Quaternion.Slerp(startRot, endRot, t);
            yield return null;
        }

        player.position = endPos;
        player.rotation = endRot;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            playerInRange = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            hasInteracted = false;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 1f);
    }

    // ======================================================
    // === Реализация интерфейса IInteractable (для курсора) ===
    // ======================================================
    public string GetInteractionText()
    {
        return ""; // Без текста, чтобы не выводилась подсказка
    }

    public void Interact()
    {
        // Проверяем что игрок в зоне дочернего коллайдера
        if (!playerInRange)
        {
            Debug.Log("Игрок не в зоне взаимодействия (дочерний коллайдер)");
            return;
        }

        if (isActivated || hasActivatedOnce)
        {
            Debug.Log("Блокировка: терминал уже активирован");
            return;
        }

        if (phoneController != null && phoneController.hasPutPhone)
        {
            Debug.Log("Все условия выполнены! Запускаем взаимодействие...");
            isActivated = true;
            hasActivatedOnce = true;
            playerInRange = true; // Принудительно устанавливаем

            // Скрываем курсор сразу
            var interactionController = FindObjectOfType<InteractionController>();
            if (interactionController != null && interactionController.crosshairDot != null)
            {
                interactionController.crosshairDot.SetActive(false);
                Debug.Log("Курсор скрыт");
            }

            // Запускаем анимацию и взаимодействие
            StartCoroutine(PlayPalmInteractionCoroutine());
            Debug.Log("Корутина PlayPalmInteractionCoroutine запущена");
        }
        else
        {
            Debug.Log($"Условия не выполнены: phoneController={phoneController != null}, hasPutPhone={phoneController?.hasPutPhone}");
            if (phoneController == null)
                Debug.LogError("PhoneController не назначен!");
            else if (!phoneController.hasPutPhone)
                Debug.Log("Телефон еще не положен!");
        }
    }
}












