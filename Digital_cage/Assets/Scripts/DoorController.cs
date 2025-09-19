private Animator animator;

void Start()
{
    // ��������� ���� Animator ����� �������� ��������
    animator = GetComponentInChildren<Animator>();

    // ��������� �������� ��� �������
    if (animator == null)
    {
        Debug.LogError("Animator �� ������ ����� �������� ��������! �������, ��� �� ���� �� ������ �����.", this);
    }
}

void Update()
{
    if (Input.GetKeyDown(KeyCode.E))
    {
        // ���������, ��� animator ������, ����� ��������������
        if (animator != null)
        {
            animator.SetTrigger("OpenDoor");
        }
    }
}
