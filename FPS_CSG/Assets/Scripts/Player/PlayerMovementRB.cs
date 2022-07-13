using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody)), SelectionBase]
public class PlayerMovementRB : MonoBehaviour
{

    #region ------VARIABLES------
    [Header("Movement")]
    [SerializeField, Tooltip("How fast the player moves in meters per second")]
    private float MovementSpeed = 3.61f;//movement speed in half life 2 is 3,61 m/s
    [SerializeField, Tooltip("How fast the player sprints in meters per second")]
    private float SprintingSpeed = 5.42f;//movement speed * 1.5 rounded up
    [SerializeField, Tooltip("How fast the player moves when crouching in meters per second")]
    private float CrouchedSpeed = 1.56f;
    [SerializeField, Tooltip("How tall the player is when crouching (in units)")]
    private float CrouchHeight = 1f;
    [Header("Jumping")]
    [SerializeField, Tooltip("How high (in meters) the player jumps")]
    private float JumpHeight = 1f;
    [SerializeField, Tooltip("How fast the player falls when a jump has reached its top")]
    private float FallMultiplier = 2.5f;
    [SerializeField, Tooltip("How fast the player falls when a jump has not reached its top")]
    private float LowJumpMultiplier = 2f;
    [SerializeField, Tooltip("How many jumps the player can do before needing to touch the ground")]
    private int MaxJumps = 1;
    [SerializeField, Tooltip("How long (in miliseconds) can the player still jump while in the air before falling")]
    private float CoyoteTime = 1000f;//1 second
    [Header("Mouse Look")]
    [SerializeField, Tooltip("Mouse sensitivity")]
    private float sensitivity = 1f;
    [SerializeField, Tooltip("Maximum up and down viewing angle")]
    private float MaxViewAngle = 75f;
    [SerializeField, Tooltip("Main camera for the player's view")]
    private Camera ViewCam;
    [Header("Walking Sound")]
    [SerializeField, Tooltip("Audio source for player sounds")]
    private AudioSource PlayerAudioSource;
    //[SerializeField, Tooltip("material lookup table for sounds to make when moving over them")]
    //private MaterialWalkScriptableObject MaterialsLookup;
    [SerializeField, Tooltip("Material Audio handler for footsteps")]
    private BaseWalkSound WalkSoundHandler;
    [SerializeField, Tooltip("Distance between step sounds when walking")]
    private float BaseStepDistance = 0.5f;
    [SerializeField, Tooltip("Multiplier on distance between step sounds when crouching")]
    private float CrouchStepMultiplier = 1.5f;
    [SerializeField, Tooltip("Multiplier on distance between step sounds when Sprinting")]
    private float SprintStepMultiplier = 0.6f;

    [Header("Misc")]
    [SerializeField, Tooltip("Distance for grounded checking")]
    private float GroundHeightCheck = 0.1f;
    [SerializeField, Tooltip("Radius for the capsule to check during the grounded check")]
    private float GroundedCheckradius = 0.18f;
    [Header("Debug")]
    [SerializeField]
    private bool NoClip = false;

    //Components
    private Rigidbody _Rb;
    private Collider _PlayerCollider;

    //fields
    private Vector2 _MoveInput, _MouseDelta;
    private float _MouseX, _MouseY;
    private bool _JumpRequest, _JumpRequesting;//request is at the start, requesting is when held down
    private float _CurrentMovementSpeed;
    private bool _IsCrouching, _IsSprinting;
    private bool _CrouchBlocked = false;//block the player from standing up if not possible
    private int _CurrentJumps = 0;
    private float _FootStepTimer = 0;
    private bool _IsGrounded, _HitHead;

    private float _GetCurrentStepOffset => _IsCrouching ? BaseStepDistance * CrouchStepMultiplier : _IsSprinting ? BaseStepDistance * SprintStepMultiplier : BaseStepDistance;

    //private values of public values
    private bool _NoClip;
    #endregion

    private void Awake()
    {
        //lock mouse
        Cursor.lockState = CursorLockMode.Locked;

        //get object components
        _Rb = GetComponent<Rigidbody>();
        _PlayerCollider = GetComponent<Collider>();

        //set private values
        _NoClip = NoClip;
        _CurrentMovementSpeed = MovementSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        //handle keyboard inputs
        HandleInput();
    }
    private void LateUpdate()
    {
        //handle camera looking after the regular movemnet has happened to accomadate for the possibly new camera position
        HandleMouseMovement();
    }

    #region ------PRIVATE METHODS------
    private void HandleInput()
    {
        HandleMovement();//handle movement
        HandleMovementSound();//handle sounds for movement
    }

    private void HandleMovement()
    {
        if (_NoClip)
        {
            //move via transform wherever the player is looking, disable collisions
            Vector3 DMove = ((transform.right * _MoveInput.x) + (ViewCam.transform.forward * _MoveInput.y));
            DMove *= _CurrentMovementSpeed * Time.deltaTime;
            //            Debug.DrawRay(transform.position, DMove, Color.red);
            if (_JumpRequesting || _JumpRequest)
            {
                DMove += ViewCam.transform.up * JumpHeight * Time.deltaTime;
            }
            transform.position += DMove * 2;
        }
        else//non NoClip movement
        {
            Vector3 move = ((transform.right * _MoveInput.x) + (transform.forward * _MoveInput.y));
            _Rb.velocity = new Vector3(move.x * MovementSpeed, _Rb.velocity.y, move.z * MovementSpeed);

            //TODO: check rb velocity.y, if <0 increase gravity multiplier/mass;

            //move *= (_CurrentMovementSpeed);
            //_VerticalVelocity += (_Gravity * Mass) * Time.deltaTime;
            //move handling
            //move.y = _VerticalVelocity;
            //Debug.DrawRay(transform.position, move, Color.red);

            //_Controller.Move(move * Time.deltaTime + (Vector3.up * _GravityVelocity));
            #region jump handling
            if (_JumpRequesting || _JumpRequest)
            {
                _CurrentJumps = _IsGrounded ? 0 : _CurrentJumps;
                Debug.Log("Jumprequest: " + _JumpRequest);
                if (_JumpRequesting)
                {
                    _CurrentJumps++;//multi jump functionality
                    if (_CurrentJumps <= MaxJumps)
                    {
                        _Rb.velocity = new Vector3(_Rb.velocity.x, JumpHeight, _Rb.velocity.z);
                        Debug.Log($"[A]RB.velocity.y= {_Rb.velocity.y}");
                        _FootStepTimer = 0;//make step noise when landing
                    }
                }
                //if (_JumpRequest && (isGrounded || _CurrentJumps < MaxJumps))
                //{
                //    //jump if the player has not reached the max jump amount yet
                //    //_VerticalVelocity = _JumpValue;
                //
                //    //TODO: force player upwards (rigidbody.addforce?)
                //
                //    _Rb.AddForce(Vector3.up * JumpHeight, ForceMode.Impulse);
                //    //_Rb.AddForce(Vector3.up * CalcJumpVelocity(), ForceMode.Impulse);
                //
                //}
                if (_Rb.velocity.y < 0 && !_IsGrounded)//reached apex of jump
                {
                    //start falling if the player has reached the apex of the jump
                    _Rb.velocity -= Vector3.up * Physics.gravity.y * (FallMultiplier - 1) * Time.deltaTime;
                    //TODO: increase gravity multiplier/mass;
                }
                else if (_Rb.velocity.y > 0 && !_JumpRequesting)//canceled jump earlier
                {
                    //fall quicker if the player has let go of jump before reaching the apex

                    _Rb.velocity -= Vector3.up * Physics.gravity.y * (LowJumpMultiplier - 1) * Time.deltaTime;
                    //TODO: increase gravity multiplier/mass;

                }
                if (_HitHead)
                {
                    //player head bumping, prevent player from sticking to ceiling when jumpingin low areas
                    _Rb.velocity = new Vector3(_Rb.velocity.x, 0, _Rb.velocity.z);
                }
                //Debug.Log($"RB.velocity.y= {_Rb.velocity.y}");
            }
            #endregion
        }
    }

    private void HandleMouseMovement()
    {
        _MouseX += (_MouseDelta.x * sensitivity);
        _MouseY -= (_MouseDelta.y * sensitivity);
        _MouseY = Mathf.Clamp(_MouseY - (_MouseDelta.y * sensitivity), -MaxViewAngle, MaxViewAngle);

        //camera looking
        transform.localEulerAngles = new Vector3(0, _MouseX, 0);
        ViewCam.transform.localEulerAngles = new Vector3(_MouseY, 0, 0);
    }

    private void HandleMovementSound()
    {
        if (!WalkSoundHandler) { return; }//no footstep sounds if the handler does not exist
        if (!_IsGrounded) { return; } //no footstep sounds if in the air
        if (_MoveInput.x == 0 && _MoveInput.y == 0) { return; }//standing still

        _FootStepTimer -= Time.deltaTime;

        if (_FootStepTimer <= 0)
        {
            WalkSoundHandler.PlayMaterialWalkSound(ViewCam, _PlayerCollider.bounds.size.y, PlayerAudioSource, gameObject.layer);
            _FootStepTimer = _GetCurrentStepOffset;
        }
    }

    #endregion
    #region ------PUBLIC EVENTS------
    public void Teleport(Vector3 newPosition)
    {
        //set player position to transform position
        this.transform.position = newPosition;
        _Rb.velocity = Vector3.zero;
    }
    #endregion
    #region ------RETURN METHODS------
    /// <summary>Returns whether the player is grounded or not.</summary>
    private bool IsGrounded()
    {
        if (_PlayerCollider != null)
        {
            //TODO: accomodata for coyote jump
            //returns if the player "should" be grounded.
            //bool res = Physics.CheckCapsule(_PlayerCollider.bounds.center, new Vector3(_PlayerCollider.bounds.center.x, _PlayerCollider.bounds.min.y - GroundHeightCheck, _PlayerCollider.bounds.center.z), GroundedCheckradius);
            Vector3 Point1 = transform.position - Vector3.up * (_PlayerCollider.bounds.extents.y - 0.01f);
            bool res = Physics.SphereCast(Point1, GroundedCheckradius, -Vector3.up, out RaycastHit hit, 1f);
            Debug.DrawRay(Point1, -Vector3.up);
            if (res)
            {
                Debug.Log($"Terra Firma ({hit.collider})");
            }
            return res;
        }
        return false;
    }

    /// <summary>Returns whether the player has a collider right above their head.</summary>
    private bool HitHead()
    {
        if (_PlayerCollider != null)
        {
            //returns if the raycast hit something right above the player, "hitting their head", make the radius a smidge smaller to prevent walls being an issue
            bool res = Physics.CheckCapsule(new Vector3(transform.position.x, _PlayerCollider.bounds.max.y + 0.001f, transform.position.z), new Vector3(transform.position.x, _PlayerCollider.bounds.max.y + 0.01f, transform.position.z), _PlayerCollider.bounds.size.x - 0.01f);
            if (res)
            {
                Debug.Log("Bonk");
            }
            return res;
        }
        return false;
    }
    private float CalcJumpVelocity()
    {
        float v;
        float t = Time.deltaTime;
        float g = Physics.gravity.y;
        float h = JumpHeight;

        v = (t - (1f / ((2f * g) * (t * t)))) * h;
        Debug.Log($"JumpVelocity: {v} (t:{t}|g:{g}|h;{h})");
        return v;

    }

    #endregion
    #region ------EVENTS------
    public void OnMovement(InputAction.CallbackContext ctx)
    {
        _MoveInput = ctx.ReadValue<Vector2>();
        //Debug.Log($"move: {_Move}");
    }

    public void OnMouseMovement(InputAction.CallbackContext ctx)
    {
        //TODO: fix controller slow when moving
        _MouseDelta = ctx.ReadValue<Vector2>();
    }
    public void OnJump(InputAction.CallbackContext ctx)
    {
        _JumpRequest = ctx.started;
        if (ctx.started)
        {
            Debug.Log("Started");
        }
        if (ctx.performed)
        {
            _JumpRequesting = true;
        }
        if (ctx.canceled)
        {
            _JumpRequesting = false;
        }

    }
    public void OnCrouch(InputAction.CallbackContext ctx)
    {
        //_isCrouching = (ctx.started || ctx.performed);
    }
    public void OnCrouchToggle(InputAction.CallbackContext ctx)
    {
        //_isCrouching = ctx.started ? !_isCrouching : _isCrouching;
    }
    public void OnSprint(InputAction.CallbackContext ctx)
    {
        //_isSprinting = (ctx.started || ctx.performed);
    }
    public void OnSprintToggle(InputAction.CallbackContext ctx)
    {
        //_isSprinting = ctx.started ? !_isSprinting : _isSprinting;
    }
    public void OnDebugNoClip(InputAction.CallbackContext ctx)
    {
        //TODO: fire NoClip event
        if (ctx.started)
        {
            _NoClip = !_NoClip;
            Debug.Log("No Clip: " + _NoClip);
            //disable collisions
            _Rb.detectCollisions = !_NoClip;
            //disable gravity
            _Rb.useGravity = !_NoClip;
            //freeze all rotations when noclipping, otherwise only freeze X and Z rotation
            _Rb.constraints = _NoClip == true ? RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ : RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

            //_PlayerCollider.enabled = !_NoClip;
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        //TODO: DOESN'T WORK
        float ThirdOffset = (_PlayerCollider as CapsuleCollider).height * 0.33f;
        Vector3 contact = collision.GetContact(0).point;
        if (contact.y < transform.position.y - ThirdOffset)
        {
            //grounded
            _IsGrounded = true;
        }
        if (contact.y > transform.position.y + ThirdOffset)
        {
            //hit head
            _HitHead = true;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        _IsGrounded = false;
        _HitHead = false;
    }
    #endregion
    private void OnDrawGizmos()
    {
        TryGetComponent<Collider>(out Collider col);
        if (col)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(transform.position - new Vector3(0, GroundHeightCheck, 0), new Vector3(GroundedCheckradius, col.bounds.size.y, GroundedCheckradius));
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(transform.position + new Vector3(0, 0.01f, 0), col.bounds.size);
            Gizmos.color = Color.white;
            Gizmos.DrawWireCube(transform.position, col.bounds.size);
        }
    }
}


