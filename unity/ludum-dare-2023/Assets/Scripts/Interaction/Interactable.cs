using UnityEngine;
using UnityEngine.Events;

namespace Interaction
{
    public class Interactable : MonoBehaviour, IInteractable
    {
        public UnityEvent onInteraction;
        public void Interact()
        {
            onInteraction?.Invoke();
        }
    }
}
