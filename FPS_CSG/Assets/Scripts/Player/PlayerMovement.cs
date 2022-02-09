using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController)), SelectionBase]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField, Tooltip("How fast the player moves in meters per second")]
    private float MovementSpeed = 3.61f;//movement speed in half life 2 is 3,61 m/s
    [SerializeField, Tooltip("How fast the player sprints in meters per second")]
    private float SprintingSpeed = 5.42f;//movement speed * 1.5 rounded up
    [SerializeField, Tooltip("How fast the player moves when crouching in meters per second")]
    private float CrouchedSpeed = 1.56f;
    [SerializeField, Tooltip("How tall the player is when crouching (in units)")]
    private float CrouchHeight = 1f;
    [SerializeField, Tooltip("How fast the player can push objects")]
    private float PushPower = 2f;
    [Header("Jumping")]
    [SerializeField, Tooltip("How high (in meters) the player jumps")]
    private float JumpHeight = 1f;
    [SerializeField, Tooltip("How fast the player falls when a jump has reached its top")]
    private float FallMultiplier = 2.5f;
    [SerializeField, Tooltip("How fast the player falls when a jump has not reached its top")]
    private float LowJumpMultiplier = 2f;
    [SerializeField, Tooltip("How many jumps the player can do before needing to touch the ground")]
    private int MaxJumps = 1;
    [Header("Mouse look")]
    [SerializeField, Tooltip("Mouse sensitivity")]
    private float sensitivity = 0.01f;
    [SerializeField, Tooltip("Maximum up and down viewing angle")]
    private float MaxViewAngle = 75f;
    [SerializeField, Tooltip("Main camera for the player's view")]
    private Camera ViewCam;
    [Header("Misc")]
    [SerializeField, Tooltip("(kg)How much the player weighs, used for gravity and pushing")]
    private float Mass = 1f;
    [SerializeField, Tooltip("Additional colliders to use for collision testing")]
    private BoxCollider[] AdditionalColliders;
    [SerializeField, Tooltip("Audio source for player sounds")]
    private AudioSource PlayerAudioSource;
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
    [Header("Debug")]
    [SerializeField]
    private bool NoClip = false;

    private bool _NoClip;
    private bool crouchBlock = false;//block the player from standing up if not possible
    private CharacterController _Controller;
    private int _CurrentJumps = 0;
    private Vector2 _Move, _MouseDelta;
    private float _MouseX, _MouseY, _GravityVelocity;
    private float _StandingHeight, _CurrentMovementSpeed, _CrouchOffset, _CamStandHeight;
    private float _FootStepTimer = 0;
    private float _GetCurrentStepOffset => _isCrouching ? BaseStepDistance * CrouchStepMultiplier : _isSprinting ? BaseStepDistance * SprintStepMultiplier : BaseStepDistance;

    private float _VerticalVelocity, _Gravity, _JumpValue;//gravity physics
    private bool _Grounded, _isCrouching, _isSprinting, _JumpRequest, _JumpRequesting;

    private void Awake()
    {
        //lock mouse
        Cursor.lockState = CursorLockMode.Locked;

        //get external components
        _Controller = GetComponent<CharacterController>();

        //set private values
        _NoClip = NoClip;
        _Gravity = Physics.gravity.y;
        _JumpValue = Mathf.Sqrt(JumpHeight * -2.0f * _Gravity);
        _CurrentMovementSpeed = MovementSpeed;
        _StandingHeight = _Controller.height;
        _CrouchOffset = CrouchHeight / _StandingHeight;
        _CamStandHeight = ViewCam.transform.position.y - transform.position.y;
    }
    // Update is called once per frame
    void Update()
    {
        if (_JumpRequest)
        {
            Debug.Log(_JumpRequest);
        }
        HandleInput();
    }
    private void HandleInput()
    {
        movement();
        HandleMovementSound();
    }
    private void LateUpdate()
    {
        MouseMovement();
    }
    #region------PRIVATE METHODS------

    private void MouseMovement()
    {
        _MouseX += (_MouseDelta.x * sensitivity);
        _MouseY -= (_MouseDelta.y * sensitivity);
        _MouseY = Mathf.Clamp(_MouseY - (_MouseDelta.y * sensitivity), -MaxViewAngle, MaxViewAngle);

        //camera looking
        transform.localEulerAngles = new Vector3(0, _MouseX, 0);
        ViewCam.transform.localEulerAngles = new Vector3(_MouseY, 0, 0);
    }

    private void movement()
    {
        _Grounded = _Controller.isGrounded;


        if (_Grounded && _VerticalVelocity < 0)
        {
            //prevent falling from ever increasing
            _VerticalVelocity = 0f;
        }

        #region Crouch and Sprint handling
        if (_isSprinting)
        {   //set moving speed to sprinting speed
            _CurrentMovementSpeed = SprintingSpeed;
        }

        if (_isCrouching)
        {
            //shorten player, reposition camera and set moving speed to crouching speed
            _Controller.height = CrouchHeight;
            _CurrentMovementSpeed = CrouchedSpeed;
            ViewCam.transform.position = new Vector3(ViewCam.transform.position.x, transform.position.y + (_Controller.height - _CrouchOffset), ViewCam.transform.position.z);
            Debug.DrawLine(ViewCam.transform.position, ViewCam.transform.position + (Vector3.up * CrouchHeight), Color.red);

        }
        else
        {
            //set player back to regular size
            //raycast, if true crouchblock = true. else, crouchblock is false and stand back up
            if (Physics.Raycast(ViewCam.transform.position, Vector3.up, CrouchHeight, gameObject.layer))
            {
                crouchBlock = true;
            }
            else
            {
                _Controller.height = _StandingHeight;
                ViewCam.transform.position = new Vector3(ViewCam.transform.position.x, transform.position.y + _CamStandHeight, ViewCam.transform.position.z);
                crouchBlock = false;
            }
        }
        if (!_isCrouching && !_isSprinting)
        {
            //set movementspeed to normal speed when neither sprinting nor crouching
            if (!crouchBlock)
            {
                _CurrentMovementSpeed = MovementSpeed;
            }
        }
        #endregion
        //movement
        if (_NoClip)
        {
            //move via transform wherever the player is looking, disable collisions
            Vector3 DMove = ((transform.right * _Move.x) + (ViewCam.transform.forward * _Move.y)).normalized;
            DMove *= _CurrentMovementSpeed * Time.deltaTime;
            //            Debug.DrawRay(transform.position, DMove, Color.red);
            if (_JumpRequest)
            {
                DMove += ViewCam.transform.up * _JumpValue * Time.deltaTime;
            }
            transform.position += DMove * 2;
        }
        else
        {
            //collisions enabled, move via CharacterController where the player is looking on the horizontal plane
            Vector3 move = ((transform.right * _Move.x) + (transform.forward * _Move.y)).normalized;
            move *= (_CurrentMovementSpeed);
            _VerticalVelocity += (_Gravity * Mass) * Time.deltaTime;
            //move handling
            move.y = _VerticalVelocity;
            //Debug.DrawRay(transform.position, move, Color.red);
            _Controller.Move(move * Time.deltaTime + (Vector3.up * _GravityVelocity));
        }
    }

    private void HandleMovementSound()
    {
        if (!WalkSoundHandler) { return; }//no footstep sounds if the handler does not exist
        if (!_Controller.isGrounded) { return; } //no footstep sounds if in the air
        if (_Move.x == 0 && _Move.y == 0) { return; }//standing still

        _FootStepTimer -= Time.deltaTime;

        if (_FootStepTimer <= 0)
        {
            WalkSoundHandler.PlayMaterialWalkSound(ViewCam, _Controller.height, PlayerAudioSource, gameObject.layer);
            _FootStepTimer = _GetCurrentStepOffset;
        }
    }
    #endregion
    #region------PUBLIC METHODS------

    public void Teleport(Vector3 newPosition)
    {
        //set player position to transform position
        this.transform.position = newPosition;
    }
    #endregion
    #region ------EVENTS------
    //----Regular Input Actions
    public void OnMovement(InputAction.CallbackContext ctx)
    {
        _Move = ctx.ReadValue<Vector2>();
        //Debug.Log($"move: {_Move}");
    }

    public void OnMouseMovement(InputAction.CallbackContext ctx)
    {
        //TODO: fix controller slow when moving
        _MouseDelta = ctx.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext ctx)
    {
        _CurrentJumps = _Grounded ? 0 : _CurrentJumps;
        _JumpRequest = ctx.started;
        if (_JumpRequest)
        {
            _CurrentJumps++;//multi jump functionality
        }
        if (ctx.performed)
        {
            _JumpRequesting = true;
        }
        if (ctx.canceled)
        {
            _JumpRequesting = false;
        }
        #region jump handling
        if (_JumpRequest && (_Grounded || _CurrentJumps < MaxJumps))
        {
            //jump if the player has not reached the max jump amount yet
            _VerticalVelocity = _JumpValue;
            _FootStepTimer = 0;//make step noise when landing
        }
        if (_Controller.velocity.y < 0 && !_Grounded)
        {
            //start falling if the player has reached the apex of the jump
            _VerticalVelocity -= FallMultiplier * Time.deltaTime;
        }
        else if (_Controller.velocity.y > 0 && !_JumpRequesting)
        {
            //fall quicker if the player has let go of jump before reaching the apex
            _VerticalVelocity -= LowJumpMultiplier * Time.deltaTime;
        }
        if ((_Controller.collisionFlags & CollisionFlags.Above) != 0)
        {
            //player head bumping, prevent player from sticking to ceiling when jumpingin low areas
            if (_VerticalVelocity > 0)
            {
                _VerticalVelocity = 0;
            }
        }
        #endregion
    }
    public void OnCrouch(InputAction.CallbackContext ctx)
    {
        _isCrouching = (ctx.started || ctx.performed);
    }
    public void OnCrouchToggle(InputAction.CallbackContext ctx)
    {
        _isCrouching = ctx.started ? !_isCrouching : _isCrouching;
    }
    public void OnSprint(InputAction.CallbackContext ctx)
    {
        _isSprinting = (ctx.started || ctx.performed);
    }
    public void OnSprintToggle(InputAction.CallbackContext ctx)
    {
        _isSprinting = ctx.started ? !_isSprinting : _isSprinting;
    }
    //----Debug Input Actions----
    public void OnDebugBreak(InputAction.CallbackContext ctx)
    {
        //TODO: fire debug break event
        if (ctx.started)
        {
#if UNITY_EDITOR
            Debug.Break();
#endif
        }
    }
    public void OnDebugNoClip(InputAction.CallbackContext ctx)
    {
        //TODO: fire NoClip event
        if (ctx.started)
        {
            _NoClip = !_NoClip;
            Debug.Log("No Clip: " + _NoClip);
            _Controller.enabled = !_NoClip;
        }
    }

    //----Physics Events----
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Rigidbody body = hit.collider.attachedRigidbody;
        Vector3 force;
        // no rigidbody
        if (body == null || body.isKinematic) { return; }//don't push if no rigidbody is present
        if (hit.moveDirection.y < -0.3) { return; }//don't push if rigidbody is below player
        /*if (hit.moveDirection.y < -0.3)
        {//push down with mass if rigidbody is below player
            force = new Vector3(0, -0.5f, 0) * _Gravity * Mass;
        }*/
        force = hit.controller.velocity * PushPower;
        // Apply push
        body.AddForceAtPosition(force, hit.point);
    }
    #endregion
}
