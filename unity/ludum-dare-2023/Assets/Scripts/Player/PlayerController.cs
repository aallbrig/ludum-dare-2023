using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    [RequireComponent(typeof(PlayerInput), typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        public PlayerSettings playerSettings;
        private PlayerInput _playerInput;
        private Camera _perspectiveCamera;
        private CharacterController _characterController;
        private Vector2 _inputVector;
        private PlayerSettings _playerSettingsInstance;

        private void Awake()
        {
            _playerInput = GetComponent<PlayerInput>();
            _perspectiveCamera = _playerInput.camera;
            _perspectiveCamera ??= Camera.main;
            _characterController = GetComponent<CharacterController>();
            playerSettings ??= ScriptableObject.CreateInstance<PlayerSettings>();
            _playerSettingsInstance = Instantiate(playerSettings);
        }

        public void OnMove(InputValue value)
        {
            _inputVector = value.Get<Vector2>();
        }

        private void Update()
        {
            if (_inputVector != Vector2.zero)
            {
                var convertedInputVector = new Vector3(_inputVector.x, 0, _inputVector.y);
                var movementByPerspective = _perspectiveCamera.transform.TransformDirection(convertedInputVector);
                // Don't allow player to move in y direction
                movementByPerspective.y = 0;
                _characterController.Move(
                    _playerSettingsInstance.movementSpeed
                    * Time.deltaTime
                    * movementByPerspective
                );
                transform.rotation = Quaternion.LookRotation(movementByPerspective);
            }
            else
                _characterController.Move(Physics.gravity * Time.deltaTime);
        }
    }
}
