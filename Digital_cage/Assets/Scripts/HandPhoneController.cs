using UnityEngine;
using System.Collections;

public class HandPhoneController : MonoBehaviour
{
    [Header("Слоты")]
    public Transform phoneSlotInHand; // пустышка в руке
    public GameObject phone;          // телефон
    public Transform tableSpot;       // точка на столе
    public Animator handAnimator;     // Animator с LiftHand и ReturnHand

    [Header("Настройки")]
    public float moveDuration = 0.7f; // время перемещения телефона

    [HideInInspector] public bool hasPutPhone = false;

    // вызываем на 7-й реплике
    public void OnDialogueLineFinished(int lineIndex)
    {
        if (lineIndex == 7) // 7-я реплика (индексация с 0)
        {
            StartCoroutine(PutPhoneOnTable());
        }
    }

    private IEnumerator PutPhoneOnTable()
    {
        // 1?? запустить анимацию подъёма руки
        handAnimator.SetTrigger("Lift");

        // 2?? подождать пока рука поднимется (0.75 сек из твоего Idle?LiftHand)
        yield return new WaitForSeconds(1.2f);

        // 3?? отсоединить телефон от руки
        phone.transform.SetParent(null);

        // 4?? плавно переложить телефон на стол
        Vector3 startPos = phone.transform.position;
        Quaternion startRot = phone.transform.rotation;
        Vector3 endPos = tableSpot.position;
        Quaternion endRot = tableSpot.rotation;

        float timer = 0f;
        while (timer < moveDuration)
        {
            float t = Mathf.SmoothStep(0f, 1f, timer / moveDuration);
            phone.transform.position = Vector3.Lerp(startPos, endPos, t);
            phone.transform.rotation = Quaternion.Slerp(startRot, endRot, t);
            timer += Time.deltaTime;
            yield return null;
        }
        phone.transform.position = endPos;
        phone.transform.rotation = endRot;

        hasPutPhone = true; // <-- ДОБАВЛЕНО: теперь можно вставать
    }
}




