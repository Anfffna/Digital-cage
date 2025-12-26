using UnityEngine;

public class Windows : MonoBehaviour
{
    [Header("Materials")]
    public Material whiteWireMaterial; // Начальный материал (белая сетка)
    public Material windowsMaterial;   // Конечный материал (окна)

    [Header("Settings")]
    public float changeDelay = 2f; // Задержка перед сменой материала

    private MeshRenderer meshRenderer;
    private bool materialChanged = false;
    private SitWork sitWorkScript;

    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer == null)
        {
            Debug.LogError("Windows: На объекте нет MeshRenderer!");
            return;
        }

        // Находим скрипт SitWork в сцене
        sitWorkScript = FindObjectOfType<SitWork>();
        if (sitWorkScript != null)
        {
            // Подписываемся на событие, если оно есть в SitWork
            var sitWork = sitWorkScript as MonoBehaviour;
            if (sitWork != null)
            {
                // Попробуем получить событие через рефлексию если оно есть
                System.Reflection.FieldInfo field = sitWork.GetType().GetField("OnPlayerSatDown");
                if (field != null)
                {
                    System.Action action = field.GetValue(sitWork) as System.Action;
                    if (action != null)
                    {
                        action += OnPlayerSatDown;
                        field.SetValue(sitWork, action);
                    }
                }
            }
        }
        else
        {
            // Альтернативный способ - ищем по тегу
            GameObject playerChair = GameObject.FindGameObjectWithTag("PlayerChair");
            if (playerChair != null)
            {
                sitWorkScript = playerChair.GetComponent<SitWork>();
            }
        }

        // Устанавливаем начальный материал
        if (whiteWireMaterial != null)
        {
            meshRenderer.material = whiteWireMaterial;
        }
        else
        {
            Debug.LogWarning("Windows: Не назначен whiteWireMaterial! Используется материал по умолчанию.");
        }
    }

    void Update()
    {
        // Альтернативный способ отслеживания: проверяем свойство IsPlayerSitting
        if (!materialChanged && sitWorkScript != null && sitWorkScript.IsPlayerSitting)
        {
            StartCoroutine(ChangeMaterialAfterDelay());
        }
    }

    private System.Collections.IEnumerator ChangeMaterialAfterDelay()
    {
        materialChanged = true;

        yield return new WaitForSeconds(changeDelay);

        if (windowsMaterial != null && meshRenderer != null)
        {
            meshRenderer.material = windowsMaterial;
            Debug.Log("Windows: Материал изменен на 'windows'");
        }
        else if (windowsMaterial == null)
        {
            Debug.LogError("Windows: Не назначен windowsMaterial!");
        }
    }

    // Метод вызывается когда игрок садится (если подписка сработала)
    private void OnPlayerSatDown()
    {
        if (!materialChanged)
        {
            StartCoroutine(ChangeMaterialAfterDelay());
        }
    }

    // Публичный метод для принудительной смены материала
    public void ForceChangeMaterial()
    {
        if (!materialChanged)
        {
            StopAllCoroutines();
            StartCoroutine(ChangeMaterialAfterDelay());
        }
    }

    // Метод для сброса к начальному материалу
    public void ResetMaterial()
    {
        materialChanged = false;
        StopAllCoroutines();

        if (whiteWireMaterial != null && meshRenderer != null)
        {
            meshRenderer.material = whiteWireMaterial;
        }
    }

    // Для отладки в редакторе
    void OnValidate()
    {
        if (Application.isPlaying && meshRenderer != null && whiteWireMaterial != null)
        {
            meshRenderer.material = whiteWireMaterial;
        }
    }
}