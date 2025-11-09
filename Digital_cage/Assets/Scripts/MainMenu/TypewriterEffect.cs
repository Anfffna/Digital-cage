using UnityEngine;
using TMPro;
using System.Collections;

public class TypewriterEffect : MonoBehaviour
{
    [Header("Настройки печати")]
    [TextArea(3, 10)]
    [SerializeField] private string textToType = "Добро пожаловать в игру!";
    [SerializeField] private float typingSpeed = 0.05f;
    [SerializeField] private float startDelay = 1.0f;
    [SerializeField] private float eraseDelay = 2.0f;
    [SerializeField] private float eraseSpeed = 0.02f;

    private TMP_Text textComponent;

    void Start()
    {
        // Получаем компонент TextMeshPro
        textComponent = GetComponent<TMP_Text>();

        // Проверяем, что компонент найден
        if (textComponent == null)
        {
            Debug.LogError("TMP_Text component not found on " + gameObject.name);
            return;
        }

        // Начинаем бесконечный цикл печати
        StartCoroutine(TypewriterLoop());
    }

    IEnumerator TypewriterLoop()
    {
        // Начальная задержка перед первым циклом
        yield return new WaitForSeconds(startDelay);

        // Бесконечный цикл
        while (true)
        {
            // Печатаем текст
            yield return StartCoroutine(TypeText());

            // Ждем перед стиранием
            yield return new WaitForSeconds(eraseDelay);

            // Стираем текст
            yield return StartCoroutine(EraseText());

            // Короткая пауза перед началом нового цикла
            yield return new WaitForSeconds(0.5f);
        }
    }

    IEnumerator TypeText()
    {
        if (textComponent == null) yield break;

        textComponent.text = "";

        foreach (char letter in textToType.ToCharArray())
        {
            if (textComponent == null) yield break;

            textComponent.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }
    }

    IEnumerator EraseText()
    {
        if (textComponent == null) yield break;

        // Стираем по одному символу с конца
        while (textComponent.text.Length > 0)
        {
            if (textComponent == null) yield break;

            textComponent.text = textComponent.text.Substring(0, textComponent.text.Length - 1);
            yield return new WaitForSeconds(eraseSpeed);
        }
    }

    // Метод для принудительного запуска печати
    public void StartTyping(string newText = "")
    {
        if (!string.IsNullOrEmpty(newText))
        {
            textToType = newText;
        }

        StopAllCoroutines();
        StartCoroutine(TypewriterLoop());
    }

    // Метод для остановки цикла
    public void StopTypewriter()
    {
        StopAllCoroutines();
    }
}