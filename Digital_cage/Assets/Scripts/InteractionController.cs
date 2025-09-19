using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InteractionController : MonoBehaviour
{
    [Header("Settings")]
    public float interactionDistance = 3f; // Дистанция взаимодействия
    public KeyCode interactionKey = KeyCode.E; // Клавиша для взаимодействия
    public LayerMask interactionLayer; // Слой для взаимодействия (опционально)

    [Header("UI References")]
    public GameObject interactionUI; // Весь UI элемент подсказки
    public TextMeshProUGUI interactionText; // Текст подсказки

    private Camera playerCamera;
    private IInteractable currentInteractable;

    void Start()
    {
        playerCamera = GetComponentInChildren<Camera>();

        // Скрываем UI при старте
        if (interactionUI != null)
            interactionUI.SetActive(false);
    }

    void Update()
    {
        FindInteractableObject();
        HandleInteractionInput();
    }

    void FindInteractableObject()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;

        // Проверяем лучом вперед от камеры
        if (Physics.Raycast(ray, out hit, interactionDistance, interactionLayer))
        {
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();

            if (interactable != null)
            {
                currentInteractable = interactable;
                ShowInteractionUI(interactable.GetInteractionText());
                return;
            }
        }

        // Если ничего не нашли, скрываем UI
        currentInteractable = null;
        HideInteractionUI();
    }

    void HandleInteractionInput()
    {
        if (currentInteractable != null && Input.GetKeyDown(interactionKey))
        {
            currentInteractable.Interact();
        }
    }

    void ShowInteractionUI(string text)
    {
        if (interactionUI != null)
        {
            interactionUI.SetActive(true);
            if (interactionText != null)
                interactionText.text = $"{text} [E]";
        }
    }

    void HideInteractionUI()
    {
        if (interactionUI != null)
            interactionUI.SetActive(false);
    }

    // Визуализация луча в редакторе (для отладки)
    void OnDrawGizmos()
    {
        if (playerCamera != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(playerCamera.transform.position, playerCamera.transform.forward * interactionDistance);
        }
    }
}