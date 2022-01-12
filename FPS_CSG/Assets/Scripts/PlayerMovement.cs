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
    private float MaxViewAngle = 89f;
    [SerializeField]
    private Camera ViewCam;

    [Header("Debug")]
    [SerializeField]
    private bool NoClip = false;

    private bool _NoClip;

    private CharacterController Controller;
    private int CurrentJumps = 0;
    private float MouseX, MouseY, GravityVelocity;

    private float PlayerVelocity, Gravity, JumpValue;
    private bool Grounded;

    private void Awake()
    {
        _NoClip = NoClip;
        Gravity = Physics.gravity.y;
        JumpValue = Mathf.Sqrt(JumpHeight * -3.0f * Gravity);
        //force lower framerate for testing
        QualitySettings.vSyncCount = 1;
        Application.targetFrameRate = -1;
        Cursor.lockState = LockMouse ? CursorLockMode.Locked : CursorLockMode.None;
        Controller = GetComponent<CharacterController>();
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
            Controller.enabled = !NoClip;
            _NoClip = NoClip;
        }
    }
    private void movement()
    {
        MouseX += Input.GetAxis("Mouse X") * sensitivity;
        MouseY -= Input.GetAxis("Mouse Y") * sensitivity;

        float moveX = Input.GetAxis("Horizontal");
        float moveY = Input.GetAxis("Vertical");
        Grounded = Controller.isGrounded;
        CurrentJumps = Grounded ? 0 : CurrentJumps;

        if (Grounded && PlayerVelocity < 0)
        {
            PlayerVelocity = 0f;
        }
        //camera looking
        transform.localEulerAngles = new Vector3(0, MouseX, 0);
        ViewCam.transform.localEulerAngles = new Vector3(Mathf.Clamp(MouseY, -MaxViewAngle, MaxViewAngle), 0, 0);

        //movement
        if (_NoClip)
        {
            Vector3 DMove = ((transform.right * moveX) + (ViewCam.transform.forward * moveY)).normalized;
            DMove *= MovementSpeed * Time.deltaTime;
            Debug.DrawRay(transform.position, DMove, Color.red);
            if (Input.GetButton("Jump"))
            {
                DMove += ViewCam.transform.up * JumpValue * Time.deltaTime;

            }
            transform.position += DMove * 2;
        }
        else
        {
            Vector3 move = ((transform.right * moveX) + (transform.forward * moveY)).normalized;
            move *= (MovementSpeed * 2);

            //jump handling
            //Debug.Log("Velocity.y: " + Controller.velocity.y.ToString("0.00"));

            if (Input.GetButtonDown("Jump") && (Grounded || CurrentJumps < MaxJumps))
            {
                PlayerVelocity += JumpValue;
                CurrentJumps++;//multi jump functionality
            }
            if (Controller.velocity.y < 0 && !Grounded)
            {
                PlayerVelocity -= FallMultiplier * Time.deltaTime;
            }
            else if (Controller.velocity.y > 0 && !Input.GetButton("Jump"))
            {
                PlayerVelocity -= LowJumpMultiplier * Time.deltaTime;
            }
            if ((Controller.collisionFlags & CollisionFlags.Above) != 0)
            {//player head bumping, prevent player from sticking to ceiling when jumpingin low areas
                if (PlayerVelocity > 0)
                {
                    PlayerVelocity = 0;
                }
            }
            PlayerVelocity += Gravity * Time.deltaTime;
            //move handling
            move.y = PlayerVelocity;
            Debug.DrawRay(transform.position, move, Color.red);
            Controller.Move(move * Time.deltaTime + (Vector3.up * GravityVelocity));
        }
    }
}
