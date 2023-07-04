using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{ 
    public Transform playerPosition;
    public float moveSpeed = 5f;
    public float cameraHeight = 15f;
    public float cameraDistance = 5f;
    public float cameraAngle = 55f;
    public float gridSize = 2f;
    public float tiltDifference = 7f;
    public float tiltSpeed = 15f;

    private bool lookUp = false;
    private bool lookDown = false;

    void Start()
    {
        transform.position = new Vector3(playerPosition.position.x, cameraHeight, playerPosition.position.z - cameraDistance);
        transform.rotation = Quaternion.Euler(cameraAngle, 0, 0);
    }

    void Update()
    {
        Vector3 targetPosition = new Vector3(playerPosition.position.x, cameraHeight, playerPosition.position.z - cameraDistance);
        transform.position = Vector3.Lerp(transform.position, targetPosition, moveSpeed * Time.deltaTime);

        // Read input for movement/rotation
        float zoomInput = Input.GetAxisRaw("Camera Zoom");

        if (zoomInput != 0) {
            lookUp = zoomInput < 0;
            lookDown = zoomInput > 0;
        } else {
            lookUp = false;
            lookDown = false;
        }

        tiltCamera();
    }

    void tiltCamera() {
        Quaternion targetRotation = Quaternion.Euler(cameraAngle, 0, 0);
        if (lookUp) {
            targetRotation = Quaternion.Euler(cameraAngle + tiltDifference, 0, 0);
        } else if (lookDown) {
            targetRotation = Quaternion.Euler(cameraAngle - tiltDifference, 0, 0);
        }
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, tiltSpeed * Time.deltaTime);
    }

}
