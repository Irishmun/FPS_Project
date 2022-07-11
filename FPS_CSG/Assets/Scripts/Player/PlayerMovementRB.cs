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
    private bool _JumpRequest, _JumpRequesting;

    //private values of public values
    private bool _NoClip;
    private float _CurrentMovementSpeed;
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
        HandleInput();
    }
    private void LateUpdate()
    {
        HandleMouseMovement();
    }

    #region ------PRIVATE METHODS------
    private void HandleInput()
    {
        HandleMovement();
    }

    private void HandleMovement()
    {
        bool isGrounded = IsGrounded();
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

            //move *= (_CurrentMovementSpeed);
            //_VerticalVelocity += (_Gravity * Mass) * Time.deltaTime;
            //move handling
            //move.y = _VerticalVelocity;
            //Debug.DrawRay(transform.position, move, Color.red);

            //_Controller.Move(move * Time.deltaTime + (Vector3.up * _GravityVelocity));
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
    /// <summary>Returns whether the player is grounded or not.</summary>
    private bool IsGrounded()
    {
        if (_PlayerCollider != null)
        {
            //returns if the player "should" be grounded.
            //From dead center, which might want to be checked in multiple spots for coyote jump
            return Physics.CheckCapsule(_PlayerCollider.bounds.center, new Vector3(_PlayerCollider.bounds.center.x, _PlayerCollider.bounds.min.y - GroundHeightCheck, _PlayerCollider.bounds.center.z), GroundedCheckradius);
            //return Physics.Raycast(transform.position, -Vector3.up, _PlayerCollider.bounds.size.y + GroundHeightCheck);

        }
        return false;
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
        //_CurrentJumps = IsGrounded() ? 0 : _CurrentJumps;
        _JumpRequest = ctx.started;
        if (_JumpRequest)
        {
            // _CurrentJumps++;//multi jump functionality
        }
        if (ctx.performed)
        {
            _JumpRequesting = true;
        }
        if (ctx.canceled)
        {
            _JumpRequesting = false;
        }
        //#region jump handling
        //if (_JumpRequest && (_Grounded || _CurrentJumps < MaxJumps))
        //{
        //    //jump if the player has not reached the max jump amount yet
        //    _VerticalVelocity = _JumpValue;
        //    _FootStepTimer = 0;//make step noise when landing
        //}
        //if (_Controller.velocity.y < 0 && !_Grounded)
        //{
        //    //start falling if the player has reached the apex of the jump
        //    _VerticalVelocity -= FallMultiplier * Time.deltaTime;
        //}
        //else if (_Controller.velocity.y > 0 && !_JumpRequesting)
        //{
        //    //fall quicker if the player has let go of jump before reaching the apex
        //    _VerticalVelocity -= LowJumpMultiplier * Time.deltaTime;
        //}
        //if ((_Controller.collisionFlags & CollisionFlags.Above) != 0)
        //{
        //    //player head bumping, prevent player from sticking to ceiling when jumpingin low areas
        //    if (_VerticalVelocity > 0)
        //    {
        //        _VerticalVelocity = 0;
        //    }
        //}
        //#endregion
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
            _Rb.detectCollisions = !_NoClip;
            _Rb.useGravity = !_NoClip;
            _Rb.constraints = _NoClip == true ? RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ : RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

            _PlayerCollider.enabled = !_NoClip;
        }
    }
    #endregion

    private void OnDrawGizmos()
    {
        TryGetComponent<Collider>(out Collider col);
        if (col)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(transform.position - new Vector3(0, GroundHeightCheck, 0), new Vector3(GroundedCheckradius, col.bounds.size.y, GroundedCheckradius));
            Gizmos.color = Color.white;
            Gizmos.DrawWireCube(transform.position, col.bounds.size);
        }
    }
}


