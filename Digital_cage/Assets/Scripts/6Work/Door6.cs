using UnityEngine;

public class Door6 : MonoBehaviour, IInteractable
{
    [Header("Контроллеры дверей")]
    public RuntimeAnimatorController leftDoorController;
    public RuntimeAnimatorController rightDoorController;

    [Header("Основные двери")]
    public GameObject leftDoorObject;
    public GameObject rightDoorObject;

    [Header("Стекла")]
    public GameObject Glass_left1;
    public GameObject Glass_left2;
    public GameObject Glass_right1;
    public GameObject Glass_right2;

    [Header("Настройки")]
    public string interactionText = "Открыть дверь";
    public bool oneTimeUse = true; // Одноразовое использование

    private Animator leftAnimator;
    private Animator rightAnimator;
    private Animator glassLeft1Animator;
    private Animator glassLeft2Animator;
    private Animator glassRight1Animator;
    private Animator glassRight2Animator;

    private Collider triggerCollider;
    private bool isOpen = false;
    private bool isUsed = false;

    private void Start()
    {
        // Получаем коллайдер
        triggerCollider = GetComponent<Collider>();
        if (triggerCollider == null)
        {
            Debug.LogError("Door6: На объекте нет коллайдера!");
        }

        // Инициализируем аниматоры для основных дверей
        InitializeAnimator(ref leftAnimator, leftDoorObject, leftDoorController, "Левая дверь");
        InitializeAnimator(ref rightAnimator, rightDoorObject, rightDoorController, "Правая дверь");

        // Инициализируем аниматоры для стекол
        // ЛЕВЫЕ стекла используют контроллер левой двери
        InitializeAnimator(ref glassLeft1Animator, Glass_left1, leftDoorController, "Стекло левое 1");
        InitializeAnimator(ref glassLeft2Animator, Glass_left2, leftDoorController, "Стекло левое 2");

        // ПРАВЫЕ стекла используют контроллер правой двери
        InitializeAnimator(ref glassRight1Animator, Glass_right1, rightDoorController, "Стекло правое 1");
        InitializeAnimator(ref glassRight2Animator, Glass_right2, rightDoorController, "Стекло правое 2");
    }

    private void InitializeAnimator(ref Animator animator, GameObject targetObject,
                                   RuntimeAnimatorController controller, string objectName)
    {
        if (targetObject != null)
        {
            animator = targetObject.GetComponent<Animator>();
            if (animator == null)
            {
                animator = targetObject.AddComponent<Animator>();
                Debug.Log($"Door6: Добавлен Animator на {objectName}");
            }

            if (controller != null)
            {
                animator.runtimeAnimatorController = controller;
                Debug.Log($"Door6: Назначен контроллер на {objectName}");
            }
            else
            {
                Debug.LogWarning($"Door6: Не назначен контроллер для {objectName} (это нормально, если объект уже имеет аниматор)");
            }
        }
        else
        {
            Debug.LogWarning($"Door6: Не назначен объект: {objectName}");
        }
    }

    public void Interact()
    {
        if (isUsed && oneTimeUse) return;

        Debug.Log($"Door6: Взаимодействие, isOpen={isOpen}");

        if (isOpen)
            CloseDoors();
        else
            OpenDoors();

        if (oneTimeUse)
        {
            isUsed = true;
            // Отключаем коллайдер
            if (triggerCollider != null)
            {
                triggerCollider.enabled = false;
                Debug.Log("Door6: Триггер отключен (одноразовое использование)");
            }

            // Отключаем этот скрипт, чтобы больше не реагировать
            this.enabled = false;
        }
    }

    public string GetInteractionText()
    {
        if (isUsed && oneTimeUse) return "";
        return interactionText;
    }

    private void OpenDoors()
    {
        Debug.Log("Door6: Открываю двери и стекла...");

        // Основные двери
        SetTriggerOnAnimator(leftAnimator, "Open");
        SetTriggerOnAnimator(rightAnimator, "Open");

        // Стекла
        SetTriggerOnAnimator(glassLeft1Animator, "Open");
        SetTriggerOnAnimator(glassLeft2Animator, "Open");
        SetTriggerOnAnimator(glassRight1Animator, "Open");
        SetTriggerOnAnimator(glassRight2Animator, "Open");

        isOpen = true;
    }

    private void CloseDoors()
    {
        Debug.Log("Door6: Закрываю двери и стекла...");

        // Основные двери
        SetTriggerOnAnimator(leftAnimator, "Close");
        SetTriggerOnAnimator(rightAnimator, "Close");

        // Стекла
        SetTriggerOnAnimator(glassLeft1Animator, "Close");
        SetTriggerOnAnimator(glassLeft2Animator, "Close");
        SetTriggerOnAnimator(glassRight1Animator, "Close");
        SetTriggerOnAnimator(glassRight2Animator, "Close");

        isOpen = false;
    }

    private void SetTriggerOnAnimator(Animator animator, string triggerName)
    {
        if (animator != null && animator.runtimeAnimatorController != null)
        {
            // Проверяем, есть ли такой параметр в аниматоре
            bool hasParameter = false;
            foreach (var param in animator.parameters)
            {
                if (param.name == triggerName && param.type == AnimatorControllerParameterType.Trigger)
                {
                    hasParameter = true;
                    break;
                }
            }

            if (hasParameter)
            {
                // Сбрасываем противоположный триггер
                string oppositeTrigger = triggerName == "Open" ? "Close" : "Open";
                animator.ResetTrigger(oppositeTrigger);

                // Устанавливаем нужный триггер
                animator.SetTrigger(triggerName);
                Debug.Log($"Door6: Триггер '{triggerName}' установлен на {animator.gameObject.name}");
            }
            else
            {
                Debug.LogWarning($"Door6: У аниматора {animator.gameObject.name} нет триггера '{triggerName}'");
            }
        }
        else if (animator == null)
        {
            Debug.LogWarning($"Door6: Попытка установить триггер на null аниматор");
        }
        else if (animator.runtimeAnimatorController == null)
        {
            Debug.LogWarning($"Door6: У аниматора {animator.gameObject.name} нет контроллера");
        }
    }

    // Отладочная визуализация в редакторе
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 0.2f);

        // Линии к дверям
        if (leftDoorObject != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, leftDoorObject.transform.position);
        }

        if (rightDoorObject != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, rightDoorObject.transform.position);
        }
    }
}