using UnityEngine;

namespace Interactables
{
    public interface IInteractable
    {
        string GetInteractionPrompt();
        void Interact(GameObject interactor);
        bool CanInteract(GameObject interactor);
        Transform GetTransform();
    }
}