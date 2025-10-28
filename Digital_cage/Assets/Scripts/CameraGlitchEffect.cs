using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraGlitchEffect : MonoBehaviour
{
    [Header("Glitch Settings")]
    public float glitchDuration = 4f;
    public float maxShakeIntensity = 0.25f;
    public float shakeSpeed = 25f;
    public float rotationIntensity = 2f;
    public bool randomJitter = true;

    [Header("Color Glitch")]
    public Material glitchMaterial; // перетащи сюда GlitchRGBMat
    public float maxColorOffset = 0.02f; // максимальный сдвиг RGB

    [Header("Audio")]
    public AudioClip glitchAudio; // Аудио для глитч-эффекта
    public float audioStopTime = 3.8f; // Когда остановить аудио (до конца глитча)
    [Range(0f, 1f)]
    public float audioVolume = 1f; // ГРОМКОСТЬ от 0 до 1

    private bool glitchActive = false;
    private Vector3 originalPos;
    private Quaternion originalRot;
    private Camera cam;
    private AudioSource audioSource;

    void Start()
    {
        originalPos = transform.localPosition;
        originalRot = transform.localRotation;
        cam = GetComponent<Camera>();

        // Создаем AudioSource если его нет
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.loop = false;
    }

    public void StartGlitch()
    {
        if (!glitchActive)
            StartCoroutine(GlitchRoutine());
    }

    private IEnumerator GlitchRoutine()
    {
        glitchActive = true;
        float elapsed = 0f;

        // === ЗАПУСК АУДИО В НАЧАЛЕ ГЛИТЧА ===
        if (glitchAudio != null && audioSource != null)
        {
            audioSource.clip = glitchAudio;
            audioSource.volume = audioVolume; // Устанавливаем громкость
            audioSource.Play();
            Debug.Log($"?? Аудио глитча запущено (громкость: {audioVolume})");
        }

        while (elapsed < glitchDuration)
        {
            elapsed += Time.deltaTime;

            // === ОСТАНОВКА АУДИО ЗАРАНЕЕ (если нужно) ===
            if (elapsed >= audioStopTime && audioSource != null && audioSource.isPlaying)
            {
                audioSource.Stop();
                Debug.Log("?? Аудио глитча остановлено досрочно");
            }

            // t растёт медленно сначала, резко под конец
            float t = Mathf.Pow(elapsed / glitchDuration, 2.5f);

            // Дрожание камеры только по X/Z
            Vector3 shakeOffset = new Vector3(
                Random.Range(-1f, 1f) * maxShakeIntensity * t,
                0f,
                Random.Range(-1f, 1f) * maxShakeIntensity * t
            );

            // Левое-правое колебание
            shakeOffset.x += Mathf.Sin(Time.time * shakeSpeed) * 0.05f * t;
            shakeOffset.z += Mathf.Cos(Time.time * shakeSpeed * 1.1f) * 0.03f * t;

            // Резкие случайные рывки
            if (randomJitter && Random.value < 0.05f)
            {
                Vector3 jitter = new Vector3(
                    Random.Range(-1f, 1f) * maxShakeIntensity * 2f * t,
                    0f,
                    Random.Range(-1f, 1f) * maxShakeIntensity * 2f * t
                );
                shakeOffset += jitter;
            }

            Vector3 pos = originalPos + shakeOffset;
            pos.y = 1.4f; // фиксируем высоту
            transform.localPosition = pos;

            // Поворот камеры
            float rotX = Mathf.Sin(Time.time * shakeSpeed) * rotationIntensity * t;
            float rotY = Mathf.Cos(Time.time * shakeSpeed * 1.1f) * rotationIntensity * t;
            transform.localRotation = Quaternion.Euler(originalRot.eulerAngles + new Vector3(rotX, rotY, 0));

            // Цветной глитч через материал
            if (glitchMaterial != null)
            {
                glitchMaterial.SetVector("_Offset", new Vector4(
                    Random.Range(-maxColorOffset, maxColorOffset) * t,
                    Random.Range(-maxColorOffset, maxColorOffset) * t,
                    Random.Range(-maxColorOffset, maxColorOffset) * t,
                    Random.Range(-maxColorOffset, maxColorOffset) * t
                ));
            }

            yield return null;
        }

        // === ГАРАНТИРОВАННАЯ ОСТАНОВКА АУДИО В КОНЦЕ ===
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
            Debug.Log("?? Аудио глитча остановлено в конце");
        }

        transform.localPosition = originalPos;
        transform.localRotation = originalRot;

        // Сбрасываем цвет
        if (glitchMaterial != null)
            glitchMaterial.SetVector("_Offset", Vector4.zero);

        glitchActive = false;
    }

    // Рисуем Fullscreen эффект
    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        if (glitchMaterial != null && glitchActive)
            Graphics.Blit(src, dest, glitchMaterial);
        else
            Graphics.Blit(src, dest);
    }

    // Метод для принудительной остановки глитча и аудио
    public void StopGlitch()
    {
        StopAllCoroutines();
        glitchActive = false;

        if (audioSource != null && audioSource.isPlaying)
            audioSource.Stop();

        transform.localPosition = originalPos;
        transform.localRotation = originalRot;

        if (glitchMaterial != null)
            glitchMaterial.SetVector("_Offset", Vector4.zero);
    }

    // Метод для изменения громкости во время выполнения
    public void SetVolume(float volume)
    {
        audioVolume = Mathf.Clamp01(volume);
        if (audioSource != null)
        {
            audioSource.volume = audioVolume;
        }
    }
}