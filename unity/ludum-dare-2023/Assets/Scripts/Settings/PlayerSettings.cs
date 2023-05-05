using UnityEngine;

namespace Settings
{
    [CreateAssetMenu(fileName = "Player Settings", menuName = "Game/Player/new Player Settings", order = 0)]
    public class PlayerSettings : ScriptableObject
    {
        public float movementSpeed = 4f;
        public float jumpForce = 4f;
        public float jumpTime = 1.2f;
        public AnimationCurve jumpCurve;
        public float interactDistance = 2.4f;
    }
}
