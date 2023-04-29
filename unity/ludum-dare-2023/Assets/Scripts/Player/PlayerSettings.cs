using UnityEngine;

namespace Player
{
    [CreateAssetMenu(fileName = "Player Settings", menuName = "Game/Player/new Player Settings", order = 0)]
    public class PlayerSettings : ScriptableObject
    {
        public float movementSpeed = 4f;
    }
}
