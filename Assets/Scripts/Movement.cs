using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float gridSize = 1f;
    
    private Vector3 targetPosition;
    private bool isMoving = false;
    
    private void Update()
    {
        if (isMoving)
        {
            // Move towards the target position
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            
            // Check if we have reached the target position
            if (transform.position == targetPosition)
            {
                isMoving = false;
            }
        }
        else
        {
            // Read input for movement
            float horizontalInput = Input.GetAxisRaw("Horizontal");
            float verticalInput = Input.GetAxisRaw("Vertical");
            
            if (horizontalInput != 0 || verticalInput != 0)
            {
                // Calculate the new target position
                float x = Mathf.Round(transform.position.x / gridSize) * gridSize;
                float z = Mathf.Round(transform.position.z / gridSize) * gridSize;
                targetPosition = new Vector3(x + gridSize * horizontalInput, transform.position.y, z + gridSize * verticalInput);
                
                // Check if the target position is valid (within bounds of the grid)
                if (IsPositionValid(targetPosition))
                {
                    isMoving = true;
                }
            }
        }
    }
    
    private bool IsPositionValid(Vector3 position)
    {
        // Implement your own logic here to check if the position is valid
        // For example, you could check if the position is within the bounds of the grid
        
        // Assuming the grid has a size of 10 units in both x and y directions
        // if (position.x < -5f || position.x > 5f || position.y < -5f || position.y > 5f)
        // {
        //     return false;
        // }
        
        return true;
    }
}
