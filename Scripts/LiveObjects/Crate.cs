using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Scripts.LiveObjects
{
    public class Crate : MonoBehaviour
    {
        [SerializeField] private float _punchDelay;
        [SerializeField] private GameObject _wholeCrate, _brokenCrate;
        [SerializeField] private Rigidbody[] _pieces;
        [SerializeField] private BoxCollider _crateCollider;
        [SerializeField] private InteractableZone _interactableZone;
        private bool _isReadyToBreak = false;


        private float time = 0;
        private float timer = 3f;
        private float holdTimer = 0;
        private bool _strongPunch = false;
        
        //Reference to New InputSystem Action Map
        private Player_Controls _newInputControl;

        private List<Rigidbody> _brakeOff = new List<Rigidbody>();

        private void OnEnable()
        {
            InteractableZone.onZoneInteractionComplete += InteractableZone_onZoneInteractionComplete;
        }

        private void InteractableZone_onZoneInteractionComplete(InteractableZone zone)
        {
            if (_isReadyToBreak == false && _brakeOff.Count >0)
            {
                _wholeCrate.SetActive(false);
                _brokenCrate.SetActive(true);
                _isReadyToBreak = true;
            }

            if (_isReadyToBreak && zone.GetZoneID() == 6) //Crate zone            
            {
                if (_brakeOff.Count > 0)
                {
                    /*
                    if (_strongPunch == true)
                    {
                        BreakPartStrong();
                    }
                    else if (_strongPunch == false)
                    {
                        BreakPart();
                    }
                    */

                    BreakPart();
                    StartCoroutine(PunchDelay());
                }
                else if(_brakeOff.Count == 0)
                {
                    _isReadyToBreak = false;
                    _crateCollider.enabled = false;
                    _interactableZone.CompleteTask(6);
                    Debug.Log("Completely Busted");
                }
            }
        }

        private void Start()
        {
            _brakeOff.AddRange(_pieces);

            _newInputControl = new Player_Controls();
            _newInputControl.Player.Enable();

            _newInputControl.Player.Punch.started += OnPunchStarted;
            _newInputControl.Player.Punch.performed += OnPunchPerformed;
            _newInputControl.Player.Punch.canceled += OnPunchCanceled;
        }

        private void OnPunchStarted(InputAction.CallbackContext context)
        {
            _strongPunch = true;
        }

        private void OnPunchPerformed(InputAction.CallbackContext context)
        {
            _strongPunch = false;
            if (Time.time > time)
            {
                time = Time.time + timer;
                BreakPartStrong();
            }
        }

        private void OnPunchCanceled(InputAction.CallbackContext context)
        {
            if (_strongPunch)
            {
                _strongPunch = false;
                BreakPart();
                time = 0f;
            }
        }

        private void Update()
        {
            /*
            if (_newInputControl.Player.Punch.IsPressed() && _strongPunch == false)
            {
                holdTimer += Time.deltaTime;

                if (holdTimer > 1f)
                {
                    if (Time.time > time)
                    {
                        time = Time.time + timer;

                        _strongPunch = true;
                        BreakPartStrong();
                    }
                }
            }
            else if (_newInputControl.Player.Punch.WasPerformedThisFrame())
            {
                if (holdTimer < 1f)
                {
                    BreakPart();
                    time = 0f;
                }
                holdTimer = 0;
                _strongPunch = false;
            }
            */
        }

        public void BreakPart()
        {
            int rng = Random.Range(0, _brakeOff.Count);
            _brakeOff[rng].constraints = RigidbodyConstraints.None;
            _brakeOff[rng].AddForce(new Vector3(1f, 1f, 1f), ForceMode.Force);
            _brakeOff.Remove(_brakeOff[rng]);            
        }

        public void BreakPartStrong() 
        {
            if (_brakeOff.Count == 0) 
            return;

            int breakCount = Random.Range(3, 4);

            for (int i = 0; i < breakCount; i++)
            {
                int rng = Random.Range(0, _brakeOff.Count);
                _brakeOff[rng].constraints = RigidbodyConstraints.None;
                _brakeOff[rng].AddForce(new Vector3(1f, 1f, 1f), ForceMode.Force);
                _brakeOff.Remove(_brakeOff[rng]);
            }
        }

        IEnumerator PunchDelay()
        {
            float delayTimer = 0;
            while (delayTimer < _punchDelay)
            {
                yield return new WaitForEndOfFrame();
                delayTimer += Time.deltaTime;
            }

            _interactableZone.ResetAction(6);
        }

        private void OnDisable()
        {
            InteractableZone.onZoneInteractionComplete -= InteractableZone_onZoneInteractionComplete;
        }
    }
}
