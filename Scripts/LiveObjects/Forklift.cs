using System;
using UnityEngine;
using Cinemachine;
using UnityEngine.InputSystem;

namespace Game.Scripts.LiveObjects
{
    public class Forklift : MonoBehaviour
    {
        [SerializeField]
        private GameObject _lift, _steeringWheel, _leftWheel, _rightWheel, _rearWheels;
        [SerializeField]
        private Vector3 _liftLowerLimit, _liftUpperLimit;
        [SerializeField]
        private float _speed = 5f, _liftSpeed = 1f;
        [SerializeField]
        private CinemachineVirtualCamera _forkliftCam;
        [SerializeField]
        private GameObject _driverModel;
        private bool _inDriveMode = false;
        [SerializeField]
        private InteractableZone _interactableZone;

        public static event Action onDriveModeEntered;
        public static event Action onDriveModeExited;

        //Reference to New InputSystem Action Map
        private Player_Controls _newInputControl;

        private void Start()
        {
            _newInputControl = new Player_Controls();
            _newInputControl.Forklift.Enable();
        }

        private void OnEnable()
        {
            InteractableZone.onZoneInteractionComplete += EnterDriveMode;
        }

        private void EnterDriveMode(InteractableZone zone)
        {
            if (_inDriveMode !=true && zone.GetZoneID() == 5) //Enter ForkLift
            {
                _inDriveMode = true;
                _forkliftCam.Priority = 11;
                onDriveModeEntered?.Invoke();
                _driverModel.SetActive(true);
                _interactableZone.CompleteTask(5);
            }
        }

        private void ExitDriveMode()
        {
            _inDriveMode = false;
            _forkliftCam.Priority = 9;            
            _driverModel.SetActive(false);
            onDriveModeExited?.Invoke();
            
        }

        private void Update()
        {
            if (_inDriveMode == true)
            {
                LiftControls();
                CalcutateMovement();
                //if (Input.GetKeyDown(KeyCode.Escape))
                //if (Keyboard.current.escapeKey.wasPressedThisFrame || (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame))
                if (_newInputControl.Forklift.Exit.WasPressedThisFrame())
                    ExitDriveMode();
            }
        }

        //New InputSystem
        private void CalcutateMovement()
        {
            var moveDirection = _newInputControl.Forklift.Drive.ReadValue<Vector2>();

            var direction = new Vector3(0, 0, moveDirection.y);
            var velocity = direction * _speed;

            transform.Translate(velocity * Time.deltaTime);

            if (Mathf.Abs(moveDirection.y) > 0)
            {
                var tempRot = transform.rotation.eulerAngles;
                tempRot.y += moveDirection.x * _speed / 2;
                transform.rotation = Quaternion.Euler(tempRot);
            }
        }

        /*
        private void CalcutateMovement()
        {
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            var direction = new Vector3(0, 0, v);
            var velocity = direction * _speed;

            transform.Translate(velocity * Time.deltaTime);

            if (Mathf.Abs(v) > 0)
            {
                var tempRot = transform.rotation.eulerAngles;
                tempRot.y += h * _speed / 2;
                transform.rotation = Quaternion.Euler(tempRot);
            }
        }
        */

        private void LiftControls()
        {
            //if (Input.GetKey(KeyCode.R))
            //if (Keyboard.current.rKey.isPressed || (Gamepad.current != null && Gamepad.current.buttonWest.wasPressedThisFrame))
            if (_newInputControl.Forklift.LiftUp.IsPressed())
                LiftUpRoutine();
            //else if (Input.GetKey(KeyCode.T))
            //else if (Keyboard.current.tKey.isPressed || (Gamepad.current != null && Gamepad.current.buttonEast.wasPressedThisFrame))
            if (_newInputControl.Forklift.LiftDown.IsPressed())
                LiftDownRoutine();
        }

        private void LiftUpRoutine()
        {
            if (_lift.transform.localPosition.y < _liftUpperLimit.y)
            {
                Vector3 tempPos = _lift.transform.localPosition;
                tempPos.y += Time.deltaTime * _liftSpeed;
                _lift.transform.localPosition = new Vector3(tempPos.x, tempPos.y, tempPos.z);
            }
            else if (_lift.transform.localPosition.y >= _liftUpperLimit.y)
                _lift.transform.localPosition = _liftUpperLimit;
        }

        private void LiftDownRoutine()
        {
            if (_lift.transform.localPosition.y > _liftLowerLimit.y)
            {
                Vector3 tempPos = _lift.transform.localPosition;
                tempPos.y -= Time.deltaTime * _liftSpeed;
                _lift.transform.localPosition = new Vector3(tempPos.x, tempPos.y, tempPos.z);
            }
            else if (_lift.transform.localPosition.y <= _liftUpperLimit.y)
                _lift.transform.localPosition = _liftLowerLimit;
        }

        private void OnDisable()
        {
            InteractableZone.onZoneInteractionComplete -= EnterDriveMode;
        }

    }
}