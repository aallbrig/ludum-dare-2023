using Cinemachine;
using CleverCrow.Fluid.FSMs;
using Player.FSM;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    [RequireComponent(typeof(PlayerInput), typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        public PlayerSettings playerSettings;
        public Animator targetAnimator;
        private PlayerInput _playerInput;
        private Camera _perspectiveCamera;
        private CharacterController _characterController;
        private Vector2 _movementInputVector;
        private bool _jumpingInput;
        private float _jumpTime;
        private PlayerSettings _playerSettingsInstance;
        private IFsm _playerFsm;
        public CinemachineFreeLook cinemachineFreeLook;
        private float _fieldOfView;

        public void OnMove(InputValue value) => _movementInputVector = value.Get<Vector2>();
        public void OnJump(InputValue value) => _jumpingInput = value.isPressed;
        public void OnZoom(InputValue value)
        {
            if (value.isPressed)
                cinemachineFreeLook.m_Lens.FieldOfView = 30f;
            else
                cinemachineFreeLook.m_Lens.FieldOfView = _fieldOfView;
        }
        private IFsm CreatePlayerFsm()
        {
            return new FsmBuilder()
                .Owner(gameObject)
                .Default(PlayerStates.Idle)
                .State(PlayerStates.Idle, stateBuilder =>
                {
                    stateBuilder
                        .Enter(_ =>
                        {
                            targetAnimator.SetBool("Idle", true);
                            targetAnimator.SetBool("Running", false);
                            targetAnimator.SetBool("Jumping", false);
                        })
                        .SetTransition(PlayerStates.Running.ToString(), PlayerStates.Running)
                        .SetTransition(PlayerStates.Jumping.ToString(), PlayerStates.Jumping)
                        .Update(action =>
                        {
                            if (_movementInputVector != Vector2.zero) action.Transition(PlayerStates.Running.ToString());
                            if (_jumpingInput) action.Transition(PlayerStates.Jumping.ToString());
                        });
                })
                .State(PlayerStates.Running, stateBuilder =>
                {
                    stateBuilder
                        .Enter(_ =>
                        {
                            targetAnimator.SetBool("Idle", false);
                            targetAnimator.SetBool("Running", true);
                            targetAnimator.SetBool("Jumping", false);
                        })
                        .SetTransition(PlayerStates.Idle.ToString(), PlayerStates.Idle)
                        .SetTransition(PlayerStates.Jumping.ToString(), PlayerStates.Jumping)
                        .Update(action =>
                        {
                            if (_movementInputVector == Vector2.zero) action.Transition(PlayerStates.Idle.ToString());
                            if (_jumpingInput) action.Transition(PlayerStates.Jumping.ToString());
                            var gravity = Physics.gravity * Time.deltaTime;
                            if (_movementInputVector != Vector2.zero)
                            {
                                var convertedInputVector = new Vector3(_movementInputVector.x, 0, _movementInputVector.y);
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
                            _characterController.Move(gravity);
                        });
                })
                .State(PlayerStates.Jumping, stateBuilder =>
                {
                    stateBuilder
                        .SetTransition(PlayerStates.Idle.ToString(), PlayerStates.Idle)
                        .Enter(action =>
                        {
                            _jumpTime = 0;
                            targetAnimator.SetBool("Idle", false);
                            targetAnimator.SetBool("Running", false);
                            targetAnimator.SetBool("Jumping", true);
                        })
                        .Update(action =>
                        {
                            var gravity = Physics.gravity * Time.deltaTime;
                            _jumpTime += Time.deltaTime;
                            var jumpCurveEval = _playerSettingsInstance.jumpCurve.Evaluate(_jumpTime);
                            var moveDirection = new Vector3(_movementInputVector.x, 0, _movementInputVector.y);
                            moveDirection.y = _playerSettingsInstance.jumpForce * jumpCurveEval;
                            _characterController.Move(moveDirection * Time.deltaTime);
                            _characterController.Move(gravity);
                            if (_characterController.isGrounded) action.Transition(PlayerStates.Idle.ToString());
                        });
                })
                .Build();
        }
        private void Awake()
        {
            _playerInput = GetComponent<PlayerInput>();
            _perspectiveCamera = _playerInput.camera;
            _perspectiveCamera ??= Camera.main;
            _characterController = GetComponent<CharacterController>();
            playerSettings ??= ScriptableObject.CreateInstance<PlayerSettings>();
            _playerSettingsInstance = Instantiate(playerSettings);
            _playerFsm = CreatePlayerFsm();
            cinemachineFreeLook ??= FindObjectOfType<CinemachineFreeLook>();
            _fieldOfView = cinemachineFreeLook.m_Lens.FieldOfView;
        }

        private void Update()
        {
            _playerFsm.Tick();
        }
    }
}
