using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{ 
   
    public Transform playerTransform;
    public float rotationAngleY = -45f;
    public float rotationAngleX = 40f;
    public float moveSpeed = 5f;

    public Vector3 cameraOffset = new Vector3(-15f, 30f, -15f);

    private void Start()
    {
        // Calculate the initial camera offset from the player
        transform.position = playerTransform.position + cameraOffset;
        transform.rotation = Quaternion.Euler(rotationAngleX, rotationAngleY, 0);
    }

    void Update()
    {
        Vector3 targetPosition = new Vector3(playerTransform.position.x + cameraOffset.x, cameraOffset.y + playerTransform.position.y, playerTransform.position.z - cameraOffset.z);
        transform.position = Vector3.Lerp(transform.position, targetPosition, moveSpeed * Time.deltaTime);
    }
}
