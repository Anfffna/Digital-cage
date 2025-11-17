using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 5f;
    public float runSpeed = 8f;
    public float jumpHeight = 1.5f;
    public float gravity = -9.81f;
    public KeyCode jumpKey = KeyCode.Space;

    [Header("Look Settings")]
    public float mouseSensitivity = 100f;
    public Transform playerCamera;

    [Header("Footstep Sounds - WALK")]
    public AudioClip[] walkFootstepSounds;
    public float footstepDelayWalk = 0.5f;
    [Range(0.1f, 2.0f)] public float walkVolume = 1.0f;

    [Header("Footstep Sounds - RUN")]
    public AudioClip[] runFootstepSounds;
    public float footstepDelayRun = 0.3f;
    [Range(0.1f, 2.0f)] public float runVolume = 1.2f;

    [Header("Audio Source")]
    public AudioSource footstepAudioSource;

    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;
    private float xRotation = 0f;
    private float nextFootstepTime;

    [HideInInspector]
    public bool restrictHorizontalLook = false; // ограничение Y при сидении
    public float minSitY = -106f;
    public float maxSitY = 74f;
    public bool restrictVerticalLook = false; // ограничение X при сидении
    public float minSitX = -5f;  // минимальный угол X при сидении
    public float maxSitX = 15f;  // максимальный угол X при сидении
    public bool canMove = true; // флаг, блокирующий движение, но камера остаётся активной
    public bool lockVerticalLook = false; // если true — нельзя крутить камеру по X
    void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;

        if (playerCamera == null)
        {
            playerCamera = GetComponentInChildren<Camera>().transform;
        }

        if (footstepAudioSource != null)
        {
            footstepAudioSource.spatialBlend = 1f;
            footstepAudioSource.maxDistance = 25f;
            footstepAudioSource.rolloffMode = AudioRolloffMode.Logarithmic;
            footstepAudioSource.minDistance = 1.5f;
            footstepAudioSource.playOnAwake = false;
            footstepAudioSource.loop = false;
        }
    }

    void Update()
    {
        HandleGravityAndGrounded();
        HandleMouseLook();   // Камера работает всегда
        HandleMovement();    // Движение может быть заблокировано
        HandleFootsteps();
    }

    void HandleGravityAndGrounded()
    {
        if (!canMove)  // если сидим, гравитация не нужна
        {
            velocity = Vector3.zero;
            return;
        }

        isGrounded = controller.isGrounded;

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -1.8f;
        }

        velocity.y += gravity * Time.deltaTime;
    }


    void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // вертикаль
        if (restrictVerticalLook)
            xRotation = Mathf.Clamp(xRotation - mouseY, minSitX, maxSitX);
        else
            xRotation = Mathf.Clamp(xRotation - mouseY, -90f, 90f);

        // горизонталь
        float yRotation = mouseX;
        if (restrictHorizontalLook)
        {
            float currentY = transform.eulerAngles.y;
            float targetY = currentY + yRotation;

            // преобразуем в -180..180 для корректного ограничения
            if (targetY > 180f) targetY -= 360f;

            targetY = Mathf.Clamp(targetY, minSitY, maxSitY);
            transform.rotation = Quaternion.Euler(transform.eulerAngles.x, targetY, transform.eulerAngles.z);
        }
        else
        {
            transform.Rotate(Vector3.up * yRotation);
        }

        playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }




    void HandleMovement()
    {
        if (!canMove) return; // если игрок сидит, движение блокируется, камера работает

        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        float currentSpeed = isRunning ? runSpeed : walkSpeed;

        Vector3 move = (transform.right * horizontal + transform.forward * vertical).normalized;
        controller.Move(move * currentSpeed * Time.deltaTime);

        if (Input.GetKeyDown(jumpKey) && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        controller.Move(velocity * Time.deltaTime);
    }

    void HandleFootsteps()
    {
        if (!canMove)
        {
            if (footstepAudioSource.isPlaying) footstepAudioSource.Stop();
            return;
        }

        bool isMoving = (Mathf.Abs(Input.GetAxis("Horizontal")) > 0.1f || Mathf.Abs(Input.GetAxis("Vertical")) > 0.1f) && isGrounded;

        if (isMoving)
        {
            bool isRunning = Input.GetKey(KeyCode.LeftShift);
            AudioClip[] currentSounds = isRunning ? runFootstepSounds : walkFootstepSounds;
            float currentDelay = isRunning ? footstepDelayRun : footstepDelayWalk;
            float currentVolume = isRunning ? runVolume : walkVolume;

            if (currentSounds != null && currentSounds.Length > 0 && Time.time > nextFootstepTime)
            {
                AudioClip selectedClip = currentSounds[Random.Range(0, currentSounds.Length)];
                footstepAudioSource.clip = selectedClip;
                footstepAudioSource.volume = currentVolume;
                footstepAudioSource.pitch = Random.Range(0.8f, 1.2f);
                footstepAudioSource.Play();

                nextFootstepTime = Time.time + currentDelay;

                Debug.Log("Step played: " + selectedClip.name); // проверка
            }
        }
        else
        {
            if (footstepAudioSource.isPlaying)
                footstepAudioSource.Stop();
        }
    }

    public void SetMovementEnabled(bool enabled)
    {
        canMove = enabled;

        if (!enabled)
        {
            // Останавливаем звуки шагов при блокировке движения
            if (footstepAudioSource != null && footstepAudioSource.isPlaying)
            {
                footstepAudioSource.Stop();
            }

            // Сбрасываем velocity чтобы игрок не продолжал движение по инерции
            velocity = Vector3.zero;
        }

        Debug.Log("PlayerController: Движение " + (enabled ? "разблокировано" : "заблокировано"));
    }

    // ДОБАВЛЕННЫЕ МЕТОДЫ ДЛЯ ВЗАИМОДЕЙСТВИЯ СО СТУЛОМ
    public void OnSitDown()
{
    canMove = false;
    velocity = Vector3.zero;

    if (footstepAudioSource != null && footstepAudioSource.isPlaying)
    {
        footstepAudioSource.Stop();
    }

    Debug.Log("Игрок сел на стул - движение заблокировано");
}

    public void OnStandUp()
    {
        canMove = true;
        velocity = Vector3.zero; // <---- сбросить накопленную скорость, чтобы не подлетать
        Debug.Log("Игрок встал со стула - движение разблокировано");
    }

    /// <summary>
    /// Жёстко сбрасывает только вертикальную скорость,
    /// чтобы при вставании не было подлётов или рывков.
    /// </summary>
    public void ResetVerticalVelocity()
{
    velocity.y = -2f; // значение как при grounded в Unity
}


    // ======= ПУБЛИЧНОЕ СВОЙСТВО ДЛЯ УПРАВЛЕНИЯ ВЕРТИКАЛЬНЫМ УГЛОМ КАМЕРЫ =======
    public float CameraXRotation
    {
        get { return xRotation; }
        set { xRotation = value; }
    }
}
