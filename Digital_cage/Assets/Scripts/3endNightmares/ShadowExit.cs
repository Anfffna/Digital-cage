using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ShadowExit : MonoBehaviour
{
    [Header("Shadow Settings")]
    public GameObject[] shadowObjects;
    public float activationDelay = 2f;
    public float approachSpeed = 2f;
    public float minDistance = 3f;
    public float separationDistance = 2f; // Минимальная дистанция между тенями

    [Header("Effect Settings")]
    public float blinkInterval = 0.5f;
    public float effectChance = 0.3f;

    [Header("Audio Settings")]
    public AudioClip[] whisperSounds;
    public AudioClip[] appearSounds;
    public AudioSource audioSource;

    private List<ShadowData> shadowDataList = new List<ShadowData>();
    private Transform player;
    private bool isActive = false;

    [System.Serializable]
    private class ShadowData
    {
        public GameObject shadowObject;
        public Renderer[] renderers;
        public Vector3 originalPosition;
        public Quaternion originalRotation;
        public bool isVisible = false;
        public bool isActive = false;
        public Animator animator;
        public Vector3 avoidanceForce; // Сила для избегания других теней
    }

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        InitializeShadows();

        if (player == null)
        {
            Debug.LogError("ShadowExit: Player not found!");
        }
    }

    private void InitializeShadows()
    {
        foreach (GameObject shadow in shadowObjects)
        {
            if (shadow != null)
            {
                Animator animator = shadow.GetComponent<Animator>();
                if (animator == null)
                {
                    animator = shadow.AddComponent<Animator>();
                    Debug.Log($"ShadowExit: Добавлен Animator тени {shadow.name}");
                }

                animator.updateMode = AnimatorUpdateMode.Normal;
                animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;

                ShadowData data = new ShadowData
                {
                    shadowObject = shadow,
                    renderers = shadow.GetComponentsInChildren<Renderer>(),
                    originalPosition = shadow.transform.position,
                    originalRotation = shadow.transform.rotation,
                    isVisible = false,
                    isActive = false,
                    animator = animator,
                    avoidanceForce = Vector3.zero
                };

                SetShadowVisibility(data, false);
                shadowDataList.Add(data);
            }
        }

        Debug.Log($"ShadowExit: Инициализировано {shadowDataList.Count} теней");
    }

    public void StartShadows()
    {
        if (isActive || player == null) return;

        isActive = true;
        StartCoroutine(ActivateShadowsSequentially());

        Debug.Log("ShadowExit: Активируем тени!");
    }

    private IEnumerator ActivateShadowsSequentially()
    {
        foreach (ShadowData data in shadowDataList)
        {
            if (data.shadowObject != null)
            {
                ActivateShadow(data);
                PlayRandomSound(appearSounds, data.shadowObject.transform.position);
                yield return new WaitForSeconds(Random.Range(0.5f, activationDelay));
            }
        }
    }

    private void ActivateShadow(ShadowData data)
    {
        data.isActive = true;
        data.isVisible = true;

        data.shadowObject.transform.position = data.originalPosition;
        data.shadowObject.transform.rotation = data.originalRotation;

        SetShadowVisibility(data, true);

        Debug.Log($"ShadowExit: Тень активирована - {data.shadowObject.name}");
    }

    void Update()
    {
        if (!isActive || player == null) return;

        // Сначала вычисляем силы избегания для всех теней
        CalculateAvoidanceForces();

        // Затем обновляем движение с учетом avoidance
        foreach (ShadowData data in shadowDataList)
        {
            if (data.isActive && data.shadowObject != null)
            {
                UpdateShadowMovement(data);

                if (Random.value < 0.02f && data.isVisible)
                {
                    StartCoroutine(QuickBlink(data));
                }
            }
        }
    }

    private void CalculateAvoidanceForces()
    {
        // Сбрасываем силы избегания
        foreach (ShadowData data in shadowDataList)
        {
            data.avoidanceForce = Vector3.zero;
        }

        // Вычисляем силы отталкивания между всеми парами теней
        for (int i = 0; i < shadowDataList.Count; i++)
        {
            ShadowData dataA = shadowDataList[i];
            if (!dataA.isActive || dataA.shadowObject == null) continue;

            Vector3 posA = dataA.shadowObject.transform.position;

            for (int j = i + 1; j < shadowDataList.Count; j++)
            {
                ShadowData dataB = shadowDataList[j];
                if (!dataB.isActive || dataB.shadowObject == null) continue;

                Vector3 posB = dataB.shadowObject.transform.position;
                float distance = Vector3.Distance(posA, posB);

                // Если тени слишком близко - добавляем силу отталкивания
                if (distance < separationDistance && distance > 0.1f)
                {
                    Vector3 directionAtoB = (posB - posA).normalized;
                    float forceStrength = 1f - (distance / separationDistance); // Сильнее когда ближе

                    // Сила отталкивания (противоположные направления)
                    dataA.avoidanceForce -= directionAtoB * forceStrength * 2f;
                    dataB.avoidanceForce += directionAtoB * forceStrength * 2f;
                }
            }
        }
    }

    private void UpdateShadowMovement(ShadowData data)
    {
        Transform shadowTransform = data.shadowObject.transform;
        Vector3 playerPosition = player.position;

        Vector3 currentPos = shadowTransform.position;
        currentPos.y = data.originalPosition.y;

        Vector2 shadowPos2D = new Vector2(currentPos.x, currentPos.z);
        Vector2 playerPos2D = new Vector2(playerPosition.x, playerPosition.z);
        float distanceToPlayer = Vector2.Distance(shadowPos2D, playerPos2D);

        if (distanceToPlayer > minDistance)
        {
            Vector3 directionToPlayer = (playerPosition - currentPos).normalized;
            Vector3 targetPosition = playerPosition - directionToPlayer * minDistance;
            targetPosition.y = data.originalPosition.y;

            // ДОБАВЛЯЕМ СИЛУ ИЗБЕГАНИЯ К ЦЕЛЕВОЙ ПОЗИЦИИ
            Vector3 avoidanceOffset = data.avoidanceForce * 0.5f;
            avoidanceOffset.y = 0; // Только по горизонтали
            targetPosition += avoidanceOffset;

            // Ограничиваем максимальное смещение от игрока
            float maxOffsetFromPlayer = 2f;
            Vector3 playerToTarget = targetPosition - playerPosition;
            playerToTarget.y = 0;
            if (playerToTarget.magnitude > maxOffsetFromPlayer)
            {
                targetPosition = playerPosition + playerToTarget.normalized * maxOffsetFromPlayer;
                targetPosition.y = data.originalPosition.y;
            }

            currentPos = Vector3.MoveTowards(
                currentPos,
                targetPosition,
                approachSpeed * Time.deltaTime
            );

            shadowTransform.position = currentPos;

            // Поворачиваем к игроку (игнорируя смещение от избегания)
            Vector3 lookDirection = playerPosition - currentPos;
            lookDirection.y = 0;
            if (lookDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
                Vector3 euler = targetRotation.eulerAngles;
                euler.x = data.originalRotation.eulerAngles.x;
                euler.z = data.originalRotation.eulerAngles.z;
                shadowTransform.rotation = Quaternion.Euler(euler);
            }
        }
    }

    private IEnumerator QuickBlink(ShadowData data)
    {
        if (data.shadowObject == null || !data.isActive) yield break;

        SetShadowVisibility(data, false);
        yield return new WaitForSeconds(0.1f);

        if (data.shadowObject == null || !data.isActive) yield break;
        SetShadowVisibility(data, true);
    }

    private void SetShadowVisibility(ShadowData data, bool visible)
    {
        data.isVisible = visible;
        foreach (Renderer renderer in data.renderers)
        {
            if (renderer != null)
                renderer.enabled = visible;
        }
    }

    private void PlayRandomSound(AudioClip[] clips, Vector3 position = default)
    {
        if (clips.Length == 0) return;

        AudioClip clip = clips[Random.Range(0, clips.Length)];
        AudioSource.PlayClipAtPoint(clip, position, 0.2f);
    }

    public void StopShadows()
    {
        isActive = false;

        foreach (ShadowData data in shadowDataList)
        {
            if (data.shadowObject != null)
            {
                data.isActive = false;
                data.isVisible = false;
                SetShadowVisibility(data, false);
                data.shadowObject.transform.position = data.originalPosition;
                data.shadowObject.transform.rotation = data.originalRotation;
            }
        }
    }

    public float GetClosestShadowDistance()
    {
        if (!isActive || player == null) return Mathf.Infinity;

        float closestDistance = Mathf.Infinity;

        foreach (ShadowData data in shadowDataList)
        {
            if (data.isActive && data.shadowObject != null)
            {
                float distance = Vector3.Distance(data.shadowObject.transform.position, player.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                }
            }
        }

        return closestDistance;
    }

    void OnDestroy()
    {
        StopShadows();
    }
}