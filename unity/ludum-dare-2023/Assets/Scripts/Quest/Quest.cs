using UnityEngine;

namespace Quest
{
    public class Quest : MonoBehaviour
    {
        public bool complete = false;
        public void Complete()
        {
            complete = true;
        }
        public void Reset()
        {
            complete = false;
        }
    }
}
