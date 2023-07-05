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
    public float tiltDifference = 10f;
    public float tiltSpeed = 15f;
    public float xOffset = 0f;


    void Start()
    {
        transform.position = new Vector3(playerPosition.position.x, cameraHeight, playerPosition.position.z - cameraDistance);
        transform.rotation = Quaternion.Euler(cameraAngle, 0, 0);
    }

    void Update()
    {
        Vector3 targetPosition = new Vector3(playerPosition.position.x + xOffset, cameraHeight, playerPosition.position.z - cameraDistance);
        transform.position = Vector3.Lerp(transform.position, targetPosition, moveSpeed * Time.deltaTime);

        // Read input for movement/rotation
        float lookUpDown = Input.GetAxisRaw("Camera Zoom");
        float lookLeftRight = Input.GetAxisRaw("Camera Pan");

        tiltCamera(lookUpDown, lookLeftRight);
    }

    void tiltCamera(float lookUpDown, float lookLeftRight) {
        Quaternion targetRotation = Quaternion.Euler(cameraAngle - (tiltDifference * lookUpDown), lookLeftRight * tiltDifference - xOffset, 0);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, tiltSpeed * Time.deltaTime);
    }
}
