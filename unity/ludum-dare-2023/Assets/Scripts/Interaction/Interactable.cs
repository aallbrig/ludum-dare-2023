using UnityEngine;
using UnityEngine.Events;

namespace Interaction
{
    public class InteractableUnityEvent : MonoBehaviour, IInteractable
    {
        public UnityEvent onInteract;
        public void Interact()
        {
            onInteract?.Invoke();
        }
    }
}
