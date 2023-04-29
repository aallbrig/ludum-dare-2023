using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    [RequireComponent(typeof(PlayerInput), typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        private PlayerInput _playerInput;
        private CharacterController _characterController;

        private void Awake()
        {
            _playerInput = GetComponent<PlayerInput>();
            _characterController = GetComponent<CharacterController>();
        }

        // Receives the "OnMove" event from the PlayerInput component
        public void OnMove(InputAction.CallbackContext context)
        {
            // If the player is not grounded, don't move
            if (!_characterController.isGrounded) return;
            
            // Get the input vector from the context
            var inputVector = context.ReadValue<Vector2>();
            
            // Move the player
            _characterController.Move(new Vector3(inputVector.x, 0, inputVector.y));
        }
    }
}
