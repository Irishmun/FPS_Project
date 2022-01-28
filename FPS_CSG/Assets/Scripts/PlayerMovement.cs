using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    [SerializeField, Tooltip("How many jumps the player can do in total")]
    private int MaxJumps = 1;
    [Header("Mouse look")]
    [SerializeField]
    private float sensitivity = 10;
    [SerializeField]
    private float MaxViewAngle = 75f;
    [SerializeField]
    private Camera ViewCam;
    [Header("Misc")]
    [SerializeField, Tooltip("(kg)How much the player weighs, used for gravity and pushing")]
    private float Mass = 1f;
    [SerializeField, Tooltip("Additional colliders to use for collision testing")]
    private BoxCollider[] AdditionalColliders;
    [Header("Debug")]
    [SerializeField]
    private bool NoClip = false;

    private bool _NoClip;

    private CharacterController _Controller;
    private int _CurrentJumps = 0;
    private float _MouseX, _MouseY, _GravityVelocity;
    private float _StandingHeight, _CurrentMovementSpeed, _CrouchOffset, _CamStandHeight;

    private float _VerticalVelocity, _Gravity, _JumpValue;//gravity physics
    private bool _Grounded;

    private void Awake()
    {
        //lock mouse
        Cursor.lockState = CursorLockMode.Locked;

        //get external components
        _Controller = GetComponent<CharacterController>();

        //set private values
        _NoClip = NoClip;
        _Gravity = Physics.gravity.y;
        _JumpValue = Mathf.Sqrt(JumpHeight * -3.0f * _Gravity);
        _CurrentMovementSpeed = MovementSpeed;
        _StandingHeight = _Controller.height;
        _CrouchOffset = CrouchHeight / _StandingHeight;
        _CamStandHeight = ViewCam.transform.position.y - transform.position.y;
    }
    // Update is called once per frame
    void Update()
    {
        HandleInput();
    }
    private void HandleInput()
    {
        debugValues();
        movement();
    }

    private void debugValues()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            Debug.Break();
        }
        if (Input.GetKeyDown(KeyCode.V))
        {
            //toggle noclip mode
            NoClip = !NoClip;
        }
        if (_NoClip != NoClip)
        {
            Debug.Log("No Clip: " + NoClip);
            _Controller.enabled = !NoClip;
            _NoClip = NoClip;
        }
    }

    private void movement()
    {
        _MouseX += Input.GetAxis("Mouse X") * sensitivity;
        _MouseY -= Input.GetAxis("Mouse Y") * sensitivity;

        float moveX = Input.GetAxis("Horizontal");
        float moveY = Input.GetAxis("Vertical");
        _Grounded = _Controller.isGrounded;
        _CurrentJumps = _Grounded ? 0 : _CurrentJumps;

        if (_Grounded && _VerticalVelocity < 0)
        {
            //prevent falling from ever increasing
            _VerticalVelocity = 0f;
        }
        //camera looking
        transform.localEulerAngles = new Vector3(0, _MouseX, 0);
        ViewCam.transform.localEulerAngles = new Vector3(Mathf.Clamp(_MouseY, -MaxViewAngle, MaxViewAngle), 0, 0);

        #region Crouch and Sprint handling
        if (Input.GetKey(KeyCode.LeftShift))
        {   //set moving speed to sprinting speed
            _CurrentMovementSpeed = SprintingSpeed;
        }

        if (Input.GetKey(KeyCode.LeftControl))
        {
            //shorten player, reposition camera and set moving speed to crouching speed
            _Controller.height = CrouchHeight;
            _CurrentMovementSpeed = CrouchedSpeed;
            ViewCam.transform.position = new Vector3(ViewCam.transform.position.x, transform.position.y + (_Controller.height - _CrouchOffset), ViewCam.transform.position.z);

        }
        else
        {
            //set player back to regular size
            _Controller.height = _StandingHeight;
            ViewCam.transform.position = new Vector3(ViewCam.transform.position.x, transform.position.y + _CamStandHeight, ViewCam.transform.position.z);
        }
        if (!Input.GetKey(KeyCode.LeftControl) && !Input.GetKey(KeyCode.LeftShift))
        {
            //set movementspeed to normal speed when neither sprinting nor crouching
            _CurrentMovementSpeed = MovementSpeed;
        }
        #endregion
        //movement
        if (_NoClip)
        {
            //move via transform wherever the player is looking, disable collisions
            Vector3 DMove = ((transform.right * moveX) + (ViewCam.transform.forward * moveY)).normalized;
            DMove *= _CurrentMovementSpeed * Time.deltaTime;
            Debug.DrawRay(transform.position, DMove, Color.red);
            if (Input.GetButton("Jump"))
            {
                DMove += ViewCam.transform.up * _JumpValue * Time.deltaTime;

            }
            transform.position += DMove * 2;
        }
        else
        {
            //collisions enabled, move via CharacterController where the player is looking on the horizontal plane
            Vector3 move = ((transform.right * moveX) + (transform.forward * moveY)).normalized;
            move *= (_CurrentMovementSpeed);
            #region jump handling
            if (Input.GetButtonDown("Jump") && (_Grounded || _CurrentJumps < MaxJumps))
            {
                //jump if the player has not reached the max jump amount yet
                _VerticalVelocity += _JumpValue;
                _CurrentJumps++;//multi jump functionality
            }
            if (_Controller.velocity.y < 0 && !_Grounded)
            {
                //start falling if the player has reached the apex of the jump
                _VerticalVelocity -= FallMultiplier * Time.deltaTime;
            }
            else if (_Controller.velocity.y > 0 && !Input.GetButton("Jump"))
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
            _VerticalVelocity += (_Gravity * Mass) * Time.deltaTime;
            //move handling
            move.y = _VerticalVelocity;
            //Debug.DrawRay(transform.position, move, Color.red);
            _Controller.Move(move * Time.deltaTime + (Vector3.up * _GravityVelocity));
        }
    }

    public void Teleport(Vector3 newPosition)
    {
        //set player position to transform position
        this.transform.position = newPosition;
    }

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
}
