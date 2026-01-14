using UnityEngine;
using TMPro;

public class InteractionController : MonoBehaviour
{
    [Header("Interaction Settings")]
    public float interactionDistance = 3f;
    public KeyCode interactionKey = KeyCode.E;
    public LayerMask interactableMask;

    [Header("UI")]
    public GameObject crosshairDot;         // Центр. точка (TMP или Image)
    public GameObject interactionUI;        // Панель подсказки
    public TextMeshProUGUI interactionText; // Текст внутри панели

    private Camera playerCamera;
    private IInteractable currentInteractable;

    void Start()
    {
        playerCamera = GetComponentInChildren<Camera>();

        if (crosshairDot != null)
            crosshairDot.SetActive(false);

        if (interactionUI != null)
            interactionUI.SetActive(false);
    }

    void Update()
    {
        CheckForInteractable();
        HandleInput();
    }

    void CheckForInteractable()
    {
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        RaycastHit hit;

        // ИСПОЛЬЗУЕМ Raycast БЕЗ LayerMask - чтобы луч останавливался на любых объектах
        if (Physics.Raycast(ray, out hit, interactionDistance))
        {
            // ПРОВЕРЯЕМ вручную: находится ли объект на нужном слое?
            bool isOnInteractableLayer = ((1 << hit.collider.gameObject.layer) & interactableMask) != 0;

            if (!isOnInteractableLayer)
            {
                ClearCurrentInteractable();
                return;
            }

            IInteractable interactable = hit.collider.gameObject.GetComponent<IInteractable>();

            // Если на коллайдере нет скрипта — игнорируем (не берём с родителя!)
            if (interactable == null)
            {
                ClearCurrentInteractable();
                return;
            }

            // Проверяем, не использован ли объект
            bool alreadyUsed = false;
            if (interactable is TakePhone tp && tp.hasTakenPhone)
                alreadyUsed = true;
            else if (interactable is PhotoCapturePoint pc && pc.isUsed)
                alreadyUsed = true;

            if (!alreadyUsed)
            {
                if (currentInteractable != interactable)
                    currentInteractable = interactable;

                ShowUI(currentInteractable.GetInteractionText());
                return;
            }

            // Если луч никуда не попал или попал в неинтерактив — очищаем
            ClearCurrentInteractable();
        }

        // Если луч никуда не попал или попал в неинтерактив — очищаем
        ClearCurrentInteractable();
    }

    void ClearCurrentInteractable()
    {
        if (currentInteractable != null)
        {
            (currentInteractable as DoorHoverDialogue)?.StopHover();
            ClearUI();
            currentInteractable = null;
        }
    }


    void HandleInput()
    {
        if (currentInteractable != null && Input.GetKeyDown(interactionKey))
        {
            currentInteractable.Interact();
        }
    }

    void ShowUI(string text)
    {
        if (currentInteractable is DialogueTriggerTerminal terminal && terminal.isActivated)
            return; // курсор не показываем для активированного терминала

        if (crosshairDot == null)
            return;

        // --- Проверяем наличие ToDoUI, если есть PhotoCapturePoint или TakePhone ---
        bool todoVisible = false;

        // Проверяем для PhotoCapturePoint
        if (currentInteractable is PhotoCapturePoint photoCapture && photoCapture.toDoUI != null)
        {
            var ui = photoCapture.toDoUI;
            todoVisible = ui.gameObject.activeSelf && ui.panel != null && ui.panel.alpha >= 1f;
        }
        // Проверяем для TakePhone
        else if (currentInteractable is TakePhone takePhone && takePhone.toDoUI != null)
        {
            var ui = takePhone.toDoUI;
            todoVisible = ui.gameObject.activeSelf && ui.panel != null && ui.panel.alpha >= 1f;
        }

        // Если ToDoUI ещё не появилась — полностью скрываем курсор и ничего не показываем
        if (!todoVisible && (currentInteractable is PhotoCapturePoint || currentInteractable is TakePhone))
        {
            if (crosshairDot.activeSelf)
                crosshairDot.SetActive(false);
            return;
        }

        // === Включаем курсор, если он скрыт ===
        if (!crosshairDot.activeSelf)
            crosshairDot.SetActive(true);

        // === Берём TMP-компонент курсора ===
        var tmp = crosshairDot.GetComponent<TextMeshProUGUI>();

        //цвет курсора белый
        if (tmp != null)
        {
            // Белый курсор для: телефона, терминала, и PhotoCapturePoint с todoIndex 1 или 2
            if (currentInteractable is TakePhone ||
                currentInteractable is DoorOpener ||
                currentInteractable is DoorOpener4 ||
                currentInteractable is PhotoHall1 ||
                currentInteractable is PhotoHall2 ||
                currentInteractable is PhotoHall3 ||
                currentInteractable is CerealBox ||
                currentInteractable is Stereo ||
                currentInteractable is ChairSit ||
                currentInteractable is AdventDoor ||
                currentInteractable is InteractableDoor ||
                currentInteractable is CorridorDoor ||
                currentInteractable is LightSwitch ||
                currentInteractable is LightSwitch5 ||
                currentInteractable is CarpetMovement ||
                currentInteractable is DoorBasement ||
                currentInteractable is ExitBasement ||
                currentInteractable is ExitLock ||
                currentInteractable is ExitDoor ||
                currentInteractable is ExitDoor5 ||
                currentInteractable is DoorExit0 ||
                currentInteractable is DoorExit4 ||
                currentInteractable is ShadowBasement ||
                currentInteractable is GameMachine ||
                currentInteractable is Sleep ||
                currentInteractable is Door6 ||
                currentInteractable is ExitDoorMama6 ||
                currentInteractable is SitWork ||
                currentInteractable is DoorsClose ||
                currentInteractable is DoorInteractionComponent ||
                currentInteractable is DialogueTriggerTerminal ||
                (currentInteractable is PhotoCapturePoint capturePoint &&
                 (capturePoint.todoIndex == 1 || capturePoint.todoIndex == 2)))
            {
                tmp.color = Color.white;
            }
            else
            {
                tmp.color = Color.black;
            }
        }

        // === Исключения: не показываем UI-плашку ===
        if (currentInteractable is DoorHoverDialogue) return;
        if (currentInteractable is DoorOpener) return;
        if (currentInteractable is DoorOpener4) return;
        if (currentInteractable is TakePhone) return;
        if (currentInteractable is ChairSit) return;
        if (currentInteractable is ChairSit4) return;
        if (currentInteractable is DialogueTriggerTerminal) return;
        if (currentInteractable is PhotoCapturePoint) return; // плашку не показываем
        if (currentInteractable is CorridorDoor) return;
        if (currentInteractable is PhotoHall1) return;
        if (currentInteractable is PhotoHall2) return;
        if (currentInteractable is PhotoHall3) return;
        if (currentInteractable is Stereo) return;
        if (currentInteractable is CerealBox) return;
        if (currentInteractable is AdventDoor) return;
        if (currentInteractable is InteractableDoor) return;
        if (currentInteractable is LightSwitch) return;
        if (currentInteractable is LightSwitch5) return;
        if (currentInteractable is CarpetMovement) return;
        if (currentInteractable is DoorBasement) return;
        if (currentInteractable is ExitBasement) return;
        if (currentInteractable is ExitLock) return;
        if (currentInteractable is ExitDoor) return;
        if (currentInteractable is ExitDoor5) return;
        if (currentInteractable is ShadowBasement) return;
        if (currentInteractable is GameMachine) return;
        if (currentInteractable is Note) return;
        if (currentInteractable is Oferta) return;
        if (currentInteractable is DoorExit0) return;
        if (currentInteractable is DoorExit4) return;
        if (currentInteractable is Sleep) return;
        if (currentInteractable is Door6) return;
        if (currentInteractable is SitWork) return;
        if (currentInteractable is ExitDoorMama6) return;
        if (currentInteractable is DoorsClose) return;
        if (currentInteractable is DoorInteractionComponent) return;

        // === Обычное поведение для остальных объектов ===
        if (interactionUI != null)
        {
            if (!interactionUI.activeSelf)
                interactionUI.SetActive(true);

            if (interactionText != null)
                interactionText.text = text;
        }
    }


    void ClearUI()
    {
        if (crosshairDot != null && crosshairDot.activeSelf)
            crosshairDot.SetActive(false);

        if (interactionUI != null && interactionUI.activeSelf)
            interactionUI.SetActive(false);

        if (interactionText != null)
            interactionText.text = "";
    }
}
