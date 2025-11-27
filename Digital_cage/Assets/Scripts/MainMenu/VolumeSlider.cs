using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class VolumeSlider : MonoBehaviour, IPointerDownHandler, IDragHandler
{
    [Header("References")]
    public RectTransform slider; // Сам ползунок
    public TextMeshProUGUI volumeText; // Текст с процентом

    [Header("Slider Range")]
    public float minX = -75f; // Левый край панели
    public float maxX = 75f;  // Правый край панели

    private RectTransform volumePanel; // Родительская панель
    private float currentVolume = 0.5f;
    private string volumeKey = "MasterVolume";

    void Start()
    {
        // Получаем родительскую панель автоматически
        volumePanel = transform.parent.GetComponent<RectTransform>();

        if (slider != null)
        {
            slider.pivot = new Vector2(0.5f, 0.5f);
            slider.anchorMin = new Vector2(0.5f, 0.5f);
            slider.anchorMax = new Vector2(0.5f, 0.5f);
        }

        LoadVolume();
        UpdateVisuals();
    }

    // Явная реализация интерфейса (для самого ползунка)
    void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
    {
        UpdateVolumeFromPosition(eventData.position);
    }

    void IDragHandler.OnDrag(PointerEventData eventData)
    {
        UpdateVolumeFromPosition(eventData.position);
    }

    // Публичный метод для Event Trigger (для панели)
    public void HandlePanelClick(BaseEventData eventData)
    {
        if (eventData is PointerEventData pointerData)
        {
            UpdateVolumeFromPosition(pointerData.position);
        }
    }

    private void UpdateVolumeFromPosition(Vector2 screenPosition)
    {
        if (volumePanel == null) return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            volumePanel, screenPosition, null, out Vector2 localPoint);

        float clampedX = Mathf.Clamp(localPoint.x, minX, maxX);
        currentVolume = Mathf.InverseLerp(minX, maxX, clampedX);

        UpdateVisuals();
        ApplyVolume();
    }

    private void UpdateVisuals()
    {
        if (slider == null) return;

        float handleX = Mathf.Lerp(minX, maxX, currentVolume);
        slider.anchoredPosition = new Vector2(handleX, 0f);

        if (volumeText != null)
        {
            volumeText.text = $"{Mathf.RoundToInt(currentVolume * 100)}%";
        }
    }

    private void ApplyVolume()
    {
        AudioListener.volume = currentVolume;
        PlayerPrefs.SetFloat(volumeKey, currentVolume);
        PlayerPrefs.Save();
        Debug.Log($"Громкость: {Mathf.RoundToInt(currentVolume * 100)}%");
    }

    private void LoadVolume()
    {
        currentVolume = PlayerPrefs.GetFloat(volumeKey, 0.5f);
        AudioListener.volume = currentVolume;
    }

    public void SetVolume(float volume)
    {
        currentVolume = Mathf.Clamp01(volume);
        UpdateVisuals();
        ApplyVolume();
    }

    public float GetVolume()
    {
        return currentVolume;
    }
}