using UnityEngine;
using System.Collections;

public class TakePhone : MonoBehaviour, IInteractable
{
    [Header("Interaction")]
    public GameObject cursorUI;

    [Header("Pickup Settings")]
    public float pickupDistance = 1f;
    public Animator playerAnimator;
    public string takeTriggerName = "TakePhone";
    public float takeDuration = 2f; // длительность анимации подъёма руки

    [Header("Camera Control During Pickup")]
    public PlayerController playerController;
    public Transform cameraPivot;
    public Transform playerTransform;

    [Header("Camera Look Offsets")]
    [Tooltip("Регулировка поворота камеры влево/вправо (в градусах). Применяется к повороту игрока при встании на pivot.")]
    public float cameraYawOffset = 0f;
    [Tooltip("Регулировка наклона камеры вверх/вниз (в градусах). Применяется к cameraPivot.localEulerAngles.x.")]
    public float cameraPitchOffset = 0f;

    [Header("Phone Object")]
    public GameObject phoneObject;
    public Transform phoneSlotInHand; // слот в руке

    [Header("ToDo UI")]
    public ToDoUI toDoUI;

    [Header("Optional: Player Pivot (instant)")]
    public Transform playerPivot;

    [HideInInspector] public bool hasTakenPhone = false;
    [HideInInspector] public bool hasPutPhone = false;

    private bool taken = false;

    private float savedYaw;
    private float savedPitch;

    void Reset()
    {
        if (phoneObject == null)
            phoneObject = this.gameObject;
    }

    void Start()
    {
        if (cursorUI != null)
            cursorUI.SetActive(false);
    }

    public string GetInteractionText()
    {
        return taken ? "" : "Нажмите E, чтобы взять телефон";
    }

    public void Interact()
    {
        Debug.Log("НАЖАТИЕ E ДОШЛО ДО ТЕЛЕФОНА");

        if (taken) return;

        if (toDoUI != null && !toDoUI.CanCompleteTask(0))
        {
            Debug.Log("НЕЛЬЗЯ ВЗЯТЬ ТЕЛЕФОН: задача недоступна");
            return;
        }

        if (playerTransform == null)
            playerTransform = GameObject.FindWithTag("Player")?.transform;

        if (playerTransform == null)
        {
            Debug.LogWarning("ИГРОК С ТЕГОМ PLAYER НЕ НАЙДЕН");
            return;
        }

        float dist = Vector3.Distance(playerTransform.position, transform.position);
        Debug.Log("РАССТОЯНИЕ ДО ТЕЛЕФОНА: " + dist);

        if (dist > pickupDistance)
        {
            Debug.Log("СЛИШКОМ ДАЛЕКО ДЛЯ ВЗЯТИЯ");
            return;
        }

        Debug.Log("ЗАПУСК КОРУТИНЫ ВЗЯТИЯ");
        StartCoroutine(PlayPhonePickupSequence());
    }

    private IEnumerator PlayPhonePickupSequence()
    {
        taken = true;

        // Блокируем движение
        if (playerController != null)
        {
            playerController.canMove = false;
        }

        // --- Плавно перемещаем игрока на позицию pivot ---
        if (playerPivot != null && playerTransform != null)
        {
            CharacterController cc = playerTransform.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            Vector3 startPos = playerTransform.position;
            Quaternion startRot = playerTransform.rotation;

            // Используем ТОЛЬКО заданные в инспекторе углы
            Vector3 targetPos = playerPivot.position;
            targetPos.y = playerTransform.position.y; // ← Добавить эту строку - СОХРАНЯЕМ ВЫСОТУ

            Quaternion targetRot = Quaternion.Euler(0f, playerPivot.eulerAngles.y + cameraYawOffset, 0f);

            float duration = 0.6f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration));

                playerTransform.position = Vector3.Lerp(startPos, targetPos, t);
                playerTransform.rotation = Quaternion.Slerp(startRot, targetRot, t);

                yield return null;
            }

            playerTransform.position = targetPos;
            playerTransform.rotation = targetRot;

            if (cc != null) cc.enabled = true;
        }

        // --- Устанавливаем фиксированный угол камеры из инспектора ---
        if (cameraPivot != null)
        {
            // Плавное перемещение камеры по rotation X
            Vector3 startEuler = cameraPivot.localEulerAngles;
            Vector3 targetEuler = startEuler;
            targetEuler.x = cameraPitchOffset;

            float rotationDuration = 0.4f;
            float rotationElapsed = 0f;

            while (rotationElapsed < rotationDuration)
            {
                rotationElapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(rotationElapsed / rotationDuration));

                Vector3 newEuler = Vector3.Lerp(startEuler, targetEuler, t);
                cameraPivot.localEulerAngles = newEuler;

                yield return null;
            }

            // Финальная установка точного значения
            Vector3 finalEuler = cameraPivot.localEulerAngles;
            finalEuler.x = cameraPitchOffset;
            cameraPivot.localEulerAngles = finalEuler;
        }

        // --- СРАЗУ фиксируем взгляд на заданных углах ---
        if (playerController != null)
        {
            playerController.restrictHorizontalLook = true;
            playerController.restrictVerticalLook = true;

            // Используем ТОЛЬКО заданные в инспекторе значения
            float targetYaw = playerPivot != null ? playerPivot.eulerAngles.y + cameraYawOffset : playerTransform.eulerAngles.y;
            playerController.minSitY = targetYaw;
            playerController.maxSitY = targetYaw;
            playerController.minSitX = cameraPitchOffset;
            playerController.maxSitX = cameraPitchOffset;
        }

        // Запуск анимации руки
        if (playerAnimator != null && !string.IsNullOrEmpty(takeTriggerName))
            playerAnimator.SetTrigger(takeTriggerName);

        // Отключаем курсор
        if (cursorUI != null)
            cursorUI.SetActive(false);

        // Показываем ToDo UI
        if (toDoUI != null)
            toDoUI.ShowPanel();

        // Ждём takeDuration (момент подъёма)
        yield return new WaitForSeconds(takeDuration);

        if (phoneObject != null && phoneSlotInHand != null)
        {
            // Отключаем коллайдер, если есть
            var phoneCollider = phoneObject.GetComponent<Collider>();
            if (phoneCollider != null)
                phoneCollider.enabled = false;

            // Сохраняем мировые координаты при прикреплении
            phoneObject.transform.SetParent(phoneSlotInHand, true);

            // Плавно позиционируем в слоте за 0.1 сек
            float elapsed = 0f;
            float duration = 0.1f;
            Vector3 startPos = phoneObject.transform.localPosition;
            Quaternion startRot = phoneObject.transform.localRotation;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                phoneObject.transform.localPosition = Vector3.Lerp(startPos, Vector3.zero, t);
                phoneObject.transform.localRotation = Quaternion.Slerp(startRot, Quaternion.identity, t);
                yield return null;
            }

            phoneObject.transform.localPosition = Vector3.zero;
            phoneObject.transform.localRotation = Quaternion.identity;
        }
        else
        {
            Debug.LogWarning("TakePhone: phoneSlotInHand или phoneObject не назначен!");
        }

        hasTakenPhone = true;
        hasPutPhone = true;

        // Зачёркиваем пункт "Взять телефон" в ToDo UI
        if (toDoUI != null)
            toDoUI.MarkItemDone(0);

        // Возвращаем управление игроку
        if (playerController != null)
        {
            playerController.canMove = true;
            playerController.restrictHorizontalLook = false;
            playerController.restrictVerticalLook = false;
        }
    }

    public void OnHoverEnter()
    {
        if (taken) return;
        if (cursorUI != null && !cursorUI.activeSelf)
            cursorUI.SetActive(true);
    }

    public void OnHoverExit()
    {
        if (cursorUI != null && cursorUI.activeSelf)
            cursorUI.SetActive(false);
    }
}