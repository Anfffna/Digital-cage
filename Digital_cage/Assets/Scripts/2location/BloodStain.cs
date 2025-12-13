using UnityEngine;

public class BloodStain : MonoBehaviour
{
    [Header("Настройки эффекта")]
    public float fadeDuration = 10f;
    public float growDuration = 2f;
    public float startDelay = 0.8f;

    // Настройки размера прямо в коде
    private float startSize = 0.009f;
    private float endSize = 0.02f;

    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private float timer = 0f;
    private bool hasStarted = false;

    void Start()
    {
        // Получаем SpriteRenderer
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
            // Сразу делаем полностью прозрачным
            Color transparentColor = originalColor;
            transparentColor.a = 0f;
            spriteRenderer.color = transparentColor;
        }

        // Начинаем с очень маленького размера (скрыто)
        transform.localScale = Vector3.one * startSize;

        // Случайный поворот
        transform.Rotate(90, 0, Random.Range(0, 360));

        Debug.Log("BloodStain: Инициализирован, ждем задержку " + startDelay + " сек");
    }

    void Update()
    {
        timer += Time.deltaTime;

        // Ждем задержку перед началом анимации
        if (!hasStarted && timer >= startDelay)
        {
            hasStarted = true;
            timer = 0f; // Сбрасываем таймер для анимации
            Debug.Log("BloodStain: Начинаем анимацию роста");
        }

        if (!hasStarted) return;

        // Анимация роста и появления
        if (timer < growDuration)
        {
            float growProgress = timer / growDuration;

            // Плавное увеличение размера от startSize до endSize
            float scale = Mathf.Lerp(startSize, endSize, growProgress);
            transform.localScale = Vector3.one * scale;

            // Плавное появление (альфа от 0 до 1)
            if (spriteRenderer != null)
            {
                Color color = originalColor;
                color.a = growProgress; // Альфа от 0 до 1
                spriteRenderer.color = color;
            }
        }
        // Анимация исчезновения
        //else if (timer < fadeDuration + growDuration)
        //{
        //    float fadeProgress = (timer - growDuration) / fadeDuration;
        //    if (spriteRenderer != null)
        //    {
        //        Color color = originalColor;
        //        color.a = 1f - fadeProgress; // Альфа от 1 до 0
        //        spriteRenderer.color = color;
        //    }
        //}
        //else
        //{
        //    Debug.Log("BloodStain: Эффект завершен, уничтожаем");
        //    Destroy(gameObject);
        //}
    }
}