using UnityEngine;
using UnityEngine.Events;

namespace Interaction
{
    public class QuestInteractable : MonoBehaviour, IInteractable
    {
        public UnityEvent onQuestStart;
        public UnityEvent onQuestComplete;
        public void Interact()
        {
            foreach (var colliderHit in Physics.OverlapSphere(transform.position, 3f))
            {
                if (colliderHit.TryGetComponent(out Quest.Quest quest)) {
                    if (quest.complete) {
                        Debug.Log("quest complete!");
                        onQuestComplete?.Invoke();
                    }
                    else {
                        Debug.Log("quest not complete");
                        onQuestStart?.Invoke();
                    }
                }
            }
        }
    }
}
