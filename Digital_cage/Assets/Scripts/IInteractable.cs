// IInteractable.cs
using UnityEngine;

public interface IInteractable
{
    void Interact();            // вызывается при нажатии E
    string GetInteractionText(); // текст подсказки, например "Нажмите"
}
