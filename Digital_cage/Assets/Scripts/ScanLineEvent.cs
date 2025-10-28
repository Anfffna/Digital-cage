using UnityEngine;

public class ScanLineEvent : MonoBehaviour
{
    [Header("—сылка на родительский терминал")]
    public DialogueTriggerTerminal parentTerminal;

    // Ётот метод будет вызыватьс€ из Animation Event
    public void CallShowDialogueLine1()
    {
        if (parentTerminal != null)
        {
            parentTerminal.ShowDialogueLine1();
        }
    }
}
