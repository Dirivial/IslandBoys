using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{

    public Vector3 initialPosition = new Vector3(50, 30, 50);
    public Transform cameraTransform;

    public float moveSpeed = 5f;
    public float rotateSpeed = 8f;

    public int maxJumps = 2;
    public float jumpForce = 5f;
    public float airDragMultiplier = 0.8f;

    private int jumpCount = 0;

    private Rigidbody rb;

    private void Start()
    {
        transform.position = initialPosition;
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");

        Vector3 cameraForward = cameraTransform.forward;
        cameraForward.y = 0f;
        cameraForward.Normalize();

        Vector3 cameraRight = cameraTransform.right;
        cameraRight.y = 0f;
        cameraRight.Normalize();

        Vector3 movementDirection = (cameraForward * verticalInput + cameraRight * horizontalInput).normalized;

        // Rotate the character towards the movement direction
        if (movementDirection != Vector3.zero)
        {
            Quaternion toRotation = Quaternion.LookRotation(movementDirection);
            transform.rotation = Quaternion.Lerp(transform.rotation, toRotation, rotateSpeed * Time.fixedDeltaTime);
        }

        Vector3 movement = movementDirection * moveSpeed * Time.fixedDeltaTime;
        transform.position += movement;

        // Jumping
        if (IsGrounded())
        {
            jumpCount = 0;
        }

        if (Input.GetButtonDown("Jump") && (jumpCount < maxJumps - 1))
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            jumpCount++;
        }
    }

    private bool IsGrounded()
    {
        // Check if the player is grounded
        // You can use a sphere or capsule cast here if you want to
        // I'm using a raycast for simplicity
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 1.1f))
        {
            return true;
        }
        return false;
    }

}
