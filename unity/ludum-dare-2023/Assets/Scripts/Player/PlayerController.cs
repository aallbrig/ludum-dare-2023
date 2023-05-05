using System;
using Cinemachine;
using CleverCrow.Fluid.FSMs;
using Interaction;
using Player.FSM;
using Settings;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Player
{
    [RequireComponent(typeof(PlayerInput), typeof(CharacterController))]
    public class PlayerController : MonoBehaviour, IUnityEventInteractor
    {
        public PlayerSettings playerSettings;
        public Animator targetAnimator;
        public Transform bikeTransform;
        public UnityEvent zoomInEvents;
        public UnityEvent zoomOutEvents;
        public UnityEvent interactEvents;
        private PlayerInput _playerInput;
        private Camera _perspectiveCamera;
        private CharacterController _characterController;
        private Vector2 _movementInputVector;
        private bool _jumpingInput;
        private IFsm _playerFsm;
        private int _stateEnterFrameCount;
        public CinemachineFreeLook cinemachineFreeLook;
        private bool _isJumping;
        private Vector3 _velocity;
        private int _frameCounter = -1; // first frame should be 0, not 1
        private Vector3 _inputDirectionFromPerspectiveCamera;
        private PlayerStates _playerFsmAfterTickState;
        private PlayerStates _playerFsmBeforeTickState;
        private float _jumpTime;

        private void Awake()
        {
            _playerInput = GetComponent<PlayerInput>();
            _perspectiveCamera = _playerInput.camera;
            _perspectiveCamera ??= Camera.main;
            _characterController = GetComponent<CharacterController>();
            playerSettings ??= ScriptableObject.CreateInstance<PlayerSettings>();
            _playerFsm = CreatePlayerFsm();
            cinemachineFreeLook ??= FindObjectOfType<CinemachineFreeLook>();
        }
        private void Update()
        {
            _frameCounter++;
            _inputDirectionFromPerspectiveCamera = _perspectiveCamera.transform.TransformDirection(
                new Vector3(_movementInputVector.x, 0, _movementInputVector.y)
            );
            // convert IState to PlayerStates
            _playerFsmBeforeTickState = (PlayerStates) _playerFsm.CurrentState.Id;
            _playerFsm.Tick();
            _playerFsmAfterTickState = (PlayerStates) _playerFsm.CurrentState.Id;
            if (_playerFsmBeforeTickState != _playerFsmAfterTickState) Debug.Log($"{name} | FSM state change: {_playerFsmAfterTickState}");
            if (_velocity != Vector3.zero) _characterController.Move(_velocity * Time.deltaTime);
        }
        public UnityEvent InteractEvents => interactEvents;
        public void OnMove(InputValue value) => _movementInputVector = value.Get<Vector2>();
        public void OnJump(InputValue value) => _jumpingInput = value.isPressed;
        public void OnZoom(InputValue value)
        {
            if (value.isPressed)
            {
                Debug.Log($"{name} | zoom button pressed, triggering event number: {zoomInEvents.GetPersistentEventCount()}");
                zoomInEvents?.Invoke();
            }
            else
                zoomOutEvents?.Invoke();
        }
        public void OnInteract(InputValue value)
        {
            if (value.isPressed)
            {
                Debug.Log($"{name} | Interact triggering event number: {interactEvents.GetPersistentEventCount()}");
                InteractEvents?.Invoke();
            }
        }
        public void ShowBike(bool show)
        {
            if (bikeTransform == null) return;
            bikeTransform.gameObject.SetActive(show);
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
                            Debug.Log($"{name} | idle state enter (frame {_frameCounter})");
                            _velocity = Vector3.zero;
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
                            
                            // Don't allow player to move in y direction
                            if (_movementInputVector == Vector2.zero) return;
                            var moveDirection = new Vector3(_inputDirectionFromPerspectiveCamera.x, 0, _inputDirectionFromPerspectiveCamera.z);
                            _velocity = moveDirection * playerSettings.movementSpeed;
                            if (moveDirection != Vector3.zero) transform.rotation = Quaternion.LookRotation(moveDirection);
                        })
                        .SetTransition(PlayerStates.Idle.ToString(), PlayerStates.Idle)
                        .SetTransition(PlayerStates.Jumping.ToString(), PlayerStates.Jumping)
                        .SetTransition(PlayerStates.Falling.ToString(), PlayerStates.Falling)
                        .Update(action =>
                        {
                            // if (!_characterController.isGrounded) action.Transition(PlayerStates.Falling.ToString());
                            if (_movementInputVector == Vector2.zero) action.Transition(PlayerStates.Idle.ToString());
                            if (_jumpingInput) action.Transition(PlayerStates.Jumping.ToString());

                            // Don't allow player to move in y direction
                            var moveDirection = new Vector3(_inputDirectionFromPerspectiveCamera.x, 0, _inputDirectionFromPerspectiveCamera.z);
                            _velocity = moveDirection * playerSettings.movementSpeed;
                            if (moveDirection != Vector3.zero) transform.rotation = Quaternion.LookRotation(moveDirection);
                        });
                })
                .State(PlayerStates.Jumping, stateBuilder =>
                {
                    stateBuilder
                        .SetTransition(PlayerStates.Idle.ToString(), PlayerStates.Idle)
                        .SetTransition(PlayerStates.Falling.ToString(), PlayerStates.Falling)
                        .Enter(action =>
                        {
                            _stateEnterFrameCount = _frameCounter;
                            _jumpTime = 0;
                            Debug.Log($"Jumping state enter (frame {_stateEnterFrameCount})");
                            var sampledJumpForce = playerSettings.jumpCurve.Evaluate(_jumpTime);
                            Debug.Log($"sampled jump force {sampledJumpForce}");
                            Debug.Log($"sampled jump force {sampledJumpForce} results in {playerSettings.jumpForce * sampledJumpForce}");
                            var moveDirection = new Vector3(_inputDirectionFromPerspectiveCamera.x, 0, _inputDirectionFromPerspectiveCamera.z);
                            _velocity = moveDirection * playerSettings.movementSpeed;
                            _velocity.y = playerSettings.jumpForce * sampledJumpForce;
                            if (moveDirection != Vector3.zero) transform.rotation = Quaternion.LookRotation(moveDirection);
                            targetAnimator.SetBool("Idle", false);
                            targetAnimator.SetBool("Running", false);
                            targetAnimator.SetBool("Jumping", true);
                            // todo: falling animation state
                        })
                        .Update(action =>
                        {
                            _jumpTime += Time.deltaTime;
                            var curveSample = playerSettings.jumpCurve.Evaluate(_jumpTime / playerSettings.jumpTime);
                            var moveDirection = new Vector3(_inputDirectionFromPerspectiveCamera.x, 0, _inputDirectionFromPerspectiveCamera.z);
                            _velocity = moveDirection * playerSettings.movementSpeed;
                            _velocity.y = playerSettings.jumpForce * curveSample;
                            if (moveDirection != Vector3.zero) transform.rotation = Quaternion.LookRotation(moveDirection);

                            if (curveSample >= 0.9999f)
                            {
                                action.Transition(PlayerStates.Falling.ToString());
                            }
                        });
                })
                .State(PlayerStates.Falling, stateBuilder =>
                {
                    stateBuilder
                        .Enter(_ =>
                        {
                            _jumpTime = 0;
                            targetAnimator.SetBool("Idle", true);
                            targetAnimator.SetBool("Running", false);
                            targetAnimator.SetBool("Jumping", false);
                        })
                        .SetTransition(PlayerStates.Idle.ToString(), PlayerStates.Idle)
                        .SetTransition(PlayerStates.Running.ToString(), PlayerStates.Running)
                        .Update(action =>
                        {
                            _jumpTime += Time.deltaTime;
                            if (_characterController.isGrounded)
                            {
                                _velocity.y = 0;
                                if (_movementInputVector != Vector2.zero) action.Transition(PlayerStates.Running.ToString());
                                else action.Transition(PlayerStates.Idle.ToString());
                                return;
                            }
                            var curveSample = playerSettings.jumpCurve.Evaluate(_jumpTime / playerSettings.jumpTime);
                            var moveDirection = new Vector3(_inputDirectionFromPerspectiveCamera.x, 0, _inputDirectionFromPerspectiveCamera.z);
                            _velocity = moveDirection * playerSettings.movementSpeed;
                            _velocity.y = Physics.gravity.y * curveSample; // apply gravity
                            if (moveDirection != Vector3.zero) transform.rotation = Quaternion.LookRotation(moveDirection);
                        });
                })
                .Build();
        }
    }
}
