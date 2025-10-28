using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class SecretaryPath : MonoBehaviour
{
    [Header("Navigation")]
    public NavMeshAgent agent;
    public Animator animator;

    [Header("Main Path (уход из офиса)")]
    public Transform[] waypoints; // маршрут, когда она уходит
    private int currentWaypoint = 0;

    [Header("Return Path (возвращение в офис)")]
    public Transform[] waypointsReturn; // маршрут, когда возвращается
    private int currentReturnIndex = 0;

    [Header("Final Path (после взятия чипа)")]
    public Transform[] finalWaypoints;
    private int currentFinalWaypoint = 0;
    private bool finalPathActive = false;

    [Header("Chip Pickup")]
    public Transform handAnchor; // Привязать в инспекторе к руке
    public GameObject chip;      // Объект чипа

    [Header("Door Control")]
    public Animator doorAnimator;
    public string doorBoolName = "isOpen";
    public float doorOpenDistance = 2.5f;
    public float doorCloseDuration = 1f;
    private bool doorOpened = false;
    private bool doorClosing = false;

    [Header("Idle Rotation")]
    public float idleRotationAngle = -30f; // Поворот на 30° влево

    [Header("References")]
    public FinalDialogue finalDialogue;

    [Header("Second Part Trigger")]
    public int secondPartWaypointIndex = 1; // waypoint для запуска второй части диалога
    public DialogueTrigger dialogueTrigger;  // ссылка на триггер второй части диалога

    [Header("Start Conditions")]
    public bool waitForFinalDialogue = true; // Ждать завершения финального диалога
    private bool canStartFinalPath = false;  // Можно начинать финальный путь

    private bool isReturning = false;
    private bool pathActive = false;
    public bool finalPathCompleted = false;
    public System.Action OnFinalPathCompleted;
    private bool secondPartTriggered = false;
    private bool finalDialogueStarted = false;

    void Start()
    {
        if (agent != null)
            agent.isStopped = true;
    }

    void Update()
    {
        // Проверяем, активен ли путь и существует ли NavMeshAgent
        if (!pathActive || agent == null)
            return;

        // --- Автоматическое открытие двери при приближении ---
        if (!doorOpened && doorAnimator != null)
        {
            float distToDoor = Vector3.Distance(transform.position, doorAnimator.transform.position);
            if (distToDoor < doorOpenDistance)
            {
                doorAnimator.SetBool(doorBoolName, true);
                doorOpened = true;
            }
        }

        // --- Автоматическое закрытие двери после прохождения ---
        if (doorOpened && !doorClosing && doorAnimator != null)
        {
            Vector3 toDoor = doorAnimator.transform.position - transform.position;
            if (Vector3.Dot(agent.velocity, toDoor.normalized) < 0)
            {
                StartCoroutine(CloseDoorRoutine());
            }
        }

        // --- Проверка, достиг ли агент текущей точки ---
        if (!agent.pathPending && agent.remainingDistance < 0.15f)
        {
            // --- Путь ухода ---
            if (!isReturning && !finalPathActive)
            {
                currentWaypoint++;
                if (currentWaypoint < waypoints.Length)
                {
                    // Устанавливаем следующую точку назначения
                    agent.SetDestination(waypoints[currentWaypoint].position);

                    // Запуск второй части диалога на нужном waypoint
                    if (!secondPartTriggered && currentWaypoint == secondPartWaypointIndex)
                    {
                        if (dialogueTrigger != null)
                            dialogueTrigger.AllowSecondPartDialogue();

                        secondPartTriggered = true;
                        Debug.Log("Вторая часть диалога активирована по waypoint.");
                    }

                    // Запуск анимации ходьбы
                    if (animator != null)
                        animator.SetTrigger("Walking");
                }
                else
                {
                    // Завершение пути ухода
                    pathActive = false;
                    agent.isStopped = true;
                    Debug.Log("SecretaryPath: секретарь завершил путь ухода.");
                }
            }
            // --- Путь возврата ---
            else if (isReturning && !finalPathActive)
            {
                currentReturnIndex++;
                if (currentReturnIndex < waypointsReturn.Length)
                {
                    // Следующая точка возврата
                    agent.SetDestination(waypointsReturn[currentReturnIndex].position);

                    // Анимация ходьбы
                    if (animator != null)
                        animator.SetTrigger("Walking");
                }
                else
                {
                    // Конец пути возврата
                    agent.isStopped = true;
                    pathActive = false;

                    Debug.Log("SecretaryPath: секретарь вернулся в офис.");

                    // Проигрываем анимацию TakeChip
                    if (animator != null)
                    {
                        animator.SetTrigger("TakeChip");
                        agent.Warp(transform.position);
                    }

                    // Запускаем финальный диалог
                    if (!finalDialogueStarted && finalDialogue != null)
                    {
                        finalDialogue.StartFinalDialogue();
                        finalDialogueStarted = true;
                    }

                    // Включаем коллайдер обратно
                    Collider col = GetComponent<Collider>();
                    if (col != null)
                        col.enabled = true;

                    // --- Запуск финального пути через корутину после анимации ---
                    if (finalWaypoints != null && finalWaypoints.Length > 0)
                    {
                        StartCoroutine(StartFinalPathAfterAnimation());
                    }
                }
            }
            // --- Финальный путь после TakeChip ---
            else if (finalPathActive)
            {
                currentFinalWaypoint++;
                if (currentFinalWaypoint < finalWaypoints.Length)
                {
                    // Устанавливаем следующую точку финального пути
                    agent.SetDestination(finalWaypoints[currentFinalWaypoint].position);

                    // Анимация ходьбы
                    if (animator != null)
                        animator.SetTrigger("Walking");
                }
                else
                {
                    // Конец финального пути
                    agent.isStopped = true;
                    finalPathActive = false;
                    pathActive = false;
                    Debug.Log("SecretaryPath: секретарь завершил финальный путь.");

                    // Вызываем событие
                    OnFinalPathCompleted?.Invoke();
                }
            }
        }

        // --- Плавный поворот в сторону движения ---
        if (agent.velocity.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(agent.velocity.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
        }

        // --- Принудительно синхронизируем позицию модели с агентом ---
        if (!animator.applyRootMotion)
            transform.position = agent.nextPosition;
    }


    // === Начало пути ухода ===
    public void StartMovingAlongPath()
    {
        if (waypoints == null || waypoints.Length == 0) return;

        currentWaypoint = 0;
        isReturning = false;
        pathActive = true;
        secondPartTriggered = false;
        finalDialogueStarted = false;

        agent.isStopped = false;
        agent.SetDestination(waypoints[currentWaypoint].position);

        if (animator != null)
            animator.SetTrigger("StandUp");

        // ?? ДОБАВЛЕНО: сразу блокируем возможность второй посадки, как только началась анимация вставания
        ChairSit chair = FindObjectOfType<ChairSit>();
        if (chair != null)
        {
            chair.DisableInteractionAfterSecretaryLeft();
            Debug.Log("Вторая посадка запрещена: секретарша начала вставать.");
        }
    }

    // === Возврат в комнату ===
    public void ReturnToOffice()
    {
        if (waypointsReturn == null || waypointsReturn.Length == 0) return;

        // Ставим стартовую позицию на первую точку возврата
        transform.position = waypointsReturn[0].position;
        agent.Warp(waypointsReturn[0].position);

        isReturning = true;
        pathActive = true;
        currentReturnIndex = 0;
        finalDialogueStarted = false;

        agent.isStopped = false;
        agent.SetDestination(waypointsReturn[currentReturnIndex].position);

        if (animator != null)
        {
            animator.ResetTrigger("StandUp");
            animator.SetTrigger("StandUp"); // вставание перед ходьбой
        }

        // Выключаем коллайдер при движении назад
        Collider col = GetComponent<Collider>();
        if (col != null)
            col.enabled = false;

        Debug.Log("SecretaryPath: возвращение в офис запущено.");
    }

    // === Закрытие двери ===
    private IEnumerator CloseDoorRoutine()
    {
        doorClosing = true;

        if (doorAnimator != null)
        {
            yield return new WaitForSeconds(doorCloseDuration);
            doorAnimator.SetBool(doorBoolName, false);
        }

        doorOpened = false;
        doorClosing = false;
    }

    private void AttachChipToHand()
    {
        if (chip != null && handAnchor != null)
        {
            // Просто скрываем чип вместо прикрепления
            chip.SetActive(false);
            Debug.Log("Chip taken and hidden.");
        }
    }

    // === Разблокировка финального пути ===
    public void AllowFinalPath()
    {
        canStartFinalPath = true;
        Debug.Log("SecretaryPath: финальный путь разблокирован");

        // Если мы уже ждем в состоянии с чипом - сразу начинаем путь
        if (finalPathActive && pathActive)
        {
            // ВАЖНО: снимаем остановку агента!
            agent.isStopped = false;
            StartFinalPathImmediately();
        }
    }

    private void StartFinalPathImmediately()
    {
        finalPathActive = true;
        currentFinalWaypoint = 0;
        pathActive = true;

        agent.isStopped = false;
        agent.SetDestination(finalWaypoints[currentFinalWaypoint].position);

        if (animator != null)
        {
            // Принудительно переходим в состояние Walking
            animator.Play("Walking");
            // ИЛИ если есть слой:
            // animator.Play("Walking", 0, 0f);
        }

        Debug.Log("Финальный путь запущен");
    }

    private IEnumerator StartFinalPathAfterAnimation()
    {
        // Ждем момент взятия чипа (60 кадр)
        yield return new WaitForSeconds(2.0f);
        AttachChipToHand();
        yield return new WaitForSeconds(5.2f);

        // ПЛАВНЫЙ ПОВОРОТ НА 30° ВЛЕВО
        yield return StartCoroutine(SmoothRotate(idleRotationAngle, 3f));

        if (waitForFinalDialogue && !canStartFinalPath)
        {
            Debug.Log("SecretaryPath: ждем завершения финального диалога...");
            agent.isStopped = true;
            finalPathActive = true;
            pathActive = true;

            yield return new WaitUntil(() => canStartFinalPath);
            agent.isStopped = false;
            Debug.Log("SecretaryPath: финальный диалог завершен, начинаем путь");
        }

        StartFinalPathImmediately();
    }

    // Корутина плавного поворота
    private IEnumerator SmoothRotate(float targetAngle, float duration)
    {
        Quaternion startRotation = transform.rotation;

        // ПОВОРОТ ОТНОСИТЕЛЬНО ТЕКУЩЕГО УГЛА
        Quaternion targetRotation = Quaternion.Euler(0, transform.eulerAngles.y + targetAngle, 0);

        float elapsed = 0f;

        while (elapsed < duration)
        {
            transform.rotation = Quaternion.Slerp(startRotation, targetRotation, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.rotation = targetRotation;
    }
}
