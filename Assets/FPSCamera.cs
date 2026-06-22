using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class FPSController : MonoBehaviour
{
    public Camera playerCamera;
    public float walkSpeed = 6f;
    public float runSpeed = 12f;

    public float lookSpeed = 2f;
    public float lookXLimit = 45f;
    public bool useGravity = true;
    public float gravity = 9.81f;
    private float verticalVelocity = 0f;
    Vector3 moveDirection = Vector3.zero;
    float rotationX = 0;
    public float jumpHeight = 3f;
    public bool canMove = true;


    CharacterController characterController;
    void Start()
    {
        characterController = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {

        #region Handles Movment

        Vector3 forward = playerCamera.transform.forward;
        Vector3 right = playerCamera.transform.right;

        forward.y = 0;
        right.y = 0;

        forward.Normalize();
        right.Normalize();

        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        float speed = isRunning ? runSpeed : walkSpeed;

        float vertical = Input.GetAxis("Vertical");
        float horizontal = Input.GetAxis("Horizontal");

        Vector3 move;

         if (Input.GetKey(KeyCode.Z))
               useGravity = !useGravity;
        if (useGravity)
        {
            forward = playerCamera.transform.forward;
            right = playerCamera.transform.right;

            forward.y = 0;
            right.y = 0;

            forward.Normalize();
            right.Normalize();

            move = (forward * vertical + right * horizontal) * speed;

            if (characterController.isGrounded)
            {
                if (verticalVelocity < 0)
                    verticalVelocity = -2f;

                if (Input.GetButtonDown("Jump"))
                    verticalVelocity = Mathf.Sqrt(jumpHeight * 2f * gravity);
            }
            else
            {
                verticalVelocity -= gravity * Time.deltaTime;
            }

            move.y = verticalVelocity;
        }
        else
        {
            // Bay theo hướng camera
            move = (playerCamera.transform.forward * vertical +
                    playerCamera.transform.right * horizontal) * speed;

            // Tùy chọn lên xuống bằng Q/E
            if (Input.GetKey(KeyCode.E))
                move += Vector3.up * speed;

            if (Input.GetKey(KeyCode.Q))
                move += Vector3.down * speed;
        }

        characterController.Move(move * Time.deltaTime);

        #endregion

        #region Handles Rotation

        if (canMove)
        {
            rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
            rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
            playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
            transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed, 0);
        }

        #endregion
    }
}