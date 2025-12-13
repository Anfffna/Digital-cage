using UnityEngine;
using System.Collections;
using System.Collections.Generic; // ? ДОБАВЬ ЭТУ СТРОКУ
using TMPro;

public class CerealBox : MonoBehaviour, IInteractable
{
    [Header("=== НАСТРОЙКИ ДИАЛОГА ===")]
    public ManagerDialogue2 dialogueManager;
    [TextArea(2, 5)]
    public List<string> cerealBoxDialogueTexts;
    public int bloodEffectDialogueIndex = 2; // На каком индексе диалога запускать эффекты крови

    [Header("=== НАСТРОЙКИ КРОВАВОГО ЭФФЕКТА ===")]
    //public GameObject bloodStainPrefab;
    //public LineRenderer bloodStreamPrefab;
    public Transform[] bloodFlowPoints;

    public TodoUIManager todoManager;
    public int requiredTodoIndex = 0;

    [Header("=== СИСТЕМА ЧАСТИЦ КРОВИ ===")]
    public ParticleSystem bloodParticleSystem; // Система частиц для начального выброса
    public float particleDuration = 1.5f; // Длительность работы частиц

    [Header("=== НАСТРОЙКИ СТРУЙ КРОВИ ===")]
    public float streamDuration = 4f;
    public float streamWidth = 0.02f;
    public float minStreamWidth = 0.015f;  // ? ДОБАВЬ
    public float maxStreamWidth = 0.025f;
    public Color bloodColor = Color.red;
    public int stainsPerStream = 10;
    public float bloodSpawnRadius = 0.5f;
    public float stainSpawnDelay = 0.2f;

    [Header("=== НАСТРОЙКИ СМЕНЫ МАТЕРИАЛА ===")]
    public Material bloodMaterial;          // Красный материал крови
    public float materialChangeDuration = 2f; // Время смены материала в секундах
    public Transform[] cubesToChange;       // Массив кубов для изменения

    [Header("=== НАСТРОЙКИ ЛУЖ КРОВИ ===")]
    public int totalBloodStains = 15;
    public float stainFadeDuration = 10f;

    private bool isInteractable = false;
    private bool hasBeenUsed = false;
    private bool interactionTriggered = false;
    private bool dialogueTriggered = false; // ДОБАВЛЕНО: недостающая переменная

    void Start()
    {
        gameObject.layer = LayerMask.NameToLayer("Interactable");
        SetInteractable(false);
        StartCoroutine(CheckTodoCompletion());
    }

    /// <summary>
    /// Постоянно проверяет выполнение условий для активации
    /// </summary>
    private IEnumerator CheckTodoCompletion()
    {
        while (todoManager == null)
        {
            yield return new WaitForSeconds(0.5f);
            todoManager = FindObjectOfType<TodoUIManager>();
            Debug.Log("CerealBox: Ищу TodoUIManager...");
        }

        while (!isInteractable && !hasBeenUsed)
        {
            if (todoManager != null)
            {
                if (IsTodoItemCompleted(requiredTodoIndex))
                {
                    SetInteractable(true);
                    Debug.Log($"CerealBox: Объект теперь интерактивен! Задача {requiredTodoIndex} выполнена.");
                    break;
                }
            }
            yield return new WaitForSeconds(0.3f);
        }
    }

    /// <summary>
    /// Проверяет зачеркнут ли пункт в туду листе
    /// </summary>
    private bool IsTodoItemCompleted(int index)
    {
        if (todoManager == null || todoManager.todoItems == null)
        {
            Debug.LogError("CerealBox: TodoManager или todoItems не назначены!");
            return false;
        }

        if (index < 0 || index >= todoManager.todoItems.Length)
        {
            Debug.LogError($"CerealBox: Неверный индекс туду: {index}");
            return false;
        }

        TMPro.TextMeshProUGUI todoItem = todoManager.todoItems[index];
        bool isCompleted = todoItem.text.StartsWith("<s>");

        if (isCompleted)
            Debug.Log($"CerealBox: Задача {index} выполнена!");

        return isCompleted;
    }

    public string GetInteractionText()
    {
        if (!isInteractable || hasBeenUsed)
            return ""; // Пустой текст = не показывать подсказку

        return "Нажмите E";
    }

    public void Interact()
    {
        if (!isInteractable || hasBeenUsed || interactionTriggered || dialogueTriggered) return;

        dialogueTriggered = true;
        SetInteractable(false);
        hasBeenUsed = true;

        Debug.Log("CerealBox: Начинаем диалог!");

        if (dialogueManager != null && cerealBoxDialogueTexts != null && cerealBoxDialogueTexts.Count > 0)
        {
            // ПОДПИСЫВАЕМСЯ НА СОБЫТИЕ ИЗМЕНЕНИЯ ИНДЕКСА
            dialogueManager.OnDialogueIndexReached += OnDialogueIndexChanged;

            dialogueManager.StartDialogue(cerealBoxDialogueTexts, OnDialogueEnd);
        }
    }

    /// <summary>
    /// Вызывается когда диалог переходит на новую реплику
    /// </summary>
    private void OnDialogueIndexChanged(int currentIndex)
    {
        Debug.Log($"CerealBox: Текущая реплика диалога: {currentIndex}");

        // Запускаем эффекты крови на второй реплике (индекс 2)
        if (currentIndex == bloodEffectDialogueIndex)
        {
            Debug.Log($"CerealBox: Достигли реплики {bloodEffectDialogueIndex}, запускаем эффекты крови!");
            StartBloodEffects();

            // ОТПИСЫВАЕМСЯ ОТ СОБЫТИЯ чтобы не запускать повторно
            dialogueManager.OnDialogueIndexReached -= OnDialogueIndexChanged;
        }
    }

    /// <summary>
    /// Вызывается когда диалог полностью завершен
    /// </summary>
    private void OnDialogueEnd()
    {
        // ОТПИСЫВАЕМСЯ ОТ СОБЫТИЯ при завершении диалога
        if (dialogueManager != null)
        {
            dialogueManager.OnDialogueIndexReached -= OnDialogueIndexChanged;
        }

        // ДОБАВЛЕНО: Зачеркиваем пункт 2 в туду после завершения диалога
        if (todoManager != null)
        {
            todoManager.CompleteTodoItem(2);
            Debug.Log("CerealBox: Диалог завершен, зачеркиваем пункт 2 в туду");
        }

        dialogueTriggered = false;
        Debug.Log("CerealBox: Диалог завершен");
    }

    /// <summary>
    /// Главный корутин управления всеми эффектами крови
    /// </summary>
    private IEnumerator BloodEffectRoutine()
    {
        // ? ДОБАВЬ ЭТОТ ВЫЗОВ - Запускаем смену материала кубов
        StartCoroutine(ChangeCubesMaterial());

        // ? ДОБАВЬ ЭТОТ БЛОК - Запускаем систему частиц для начального выброса крови
        if (bloodParticleSystem != null)
        {
            bloodParticleSystem.Play();

            // Автоматически останавливаем частицы через заданное время
            StartCoroutine(StopParticlesAfterDelay());
        }

        // Запускаем струи крови из всех точек
        if (bloodFlowPoints != null && bloodFlowPoints.Length > 0)
        {
            foreach (Transform flowPoint in bloodFlowPoints)
            {
                if (flowPoint != null)
                {
                    StartCoroutine(SingleBloodStreamRoutine(flowPoint));
                    Debug.Log($"CerealBox: Запущена струйка из точки {flowPoint.name}");
                }
            }
        }

        // Создаем дополнительные лужи крови
        yield return StartCoroutine(SpawnAdditionalBloodStains());

        // Ждем завершения основных струй
        yield return new WaitForSeconds(streamDuration);

        Debug.Log("CerealBox: Кровавый эффект завершен!");
        interactionTriggered = false;
    }

    /// <summary>
    /// ДОБАВЛЕНО: Метод для запуска эффектов крови
    /// </summary>
    private void StartBloodEffects()
    {
        Debug.Log("CerealBox: Запускаем все эффекты крови!");
        StartCoroutine(BloodEffectRoutine());
    }

    /// <summary>
    /// Плавно меняет материал на дочерних кубах
    /// </summary>
    private IEnumerator ChangeCubesMaterial()
    {
        if (bloodMaterial == null)
        {
            Debug.LogError("CerealBox: Blood Material не назначен!");
            yield break;
        }

        if (cubesToChange == null || cubesToChange.Length == 0)
        {
            // Автоматически находим кубы по именам
            cubesToChange = new Transform[]
            {
            transform.Find("Cube"),
            transform.Find("Cube (1)"),
            transform.Find("Cube (2)"),
            transform.Find("Cube (3)")
            };
        }

        Debug.Log($"CerealBox: Начинаем смену материала на {cubesToChange.Length} кубах");

        // Собираем оригинальные материалы
        Renderer[] renderers = new Renderer[cubesToChange.Length];
        Material[] originalMaterials = new Material[cubesToChange.Length];

        for (int i = 0; i < cubesToChange.Length; i++)
        {
            if (cubesToChange[i] != null)
            {
                renderers[i] = cubesToChange[i].GetComponent<Renderer>();
                if (renderers[i] != null)
                {
                    originalMaterials[i] = renderers[i].material;
                }
            }
        }

        // Плавная смена материала
        float timer = 0f;
        while (timer < materialChangeDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / materialChangeDuration;

            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null)
                {
                    // Плавно интерполируем между оригинальным и кровавым материалом
                    renderers[i].material.Lerp(originalMaterials[i], bloodMaterial, progress);
                }
            }

            yield return null;
        }

        // Убеждаемся что в конце точно bloodMaterial
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
            {
                renderers[i].material = bloodMaterial;
            }
        }

        Debug.Log("CerealBox: Смена материала завершена");
    }

    /// <summary>
    /// Создает LineRenderer для струи крови через код
    /// </summary>
    private LineRenderer CreateBloodStream(Transform parent)
    {
        // Создаем объект для струи
        GameObject streamObject = new GameObject("BloodStream");
        streamObject.transform.SetParent(parent);
        streamObject.transform.localPosition = Vector3.zero;

        // Добавляем LineRenderer
        LineRenderer lineRenderer = streamObject.AddComponent<LineRenderer>();

        // Настраиваем LineRenderer
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, Vector3.zero); // Начало (верх)
        lineRenderer.SetPosition(1, Vector3.down * 0.5f); // Конец (вниз на 0.5 единиц)

        // ? ПРОСТАЯ НАСТРОЙКА БЕЗ ГРАДИЕНТА
        lineRenderer.startWidth = streamWidth;
        lineRenderer.endWidth = streamWidth;
        lineRenderer.startColor = bloodColor;
        lineRenderer.endColor = bloodColor;
        lineRenderer.useWorldSpace = false;

        lineRenderer.numCapVertices = 5; // Количество вершин для скругления концов (1-10)
        lineRenderer.alignment = LineAlignment.TransformZ; // Выравнивание по трансформу

        // Настраиваем материал
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.material.color = bloodColor;

        return lineRenderer;
    }

    /// <summary>
    /// Останавливает систему частиц через заданное время
    /// </summary>
    private IEnumerator StopParticlesAfterDelay()
    {
        yield return new WaitForSeconds(particleDuration);

        if (bloodParticleSystem != null)
        {
            bloodParticleSystem.Stop();
            Debug.Log("CerealBox: Система частиц остановлена");
        }
    }

    private IEnumerator SingleBloodStreamRoutine(Transform startPoint)
    {
        LineRenderer bloodStream = CreateBloodStream(startPoint);

        // ? ДОБАВЬ СЛУЧАЙНУЮ ТОЛЩИНУ
        float randomWidth = Random.Range(minStreamWidth, maxStreamWidth);
        bloodStream.startWidth = randomWidth;
        bloodStream.endWidth = randomWidth;

        float timer = 0f;
        int stainsCreated = 0;
        float maxStreamLength = 0.6f;

        while (timer < streamDuration && stainsCreated < stainsPerStream)
        {
            timer += Time.deltaTime;

            float streamProgress = Mathf.Clamp01(timer / 1f);
            float currentLength = maxStreamLength * streamProgress;

            bloodStream.SetPosition(0, Vector3.zero);
            bloodStream.SetPosition(1, Vector3.down * currentLength);

            if (currentLength > 0.1f && timer % 0.4f < Time.deltaTime)
            {
                Vector3 endPos = startPoint.position + Vector3.down * currentLength;
                CreateBloodStainUnderStream(endPos);
                stainsCreated++;
            }

            yield return null;
        }

        //yield return new WaitForSeconds(0.5f);
        //yield return StartCoroutine(FadeOutStream(bloodStream));
        //Destroy(bloodStream.gameObject);
    }

    /// <summary>
    /// Создает пятно крови на поверхности под струей
    /// </summary>
    private void CreateBloodStainUnderStream(Vector3 position)
    {
        // Находим спрайт крови в сцене по имени
        GameObject bloodSprite = GameObject.Find("blood"); // имя твоего спрайта

        if (bloodSprite != null)
        {
            RaycastHit hit;
            if (Physics.Raycast(position + Vector3.up * 0.1f, Vector3.down, out hit, 1f))
            {
                // Создаем копию спрайта
                GameObject stain = Instantiate(bloodSprite, hit.point + Vector3.up * 0.01f, Quaternion.identity);

                // Добавляем скрипт BloodStain на КОПИЮ
                BloodStain bloodStain = stain.AddComponent<BloodStain>();

                // Настраиваем параметры
                bloodStain.fadeDuration = stainFadeDuration;
                bloodStain.growDuration = 2f;
                bloodStain.startDelay = 0.5f;

                Debug.Log("Создано пятно крови со скриптом");
            }
        }
        else
        {
            Debug.LogError("Не найден спрайт крови в сцене! Убедись что есть объект с именем 'blood'");
        }
    }

    /// <summary>
    /// Создает дополнительные лужи крови вокруг упаковки
    /// </summary>
    private IEnumerator SpawnAdditionalBloodStains()
    {
        Debug.Log($"CerealBox: Создаем {totalBloodStains} дополнительных пятен...");

        for (int i = 0; i < totalBloodStains; i++)
        {
            SpawnSingleBloodStain();
            yield return new WaitForSeconds(stainSpawnDelay);
        }

        Debug.Log("CerealBox: Все дополнительные пятна созданы");
    }

    /// <summary>
    /// Создает одно случайное пятно крови вокруг упаковки
    /// </summary>
    private void SpawnSingleBloodStain()
    {
        // Находим спрайт крови в сцене по имени
        GameObject bloodSprite = GameObject.Find("blood");

        if (bloodSprite == null)
        {
            Debug.LogError("CerealBox: Не найден спрайт 'blood' в сцене!");
            return;
        }

        Vector2 randomCircle = Random.insideUnitCircle * bloodSpawnRadius;
        Vector3 stainPosition = transform.position +
                               new Vector3(randomCircle.x, 0.02f, randomCircle.y);

        // Проверяем что пятно будет на поверхности
        RaycastHit hit;
        if (Physics.Raycast(stainPosition + Vector3.up * 0.2f, Vector3.down, out hit, 1f))
        {
            GameObject stain = Instantiate(bloodSprite, hit.point + Vector3.up * 0.01f, Quaternion.identity);

            // Добавляем скрипт BloodStain на КОПИЮ
            BloodStain bloodStain = stain.AddComponent<BloodStain>();
            bloodStain.fadeDuration = stainFadeDuration;
            bloodStain.growDuration = 2f;
            bloodStain.startDelay = 0.5f;
        }
    }

    /// <summary>
    /// Управляет состоянием интерактивности объекта
    /// </summary>
    private void SetInteractable(bool interactable)
    {
        isInteractable = interactable;
        gameObject.layer = interactable ? LayerMask.NameToLayer("Interactable") : LayerMask.NameToLayer("Default");
        Debug.Log($"CerealBox: Interactable = {interactable}");
    }

    void OnDestroy()
    {
        StopAllCoroutines();
    }

    // ========== ВИЗУАЛЬНАЯ ОТЛАДКА ==========

    void OnDrawGizmos()
    {
        if (isInteractable && !hasBeenUsed)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(transform.position, Vector3.one * 0.3f);
        }
        else if (hasBeenUsed)
        {
            Gizmos.color = Color.gray;
            Gizmos.DrawWireCube(transform.position, Vector3.one * 0.25f);
        }
        else
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(transform.position, Vector3.one * 0.2f);
        }
    }

    void OnDrawGizmosSelected()
    {
        // Радиус создания дополнительных пятен
        Gizmos.color = new Color(1, 0, 0, 0.3f);
        Gizmos.DrawWireSphere(transform.position, bloodSpawnRadius);

        // Точки струй крови
        if (bloodFlowPoints != null)
        {
            Gizmos.color = Color.red;
            foreach (Transform point in bloodFlowPoints)
            {
                if (point != null)
                {
                    Gizmos.DrawWireSphere(point.position, 0.01f);
                    Gizmos.DrawLine(point.position, point.position + Vector3.down * 0.3f);
                }
            }
        }
    }
}