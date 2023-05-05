using Settings;
using UnityEngine;

namespace Interaction
{
    [RequireComponent(typeof(Collider))]
    public class AddInteractUnityEventInRange : MonoBehaviour
    {
        public PlayerSettings playerSettings;
        public Transform target;
        private IInteractable _interactable;
        private Collider _collider;
        private void Awake()
        {
            target ??= transform;
            if (playerSettings == null)
            {
                Debug.Log($"{name} | No PlayerSettings found. Creating default settings (worse memory profile and may lead to unexpected results)");
                playerSettings ??= ScriptableObject.CreateInstance<PlayerSettings>();
            }
            _interactable = target.GetComponent<IInteractable>();
            if (_interactable == null) Debug.Log($"{name} | No IInteractable found on target");
            _collider = GetComponent<Collider>();
            if (_collider is SphereCollider sphereCollider)
                sphereCollider.radius = playerSettings.interactDistance * 0.5f;
            if (_collider is BoxCollider boxCollider)
                boxCollider.size = Vector3.one * playerSettings.interactDistance;
        }
        private void OnTriggerEnter(Collider other)
        {
            if (_interactable == null) return;
            if (other.TryGetComponent(out IUnityEventInteractor unityEventInteractor))
                AddInteractUnityEvent(unityEventInteractor);
        }
        private void OnTriggerExit(Collider other)
        {
            if (_interactable == null) return;
            if (other.TryGetComponent(out IUnityEventInteractor unityEventInteractor))
                RemoveInteractUnityEvent(unityEventInteractor);
        }
        private void OnCollisionEnter(Collision other)
        {
            if (_interactable == null) return;
            if (other.transform.TryGetComponent(out IUnityEventInteractor unityEventInteractor))
                AddInteractUnityEvent(unityEventInteractor);
        }
        private void OnCollisionExit(Collision other)
        {
            if (other.transform.TryGetComponent(out IUnityEventInteractor unityEventInteractor))
                RemoveInteractUnityEvent(unityEventInteractor);
        }
        private void AddInteractUnityEvent(IUnityEventInteractor unityEventInteractor)
        {
            Debug.Log($"{name} | Adding interact to interactor's unity event list");
            unityEventInteractor.InteractEvents.AddListener(_interactable.Interact);
        }
        private void RemoveInteractUnityEvent(IUnityEventInteractor unityEventInteractor)
        {
            Debug.Log($"{name} | Removing interact to interactor's unity event list");
            unityEventInteractor.InteractEvents.RemoveListener(_interactable.Interact);
        }
    }
}
