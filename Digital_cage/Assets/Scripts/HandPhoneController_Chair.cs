using UnityEngine;
using System.Collections;

public class HandPhoneController_Chair : MonoBehaviour
{
    [Header("Слоты и объекты")]
    public GameObject phoneObject;           // сам телефон
    public Transform phoneSlotInHand;        // пустышка в руке
    public Transform tableSpot;              // точка на столе
    public Animator handAnimator;            // аниматор руки

    [Header("Параметры движения")]
    public float moveDuration = 0.7f;        // длительность перемещения телефона
    public float handLiftDelay = 0.9f;      // задержка до момента отрыва телефона от руки

    [HideInInspector] public bool hasPutPhone = false;

    // Этот метод вызывается из ChairSit с задержкой
    public void StartPhonePutAnimation()
    {
        if (hasPutPhone) return;
        StartCoroutine(PutPhoneOnTable());
    }

    private IEnumerator PutPhoneOnTable()
    {
        if (handAnimator != null)
            handAnimator.SetTrigger("Lift");

        yield return new WaitForSeconds(handLiftDelay);

        // Отсоединяем телефон от руки
        phoneObject.transform.SetParent(null);

        // Плавно перемещаем на стол
        Vector3 startPos = phoneObject.transform.position;
        Quaternion startRot = phoneObject.transform.rotation;
        Vector3 endPos = tableSpot.position;
        Quaternion endRot = tableSpot.rotation;

        float timer = 0f;
        while (timer < moveDuration)
        {
            float t = Mathf.SmoothStep(0f, 1f, timer / moveDuration);
            phoneObject.transform.position = Vector3.Lerp(startPos, endPos, t);
            phoneObject.transform.rotation = Quaternion.Slerp(startRot, endRot, t);
            timer += Time.deltaTime;
            yield return null;
        }

        phoneObject.transform.position = endPos;
        phoneObject.transform.rotation = endRot;
        hasPutPhone = true;

        Debug.Log("Вторая рука положила телефон на стол.");
    }
}
