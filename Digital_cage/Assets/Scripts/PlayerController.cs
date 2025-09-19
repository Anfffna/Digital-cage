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

    void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;

        if (playerCamera == null)
            playerCamera = GetComponentInChildren<Camera>().transform;

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
        HandleMouseLook();
        HandleMovement();
        HandleFootsteps();
    }

    void HandleGravityAndGrounded()
    {
        isGrounded = controller.isGrounded;

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        velocity.y += gravity * Time.deltaTime;
    }

    void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    void HandleMovement()
    {
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
        if (controller.velocity.magnitude > 0.2f && isGrounded)
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
            }
        }
    }
} 