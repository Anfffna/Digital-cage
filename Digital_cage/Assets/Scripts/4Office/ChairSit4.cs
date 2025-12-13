using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ChairSit4 : MonoBehaviour, IInteractable
{
    [Header("Setup")]
    public Transform seatPoint;
    public KeyCode sitKey = KeyCode.E;
    public float standForwardOffset = 0.6f;

    [Header("Auto Sit Settings")]
    public bool autoSitOnStart = true;
    public float autoSitDelay = 3f;

    [Header("Camera Settings")]
    public Transform cameraPivot;
    public float cameraSitX = 10f;
    public float cameraStandX = 0f;
    public Vector3 cameraSitOffset = new Vector3(0f, -1.1f, 0f);
    public float cameraLerpSpeed = 10f;

    [Header("Player Settings")]
    public float rotationLerpSpeed = 10f;

    [Header("Dialogue Settings")]
    [TextArea(2, 5)]
    public List<string> dialogueLines = new List<string>();
    public DialogueManager4 dialogueManager;
    public bool waitForDialoguesToFinish = true;
    public int expectedDialogueLines = 0; // Сколько строк должно быть в диалоге

    [Header("Cursor Settings")]
    public bool hideCursorAfterStanding = true;

    private GameObject player;
    private CharacterController charController;
    private PlayerController playerController;
    private bool playerNearby = false;
    private bool isSitting = false;
    private bool hasStoodUp = false;
    private bool dialoguesFinished = false;
    private int dialogueLinesCounted = 0;
    public bool IsPlayerSitting => isSitting;

    public bool isSecondSitAvailable = false;
    private HandPhoneController_Chair phoneControllerChair;

    private Vector3 cameraOriginalLocalPos;
    private float targetX;
    private float targetY;

    void Start()
    {
        phoneControllerChair = FindObjectOfType<HandPhoneController_Chair>();

        player = GameObject.FindWithTag("Player");
        if (player == null)
        {
            Debug.LogError("ChairSit4: Player с тегом 'Player' не найден!");
            return;
        }

        charController = player.GetComponent<CharacterController>();
        playerController = player.GetComponent<PlayerController>();

        if (cameraPivot == null && playerController != null)
        {
            cameraPivot = playerController.playerCamera;
        }

        if (cameraPivot != null)
        {
            cameraOriginalLocalPos = cameraPivot.localPosition;
        }

        // Устанавливаем ожидаемое количество строк
        expectedDialogueLines = dialogueLines.Count;

        // Автопосадка через 3 секунды
        if (autoSitOnStart)
        {
            StartCoroutine(DelayedAutoSit());
        }
    }

    void Update()
    {
        if (hasStoodUp) return;

        if (playerNearby && !isSitting && Input.GetKeyDown(sitKey))
        {
            SitDown();
        }
        else if (isSitting && Input.GetKeyDown(sitKey) && CanStandUp())
        {
            StandUp();
        }
    }

    void FixedUpdate()
    {
        if (cameraPivot != null && isSitting)
        {
            Vector3 targetPos = cameraOriginalLocalPos + cameraSitOffset;
            cameraPivot.localPosition = Vector3.Lerp(
                cameraPivot.localPosition,
                targetPos,
                Time.deltaTime * cameraLerpSpeed
            );
        }
        else if (cameraPivot != null && !isSitting)
        {
            cameraPivot.localPosition = Vector3.Lerp(
                cameraPivot.localPosition,
                cameraOriginalLocalPos,
                Time.deltaTime * cameraLerpSpeed
            );
        }

        if (isSitting && player != null)
        {
            Vector3 euler = player.transform.eulerAngles;
            float y = Mathf.LerpAngle(euler.y, targetY, Time.deltaTime * rotationLerpSpeed);
            player.transform.rotation = Quaternion.Euler(euler.x, y, euler.z);
        }

        if (playerController != null && isSitting)
        {
            playerController.CameraXRotation = Mathf.Lerp(
                playerController.CameraXRotation,
                targetX,
                Time.deltaTime * rotationLerpSpeed
            );
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !hasStoodUp)
            playerNearby = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            playerNearby = false;
    }

    private IEnumerator DelayedAutoSit()
    {
        yield return new WaitForSeconds(autoSitDelay);

        if (player != null && seatPoint != null && !isSitting && !hasStoodUp)
        {
            SitDown();
        }
    }

    private void SitDown()
    {
        if (player == null || seatPoint == null || hasStoodUp)
            return;

        if (charController != null)
            charController.enabled = false;

        player.transform.position = seatPoint.position;

        targetX = cameraSitX;
        targetY = -16f;

        if (playerController != null)
        {
            playerController.OnSitDown();
            playerController.restrictVerticalLook = true;
            playerController.minSitX = cameraSitX - 5f;
            playerController.maxSitX = cameraSitX + 5f;
            playerController.restrictHorizontalLook = true;
            playerController.minSitY = -16f - 90f;
            playerController.maxSitY = -16f + 90f;
        }

        isSitting = true;
        playerNearby = false;

        // Запускаем диалоги
        if (dialogueLines.Count > 0 && dialogueManager != null)
        {
            StartCoroutine(StartDialoguesWithDelay(0.5f));
        }
        else if (dialogueLines.Count == 0)
        {
            dialoguesFinished = true;
        }

        if (isSecondSitAvailable && phoneControllerChair != null)
        {
            StartCoroutine(DelayedPhonePut());
        }
    }

    private IEnumerator StartDialoguesWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        // ПОДПИСКА НА СОБЫТИЕ OnDialogueIndexReached вместо OnDialogueEnd
        if (dialogueManager != null)
        {
            dialogueManager.OnDialogueIndexReached += OnDialogueLineReached;
        }

        // Запускаем диалог
        dialogueManager.StartDialogue(dialogueLines, OnAllDialoguesFinished);

        Debug.Log("ChairSit4: Диалог запущен");
    }

    // Обработчик для каждой строки диалога
    private void OnDialogueLineReached(int lineIndex)
    {
        dialogueLinesCounted = lineIndex;
        Debug.Log($"ChairSit4: Получена строка диалога {lineIndex} из {expectedDialogueLines}");

        // Если это последняя строка - диалоги завершены
        if (lineIndex >= expectedDialogueLines)
        {
            dialoguesFinished = true;
            Debug.Log("ChairSit4: Все строки диалога получены");

            // Отписываемся от события
            if (dialogueManager != null)
            {
                dialogueManager.OnDialogueIndexReached -= OnDialogueLineReached;
            }
        }
    }

    // Callback, который вызывается когда диалог полностью завершен
    private void OnAllDialoguesFinished()
    {
        dialoguesFinished = true;
        Debug.Log("ChairSit4: Callback диалога - все завершено");

        // Отписываемся от события
        if (dialogueManager != null)
        {
            dialogueManager.OnDialogueIndexReached -= OnDialogueLineReached;
        }
    }

    private bool CanStandUp()
    {
        if (waitForDialoguesToFinish)
        {
            return dialoguesFinished;
        }
        return true;
    }

    private void StandUp()
    {
        if (hasStoodUp || !CanStandUp() || player == null)
            return;

        hasStoodUp = true;

        if (charController != null)
            charController.enabled = true;

        Vector3 forward = player.transform.forward * standForwardOffset;
        charController.Move(forward);

        if (playerController != null)
        {
            playerController.OnStandUp();
            playerController.restrictVerticalLook = false;
            playerController.restrictHorizontalLook = false;
        }

        isSitting = false;
        playerNearby = false;

        if (hideCursorAfterStanding)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        Collider collider = GetComponent<Collider>();
        if (collider != null)
        {
            collider.enabled = false;
        }

        // Отписываемся от события
        if (dialogueManager != null)
        {
            dialogueManager.OnDialogueIndexReached -= OnDialogueLineReached;
        }

        Debug.Log("ChairSit4: Игрок встал. Больше нельзя садиться.");
    }

    private IEnumerator DelayedPhonePut()
    {
        yield return new WaitForSeconds(1f);
        if (phoneControllerChair != null)
        {
            phoneControllerChair.StartPhonePutAnimation();
        }
        isSecondSitAvailable = false;
    }

    public string GetInteractionText()
    {
        if (hasStoodUp) return "";

        if (isSitting && !dialoguesFinished && waitForDialoguesToFinish)
            return "";

        if (isSitting && CanStandUp())
            return "Встать [E]";

        if (!isSitting && !hasStoodUp)
            return "Сесть [E]";

        return "";
    }

    public void Interact()
    {
        if (hasStoodUp) return;

        if (isSitting && CanStandUp())
            StandUp();
        else if (!isSitting)
            SitDown();
    }

    // Метод для принудительного завершения
    public void ForceFinishDialogues()
    {
        dialoguesFinished = true;
        if (dialogueManager != null)
        {
            dialogueManager.ForceEndDialogue();
        }
    }

    public void ForceStandUp()
    {
        if (isSitting && !hasStoodUp)
        {
            dialoguesFinished = true;
            StandUp();
        }
    }

    void OnDestroy()
    {
        if (dialogueManager != null)
        {
            dialogueManager.OnDialogueIndexReached -= OnDialogueLineReached;
        }
    }
}