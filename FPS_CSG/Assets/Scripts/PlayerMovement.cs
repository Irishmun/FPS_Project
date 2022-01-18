using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController)), SelectionBase]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField, Tooltip("Should the user's mouse be locked and hidden at the start of the game")]
    private bool LockMouse = true;
    [Header("Movement")]
    [SerializeField, Tooltip("How fast the player moves in meters per second")]
    private float MovementSpeed = 3.61f;//movement speed in half life 2 is 3,61 m/s
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
        //lock mouse if needed
        Cursor.lockState = LockMouse ? CursorLockMode.Locked : CursorLockMode.None;

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
    private void Start()
    {
        ViewCam.transform.localEulerAngles = new Vector3(0, ViewCam.transform.rotation.y, 0);
    }
    // Update is called once per frame
    void Update()
    {
        HandleInput();

    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        //TODO: Fix janky pushing
        Rigidbody body = hit.collider.attachedRigidbody;

        //no rigidbody on collision
        if (body == null || body.isKinematic) return;
        //dont push objects below controller
        if (hit.moveDirection.y < -0.3) return;

        Vector3 pushDir = new Vector3(hit.moveDirection.x, 0, hit.moveDirection.z);

        //push the object with strength
        body.velocity = (pushDir * (_Controller.velocity.magnitude * PushPower)) / body.mass;
    }

    private void HandleInput()
    {
        debugValues();
        movement();
    }
    private void debugValues()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            MovementSpeed++;
        }
        if (Input.GetKeyDown(KeyCode.L))
        {
            MovementSpeed--;
        }

        if (Input.GetKeyDown(KeyCode.B))
        {
            Debug.Break();
        }
        if (Input.GetKeyDown(KeyCode.V))
        {
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
            _VerticalVelocity = 0f;
        }
        //camera looking
        transform.localEulerAngles = new Vector3(0, _MouseX, 0);
        ViewCam.transform.localEulerAngles = new Vector3(Mathf.Clamp(_MouseY, -MaxViewAngle, MaxViewAngle), 0, 0);

        #region crouch handling
        if (Input.GetKey(KeyCode.LeftControl))
        {
            _Controller.height = CrouchHeight;
            _CurrentMovementSpeed = CrouchedSpeed;
            ViewCam.transform.position = new Vector3(ViewCam.transform.position.x, transform.position.y + (_Controller.height - _CrouchOffset), ViewCam.transform.position.z); //Vector3.Lerp(ViewCam.transform.position, new Vector3(ViewCam.transform.position.x, transform.position.y + _CrouchOffset, ViewCam.transform.position.z), Time.deltaTime);
            //gameObject.transform.localScale = new Vector3(transform.localScale.x, CrouchHeight / 2, transform.localScale.z);
            /* foreach (BoxCollider col in AdditionalColliders)
             {
                 col.size = new Vector3(col.size.x, CrouchHeight, col.size.z);
             }*/
        }
        else//TODO: fix crouching, changing scale gives cleaner results but messes with child objects. Changing height of controller does not matter as colliders are in the way and child objects (camera) are not repositioned
        {
            _Controller.height = _StandingHeight;
            _CurrentMovementSpeed = MovementSpeed;
            ViewCam.transform.position = new Vector3(ViewCam.transform.position.x, transform.position.y + _CamStandHeight, ViewCam.transform.position.z); //Vector3.Lerp(ViewCam.transform.position, new Vector3(ViewCam.transform.position.x, transform.position.y + _CamStandHeight, ViewCam.transform.position.z), Time.deltaTime);
            //gameObject.transform.localScale = new Vector3(transform.localScale.x, _StandingHeight / 2, transform.localScale.z);
            /*foreach (BoxCollider col in AdditionalColliders)
            {
                col.size = new Vector3(col.size.x, _StandingHeight, col.size.z);
            }*/
        }
        #endregion

        //movement
        if (_NoClip)
        {
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
            Vector3 move = ((transform.right * moveX) + (transform.forward * moveY)).normalized;
            move *= (_CurrentMovementSpeed);
            #region jump handling
            //Debug.Log("Velocity.y: " + Controller.velocity.y.ToString("0.00"));

            if (Input.GetButtonDown("Jump") && (_Grounded || _CurrentJumps < MaxJumps))
            {
                _VerticalVelocity += _JumpValue;
                _CurrentJumps++;//multi jump functionality
            }
            if (_Controller.velocity.y < 0 && !_Grounded)
            {
                _VerticalVelocity -= FallMultiplier * Time.deltaTime;
            }
            else if (_Controller.velocity.y > 0 && !Input.GetButton("Jump"))
            {
                _VerticalVelocity -= LowJumpMultiplier * Time.deltaTime;
            }
            if ((_Controller.collisionFlags & CollisionFlags.Above) != 0)
            {//player head bumping, prevent player from sticking to ceiling when jumpingin low areas
                if (_VerticalVelocity > 0)
                {
                    _VerticalVelocity = 0;
                }
            }
            #endregion
            _VerticalVelocity += _Gravity * Time.deltaTime;
            //move handling
            move.y = _VerticalVelocity;
            Debug.DrawRay(transform.position, move, Color.red);
            _Controller.Move(move * Time.deltaTime + (Vector3.up * _GravityVelocity));
        }
    }
}
