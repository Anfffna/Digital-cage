using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class Secretutka4 : MonoBehaviour
{
    [Header("Navigation")]
    public NavMeshAgent agent;
    public Animator animator;

    [Header("Path Settings")]
    public Transform[] waypoints;
    private int currentWaypoint = 0;

    [Header("Sit Settings")]
    public Transform sitTarget;
    public string sitAnimationTrigger = "SitDown";
    public float sitDistanceThreshold = 0.8f;

    [Header("Rotation Settings")]
    public float targetRotationY = -80f;
    public float rotationSpeed = 2f;

    [Header("Sitting Fix")]
    public float sitHeightAdjustment = 0.15f;
    public float fixDuration = 1.2f;
    public bool useSmartFix = true; // Включить умную коррекцию

    private bool pathActive = false;
    private bool hasReachedTarget = false;
    private bool isSitting = false;
    private Coroutine rotationCoroutine;
    private Coroutine fixPositionCoroutine;
    private Vector3 originalPositionBeforeSit;

    void Start()
    {
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        if (animator == null) animator = GetComponent<Animator>();

        StartCoroutine(StartWithDelay(0.5f));
    }

    IEnumerator StartWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        StartMoving();
    }

    void Update()
    {
        if (!pathActive || hasReachedTarget) return;

        if (agent.remainingDistance < 0.15f && !agent.pathPending)
        {
            currentWaypoint++;
            if (currentWaypoint < waypoints.Length)
            {
                agent.SetDestination(waypoints[currentWaypoint].position);
            }
            else
            {
                GoToFinalTarget();
            }
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            ForceComplete();
        }
    }

    void StartMoving()
    {
        if (waypoints == null || waypoints.Length == 0 || agent == null || sitTarget == null)
        {
            Debug.LogError("Missing components!");
            return;
        }

        currentWaypoint = 0;
        pathActive = true;
        hasReachedTarget = false;
        isSitting = false;

        agent.isStopped = false;
        agent.SetDestination(waypoints[0].position);

        Debug.Log("Started moving!");
    }

    void GoToFinalTarget()
    {
        Debug.Log("Going to final target...");
        agent.SetDestination(sitTarget.position);
        StartCoroutine(CheckForTargetReached());
    }

    IEnumerator CheckForTargetReached()
    {
        while (!hasReachedTarget)
        {
            if (agent.remainingDistance < sitDistanceThreshold && !agent.pathPending)
            {
                OnReachedTarget();
                yield break;
            }
            yield return new WaitForSeconds(0.1f);
        }
    }

    void OnReachedTarget()
    {
        if (hasReachedTarget) return;

        hasReachedTarget = true;
        pathActive = false;

        agent.isStopped = true;
        agent.velocity = Vector3.zero;

        Debug.Log("REACHED! Starting rotation...");

        if (rotationCoroutine != null)
            StopCoroutine(rotationCoroutine);

        rotationCoroutine = StartCoroutine(RotateAndSit());
    }

    IEnumerator RotateAndSit()
    {
        Debug.Log("Starting rotation coroutine");

        // 1. ПОВОРОТ
        Quaternion startRotation = transform.rotation;
        float targetY = NormalizeAngle(transform.eulerAngles.y + targetRotationY);
        Quaternion targetRotation = Quaternion.Euler(0, targetY, 0);

        Debug.Log($"Rotating from {startRotation.eulerAngles.y:F0}° to {targetY:F0}°");

        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime * rotationSpeed;
            transform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);
            yield return null;
        }

        transform.rotation = targetRotation;
        Debug.Log("Rotation complete!");

        // 2. Сохраняем позицию
        originalPositionBeforeSit = transform.position;

        // 3. Короткая пауза
        yield return new WaitForSeconds(0.1f);

        // 4. ПОСАДКА
        SitDown();
    }

    float NormalizeAngle(float angle)
    {
        while (angle < 0) angle += 360;
        while (angle >= 360) angle -= 360;
        return angle;
    }

    void SitDown()
    {
        if (isSitting) return;

        isSitting = true;

        Debug.Log("=== SITTING DOWN ===");

        if (animator != null)
        {
            // Останавливаем предыдущую коррекцию
            if (fixPositionCoroutine != null)
                StopCoroutine(fixPositionCoroutine);

            // Запускаем анимацию
            animator.SetTrigger(sitAnimationTrigger);

            // ВЫЗОВ ВАРИАНТА 3: Умная коррекция по стулу
            if (useSmartFix && sitTarget != null)
            {
                fixPositionCoroutine = StartCoroutine(SmartPositionFix());
            }
            else
            {
                // Или обычная коррекция с задержкой
                fixPositionCoroutine = StartCoroutine(DelayedPositionFix(0.3f));
            }

            Debug.Log($"Trigger '{sitAnimationTrigger}' fired");
        }
        else
        {
            Debug.LogError("No animator!");
        }
    }

    // ВАРИАНТ 3: Умная коррекция по стулу
    IEnumerator SmartPositionFix()
    {
        Debug.Log("Starting SMART position fix using chair reference");

        // Ждем начала анимации (30%)
        yield return new WaitForSeconds(0.5f);

        // Проверяем где должен быть стул
        Vector3 chairCheckPos = sitTarget.position;
        chairCheckPos.y += 0.5f; // Луч из точки выше стула

        RaycastHit hit;
        float rayLength = 1f;
        bool foundChair = false;
        float chairSurfaceHeight = 0f;

        // Ищем поверхность стула
        if (Physics.Raycast(chairCheckPos, Vector3.down, out hit, rayLength))
        {
            Debug.Log($"Found chair surface at height: {hit.point.y:F3}m");
            chairSurfaceHeight = hit.point.y;
            foundChair = true;
        }

        // Если не нашли стул, используем обычную коррекцию
        if (!foundChair)
        {
            Debug.LogWarning("Chair not found, using standard fix");
            StartCoroutine(SmoothPositionFix());
            yield break;
        }

        // Плавно опускаемся на найденную высоту стула
        float startY = transform.position.y;
        float targetY = chairSurfaceHeight + 0.05f; // 5см над поверхностью стула
        float duration = 0.8f;
        float timer = 0f;

        Debug.Log($"Smart fix: {startY:F3}m -> {targetY:F3}m (chair at {chairSurfaceHeight:F3}m)");

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float progress = Mathf.SmoothStep(0f, 1f, timer / duration);
            float newY = Mathf.Lerp(startY, targetY, progress);

            transform.position = new Vector3(
                originalPositionBeforeSit.x,
                newY,
                originalPositionBeforeSit.z
            );
            yield return null;
        }

        // Финальная точная установка
        transform.position = new Vector3(
            originalPositionBeforeSit.x,
            targetY,
            originalPositionBeforeSit.z
        );

        Debug.Log("Smart position fix complete!");
    }

    // Обычная коррекция с задержкой
    IEnumerator DelayedPositionFix(float delay)
    {
        yield return new WaitForSeconds(delay);
        StartCoroutine(SmoothPositionFix());
    }

    IEnumerator SmoothPositionFix()
    {
        Debug.Log($"Starting position fix from {originalPositionBeforeSit.y:F3}m");

        float targetHeight = originalPositionBeforeSit.y - sitHeightAdjustment;
        float timer = 0f;

        while (timer < fixDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / fixDuration;
            float smoothProgress = Mathf.SmoothStep(0f, 1f, progress);

            Vector3 newPosition = transform.position;
            newPosition.y = Mathf.Lerp(originalPositionBeforeSit.y, targetHeight, smoothProgress);

            transform.position = newPosition;
            yield return null;
        }

        FinalGroundCheck();
        Debug.Log("Position fix complete!");
    }

    void FinalGroundCheck()
    {
        RaycastHit hit;
        float rayLength = 1f;
        Vector3 rayStart = transform.position + Vector3.up * 0.5f;

        if (Physics.Raycast(rayStart, Vector3.down, out hit, rayLength))
        {
            float groundHeight = hit.point.y;
            float currentHeight = transform.position.y;

            if (currentHeight - groundHeight > 0.1f)
            {
                transform.position = new Vector3(
                    transform.position.x,
                    groundHeight + 0.05f,
                    transform.position.z
                );
                Debug.Log($"Final ground adjustment: {currentHeight:F3} -> {groundHeight + 0.05f:F3}");
            }
        }
    }

    public void ForceComplete()
    {
        Debug.Log("=== FORCE COMPLETE ===");

        if (!hasReachedTarget)
        {
            OnReachedTarget();
        }
        else if (!isSitting)
        {
            SitDown();
        }
    }

    void OnDrawGizmosSelected()
    {
        if (sitTarget != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(sitTarget.position, sitDistanceThreshold);

            // Рисуем луч для отладки умной коррекции
            if (useSmartFix)
            {
                Gizmos.color = Color.cyan;
                Vector3 rayStart = sitTarget.position + Vector3.up * 0.5f;
                Gizmos.DrawLine(rayStart, rayStart + Vector3.down * 1f);
                Gizmos.DrawSphere(rayStart, 0.05f);
            }
        }
    }
}