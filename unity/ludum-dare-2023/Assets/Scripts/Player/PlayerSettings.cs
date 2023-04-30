using UnityEngine;

namespace Player
{
    [CreateAssetMenu(fileName = "Player Settings", menuName = "Game/Player/new Player Settings", order = 0)]
    public class PlayerSettings : ScriptableObject
    {
        public float movementSpeed = 4f;
        public float jumpForce = 10f;
        public float interactDistance = 2.4f;
        public AnimationCurve jumpCurve;
    }
}
