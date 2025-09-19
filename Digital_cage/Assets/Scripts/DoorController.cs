private Animator animator;

void Start()
{
    // Правильно ищем Animator среди дочерних объектов
    animator = GetComponentInChildren<Animator>();

    // Добавляем проверку для отладки
    if (animator == null)
    {
        Debug.LogError("Animator не найден среди дочерних объектов! Проверь, что он есть на модели двери.", this);
    }
}

void Update()
{
    if (Input.GetKeyDown(KeyCode.E))
    {
        // Проверяем, что animator найден, перед использованием
        if (animator != null)
        {
            animator.SetTrigger("OpenDoor");
        }
    }
}
