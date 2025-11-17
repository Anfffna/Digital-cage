using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ApocalypseController : MonoBehaviour
{
    [Header("Light Settings")]
    public Light[] lightsToFlicker;
    public Color flickerColor = Color.red;
    public float minFlickerInterval = 0.1f;
    public float maxFlickerInterval = 0.5f;

    [Header("Object Shaking")]
    public Transform[] objectsToShake;
    public float maxShakeIntensity = 0.3f;
    public float shakeSpeed = 2f;

    [Header("Visual Effects")]
    public ParticleSystem[] dustParticles;
    public Renderer[] wallsToChange;
    public Material crackedWallMaterial;

    [Header("Audio")]
    public AudioSource[] ambientSounds;
    public AudioClip[] destructionSounds;
    public AudioSource mainAudioSource;

    [Header("Progression Settings")]
    public float warmUpDuration = 15f; // Увеличил до 15 секунд нарастание
    public float peakIntensity = 1f; // Пиковая интенсивность

    private Vector3[] originalPositions;
    private Color[] originalLightColors;
    private Material[] originalWallMaterials;
    private bool isApocalypseActive = false;
    private float apocalypseTimer = 0f;
    private Coroutine apocalypseCoroutine;
    private Coroutine infiniteEffectsCoroutine;

    void Start()
    {
        StoreOriginalStates();
    }

    private void StoreOriginalStates()
    {
        originalPositions = new Vector3[objectsToShake.Length];
        for (int i = 0; i < objectsToShake.Length; i++)
        {
            if (objectsToShake[i] != null)
                originalPositions[i] = objectsToShake[i].localPosition;
        }

        originalLightColors = new Color[lightsToFlicker.Length];
        for (int i = 0; i < lightsToFlicker.Length; i++)
        {
            if (lightsToFlicker[i] != null)
                originalLightColors[i] = lightsToFlicker[i].color;
        }

        originalWallMaterials = new Material[wallsToChange.Length];
        for (int i = 0; i < wallsToChange.Length; i++)
        {
            if (wallsToChange[i] != null)
                originalWallMaterials[i] = wallsToChange[i].material;
        }
    }

    public void StartApocalypse()
    {
        if (isApocalypseActive) return;

        isApocalypseActive = true;
        apocalypseTimer = 0f;

        if (apocalypseCoroutine != null)
            StopCoroutine(apocalypseCoroutine);

        apocalypseCoroutine = StartCoroutine(ApocalypseSequence());
        Debug.Log("Apocalypse: Начинается вечное разрушение!");
    }

    private IEnumerator ApocalypseSequence()
    {
        // Фаза 1: Постепенное нарастание за 15 секунд
        while (apocalypseTimer < warmUpDuration)
        {
            float progress = apocalypseTimer / warmUpDuration;
            UpdateEffects(progress, false);
            apocalypseTimer += Time.deltaTime;
            yield return null;
        }

        Debug.Log("Apocalypse: Достигнут пик интенсивности! Эффекты продолжаются бесконечно.");

        // Фаза 2: Бесконечное поддержание пиковой интенсивности
        if (infiniteEffectsCoroutine != null)
            StopCoroutine(infiniteEffectsCoroutine);

        infiniteEffectsCoroutine = StartCoroutine(InfinitePeakEffects());
    }

    private IEnumerator InfinitePeakEffects()
    {
        while (isApocalypseActive)
        {
            // Поддерживаем пиковую интенсивность с постоянными эффектами
            UpdatePeakEffects();
            yield return null;
        }
    }

    private void UpdatePeakEffects()
    {
        // Мигание света на пике
        UpdateLights(peakIntensity, true);

        // Тряска объектов на пике
        UpdateShaking(peakIntensity);

        // Визуальные эффекты на пике
        UpdateVisualEffects(peakIntensity, true);

        // Аудио эффекты на пике
        UpdateAudio(peakIntensity);
    }

    private void UpdateEffects(float intensity, bool isIntensePhase)
    {
        UpdateLights(intensity, isIntensePhase);
        UpdateShaking(intensity);
        UpdateVisualEffects(intensity, isIntensePhase);
        UpdateAudio(intensity);
    }

    private void UpdateLights(float intensity, bool isIntensePhase)
    {
        foreach (Light light in lightsToFlicker)
        {
            if (light != null)
            {
                // Более агрессивное мигание на пике
                float flickerChance = isIntensePhase ? 0.5f : intensity * 0.3f;

                if (Random.value < flickerChance)
                {
                    light.color = flickerColor;
                    light.intensity = Random.Range(0.1f, intensity * 2f);
                }
                else
                {
                    light.color = Color.Lerp(originalLightColors[System.Array.IndexOf(lightsToFlicker, light)],
                                           flickerColor, intensity * 0.7f);
                    light.intensity = Mathf.Lerp(1f, 0.2f, intensity);
                }

                // Быстрые мигания в интенсивной фазе
                if (isIntensePhase && Random.value < 0.2f)
                {
                    StartCoroutine(QuickLightFlicker(light));
                }
            }
        }
    }

    private IEnumerator QuickLightFlicker(Light light)
    {
        if (light == null) yield break;

        float originalIntensity = light.intensity;
        light.intensity = 0f;
        yield return new WaitForSeconds(Random.Range(0.05f, 0.15f));
        if (light != null)
            light.intensity = originalIntensity;
    }

    private void UpdateShaking(float intensity)
    {
        for (int i = 0; i < objectsToShake.Length; i++)
        {
            if (objectsToShake[i] != null)
            {
                float shakeAmount = intensity * maxShakeIntensity;
                // Более хаотичная тряска на пике
                float currentShakeSpeed = intensity >= peakIntensity ? shakeSpeed * 1.5f : shakeSpeed;

                Vector3 shake = new Vector3(
                    Mathf.PerlinNoise(Time.time * currentShakeSpeed, i * 10) - 0.5f,
                    Mathf.PerlinNoise(Time.time * currentShakeSpeed, i * 10 + 5) - 0.5f,
                    Mathf.PerlinNoise(Time.time * currentShakeSpeed, i * 10 + 10) - 0.5f
                ) * shakeAmount;

                objectsToShake[i].localPosition = originalPositions[i] + shake;
            }
        }
    }

    private void UpdateVisualEffects(float intensity, bool isIntensePhase)
    {
        // Частицы пыли
        foreach (ParticleSystem ps in dustParticles)
        {
            if (ps != null)
            {
                if (intensity > 0.3f && !ps.isPlaying)
                    ps.Play();

                var emission = ps.emission;
                emission.rateOverTime = intensity * 100f; // Увеличил количество частиц
            }
        }

        // Трещины на стенах (появляются и остаются)
        if (isIntensePhase && crackedWallMaterial != null)
        {
            foreach (Renderer wall in wallsToChange)
            {
                if (wall != null && intensity > 0.8f)
                {
                    wall.material = crackedWallMaterial;
                    // Легкое мерцание цвета трещин
                    float colorPulse = Mathf.PingPong(Time.time * 0.5f, 0.3f);
                    wall.material.color = Color.Lerp(Color.gray, Color.red, colorPulse);
                }
            }
        }
    }

    private void UpdateAudio(float intensity)
    {
        // Ambient звуки становятся более искаженными
        foreach (AudioSource audio in ambientSounds)
        {
            if (audio != null)
            {
                audio.volume = Mathf.Lerp(1f, 0.1f, intensity);
                audio.pitch = Mathf.Lerp(1f, 0.6f, intensity * 0.7f);

                // Добавляем distortion на пике
                if (intensity >= peakIntensity)
                {
                    audio.pitch = 0.6f + Mathf.PerlinNoise(Time.time * 2f, 0) * 0.4f;
                }
            }
        }

        // Более частые звуки разрушения на пике
        float destructionChance = intensity >= peakIntensity ? 0.3f : intensity * 0.1f;
        if (mainAudioSource != null && destructionSounds.Length > 0 && Random.value < destructionChance)
        {
            if (!mainAudioSource.isPlaying)
            {
                mainAudioSource.clip = destructionSounds[Random.Range(0, destructionSounds.Length)];
                mainAudioSource.pitch = Random.Range(0.8f, 1.2f);
                mainAudioSource.Play();
            }
        }
    }

    // Метод для остановки апокалипсиса (если вдруг понадобится)
    public void StopApocalypse()
    {
        isApocalypseActive = false;

        if (apocalypseCoroutine != null)
            StopCoroutine(apocalypseCoroutine);
        if (infiniteEffectsCoroutine != null)
            StopCoroutine(infiniteEffectsCoroutine);

        ResetEffects();
        Debug.Log("Apocalypse: Разрушение остановлено!");
    }

    private void ResetEffects()
    {
        for (int i = 0; i < objectsToShake.Length; i++)
        {
            if (objectsToShake[i] != null)
                objectsToShake[i].localPosition = originalPositions[i];
        }

        for (int i = 0; i < lightsToFlicker.Length; i++)
        {
            if (lightsToFlicker[i] != null)
            {
                lightsToFlicker[i].color = originalLightColors[i];
                lightsToFlicker[i].intensity = 1f;
            }
        }

        for (int i = 0; i < wallsToChange.Length; i++)
        {
            if (wallsToChange[i] != null)
                wallsToChange[i].material = originalWallMaterials[i];
        }

        foreach (ParticleSystem ps in dustParticles)
        {
            if (ps != null)
                ps.Stop();
        }
    }

    void OnDestroy()
    {
        StopApocalypse();
    }
}