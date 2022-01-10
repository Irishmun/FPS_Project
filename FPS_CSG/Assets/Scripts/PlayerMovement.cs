using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController)), SelectionBase]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField]
    private bool LockMouse = true;
    [SerializeField]
    private float MovementSpeed = 1f;
    [SerializeField]
    private float JumpHeight = 1f;
    [SerializeField]
    private int MaxJumps = 1;
    [SerializeField]
    private float Mass = 2f;
    [SerializeField]
    private float sensitivity = 10;
    [SerializeField]
    private float MaxViewAngle = 89f;
    [SerializeField]
    private Camera ViewCam;

    private int CurrentJumps = 0;
    private CharacterController Controller;
    private float vertical, jumpVelocity;
    private void Awake()
    {
        /*
         //force lower framerate for testing
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 10;*/
        Cursor.lockState = LockMouse ? CursorLockMode.Locked : CursorLockMode.None;
        Controller = GetComponent<CharacterController>();
    }
    // Update is called once per frame
    void Update()
    {
        HandleInput();
    }

    private void HandleInput()
    {
        Vector2 mouse = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")) * sensitivity;
        Vector3 move = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical")) * MovementSpeed * Time.deltaTime;

        vertical -= mouse.y;
        transform.Rotate(0f, mouse.x, 0f);
        ViewCam.transform.localRotation = Quaternion.Euler(Mathf.Clamp(vertical, -MaxViewAngle, MaxViewAngle), 0, 0);

        bool wantJump = Input.GetAxisRaw("Jump") > 0;
        /*if (Controller.isGrounded && wantJump)
        {
            jumpVelocity = JumpHeight * Time.deltaTime;
        }
        else
        {
            jumpVelocity = Physics.gravity.y * Mass * Time.deltaTime;
        }*/ //fix
        move.y = jumpVelocity;

        Controller.Move(move * MovementSpeed);
    }
}
