using System;
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
                if (colliderHit.TryGetComponent(out Quest.Quest quest))
                {
                    if (quest.complete) {
                        Debug.Log($"{name} | quest is complete");
                        onQuestComplete?.Invoke();
                    } else {
                        Debug.Log($"{name} | quest is not complete");
                        onQuestStart?.Invoke();
                    }
                }
            }
        }
    }
}
