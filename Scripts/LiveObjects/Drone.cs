using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using Game.Scripts.UI;
using UnityEngine.InputSystem;
using Unity.VisualScripting;

namespace Game.Scripts.LiveObjects
{
    public class Drone : MonoBehaviour
    {
        private enum Tilt
        {
            NoTilt, Forward, Back, Left, Right
        }

        [SerializeField]
        private Rigidbody _rigidbody;
        [SerializeField]
        private float _speed = 5f;
        private bool _inFlightMode = false;
        [SerializeField]
        private Animator _propAnim;
        [SerializeField]
        private CinemachineVirtualCamera _droneCam;
        [SerializeField]
        private InteractableZone _interactableZone;
        

        public static event Action OnEnterFlightMode;
        public static event Action onExitFlightmode;

        //Reference to New InputSystem Action Map
        private Player_Controls _newInputControl;

        private void Start()
        {
            _newInputControl = new Player_Controls();
            _newInputControl.Drone.Enable();
        }

        private void OnEnable()
        {
            InteractableZone.onZoneInteractionComplete += EnterFlightMode;
        }

        private void EnterFlightMode(InteractableZone zone)
        {
            if (_inFlightMode != true && zone.GetZoneID() == 4) // drone Scene
            {
                _propAnim.SetTrigger("StartProps");
                _droneCam.Priority = 11;
                _inFlightMode = true;
                OnEnterFlightMode?.Invoke();
                UIManager.Instance.DroneView(true);
                _interactableZone.CompleteTask(4);
            }
        }

        private void ExitFlightMode()
        {            
            _droneCam.Priority = 9;
            _inFlightMode = false;
            UIManager.Instance.DroneView(false);            
        }

        private void Update()
        {
            if (_inFlightMode)
            {
                CalculateTilt();

                CalculateMovementUpdate();

                //if (Input.GetKeyDown(KeyCode.Escape))
                //if (Keyboard.current.escapeKey.wasPressedThisFrame || (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame))
                if (_newInputControl.Drone.Exit.WasPressedThisFrame())
                {
                    _inFlightMode = false;
                    onExitFlightmode?.Invoke();
                    ExitFlightMode();
                }
            }
        }

        private void FixedUpdate()
        {
            _rigidbody.AddForce(transform.up * (9.81f), ForceMode.Acceleration);
            
            if (_inFlightMode)
                CalculateMovementFixedUpdate();
        }

        private void CalculateMovementUpdate()
        {
            //New InputSystem
            var rotateDirection = _newInputControl.Drone.Rotate.ReadValue<float>();

            if (Mathf.Abs(rotateDirection) > 0.01f)
            {
                float rotationSpeed = _speed / 3f * Time.deltaTime;
                transform.Rotate(Vector3.up * rotateDirection * rotationSpeed * 100f);
            }

            /*
            if (rotateDirection < -0.01)
            {
                var tempRot = transform.localRotation.eulerAngles;

                Debug.Log(tempRot);

                tempRot.y -= _speed / 3;
                transform.localRotation = Quaternion.Euler(tempRot);
            }
            if (rotateDirection > 0.01)
            {
                var tempRot = transform.localRotation.eulerAngles;
                tempRot.y += _speed / 3;
                transform.localRotation = Quaternion.Euler(tempRot);
            }
            */
            /*
            //Old InputSystem
            if (Input.GetKey(KeyCode.LeftArrow))
            {
                var tempRot = transform.localRotation.eulerAngles;

                Debug.Log(tempRot);

                tempRot.y -= _speed / 3;
                transform.localRotation = Quaternion.Euler(tempRot);
            }
            if (Input.GetKey(KeyCode.RightArrow))
            {
                var tempRot = transform.localRotation.eulerAngles;
                tempRot.y += _speed / 3;
                transform.localRotation = Quaternion.Euler(tempRot);
            }
            */
        }

        private void CalculateMovementFixedUpdate()
        {
            //if (Input.GetKey(KeyCode.Space))
            //if (Keyboard.current.spaceKey.isPressed || (Gamepad.current != null && Gamepad.current.buttonWest.wasPressedThisFrame))
            if (_newInputControl.Drone.Up.IsPressed())
            {
                _rigidbody.AddForce(transform.up * _speed, ForceMode.Acceleration);
            }
            //if (Input.GetKey(KeyCode.V))
            //if (Keyboard.current.vKey.isPressed || (Gamepad.current != null && Gamepad.current.buttonEast.wasPressedThisFrame))
            if (_newInputControl.Drone.Down.IsPressed())
            {
                _rigidbody.AddForce(-transform.up * _speed, ForceMode.Acceleration);
            }
        }

        private void CalculateTilt()
        {
            //New InputSystem
            var droneRotate = _newInputControl.Drone.Flight.ReadValue<Vector2>();

            float maxTilt = 30f;
            float pitch = 0f;
            float roll  = 0f;

            if (droneRotate.x < -0.1f)
            {
                roll = maxTilt;
            }
            else if (droneRotate.x > 0.1f)
            {
                roll = -maxTilt;
            }
            else if (droneRotate.y > 0.1f)
            {
                pitch = maxTilt;
            }
            else if (droneRotate.y < -0.1f)
            {
                pitch = -maxTilt;
            }

            transform.rotation = Quaternion.Euler(pitch, transform.localRotation.eulerAngles.y, roll);

            //transform.Translate(new Vector3(droneRotate.x, 0, droneRotate.y) * Time.deltaTime * _speed);s
                
            /*
            //Old InputSystem
            if (Input.GetKey(KeyCode.A)) 
                transform.rotation = Quaternion.Euler(00, transform.localRotation.eulerAngles.y, 30);
            else if (Input.GetKey(KeyCode.D))
                transform.rotation = Quaternion.Euler(0, transform.localRotation.eulerAngles.y, -30);
            else if (Input.GetKey(KeyCode.W))
                transform.rotation = Quaternion.Euler(30, transform.localRotation.eulerAngles.y, 0);
            else if (Input.GetKey(KeyCode.S))
                transform.rotation = Quaternion.Euler(-30, transform.localRotation.eulerAngles.y, 0);
            else 
                transform.rotation = Quaternion.Euler(0, transform.localRotation.eulerAngles.y, 0);
            */
        }

        private void OnDisable()
        {
            InteractableZone.onZoneInteractionComplete -= EnterFlightMode;
        }
    }
}
