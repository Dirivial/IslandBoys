using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{

    public Vector3 initialPosition = new Vector3(50, 30, 50);
    public Transform cameraTransform;

    [Header("Movement")]
    public float moveSpeed = 5f;
    public float rotateSpeed = 8f;
    public float groundDrag = 2f;

    [Header("Ground Check")]
    public float playerHeight = 2f;
    public LayerMask groundMask;

    [Header("Jumping")]
    public float jumpForce = 12f;
    public float jumpCooldown = 0.25f;
    public float airMultiplier = 0.4f;

    private bool readyToJump = true;
    private Rigidbody rb;
    private bool grounded = true;

    // private int jumpCount = 0;
    // private float movementMultiplier = 1f;
    // private bool isJumping = false;

    private float horizontalInput = 0f;
    private float verticalInput = 0f;

    private Vector3 cameraForward = Vector3.zero;
    private Vector3 cameraRight = Vector3.zero;

    private void Start()
    {
        transform.position = initialPosition;
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        if (Input.GetKeyDown("space") && grounded && readyToJump) {
            
            readyToJump = false;

            Jump();

            Invoke("ResetJump", jumpCooldown);
        }

        cameraForward = cameraTransform.forward;
        cameraForward.y = 0f;
        cameraForward.Normalize();

        cameraRight = cameraTransform.right;
        cameraRight.y = 0f;
        cameraRight.Normalize();

        // Check if the player is grounded
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight / 2f + 0.2f, groundMask);

        // If the player is grounded set drag
        if (grounded) {
            rb.drag = groundDrag;
        }
        else {
            rb.drag = 0f;
        }

        SpeedControl();
    }

    private void FixedUpdate() {

        Vector3 movementDirection = (cameraForward * verticalInput + cameraRight * horizontalInput).normalized;

        // Rotate the character towards the movement direction
        if (movementDirection != Vector3.zero)
        {
            Quaternion toRotation = Quaternion.LookRotation(movementDirection);
            transform.rotation = Quaternion.Lerp(transform.rotation, toRotation, rotateSpeed * Time.fixedDeltaTime);
        }

        Vector3 movement = movementDirection * moveSpeed * 10f;

        // If the player is grounded apply the movement directly
        if (grounded) {
            rb.AddForce(movement, ForceMode.Force);
        } else {
            // If the player is in the air apply the movement with airMultiplier
            rb.AddForce(movement * airMultiplier, ForceMode.Force);
        }
        
    }

    private void SpeedControl() {
        Vector3 flatVelocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        if (flatVelocity.magnitude > moveSpeed) {
            Vector3 limitedVelocity = flatVelocity.normalized * moveSpeed;
            rb.velocity = new Vector3(limitedVelocity.x, rb.velocity.y, limitedVelocity.z);
        }
    }

    private void Jump() {
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private void ResetJump() {
        readyToJump = true;
    }
}
