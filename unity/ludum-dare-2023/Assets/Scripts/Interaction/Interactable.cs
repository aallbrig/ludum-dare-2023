using UnityEngine;
using UnityEngine.Events;

namespace Interaction
{
    public class Interactable : MonoBehaviour, IInteractable
    {
        public UnityEvent onInteract;
        public void Interact()
        {
            onInteract?.Invoke();
        }
    }
}
