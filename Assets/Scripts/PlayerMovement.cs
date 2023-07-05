using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    public float gridSize = 1f;
    public float jumpHeight = 1f;
    public float jumpDuration = 1f;
    public AnimationCurve jumpCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    public GameObject Island;

    private float jumpTimer = 0f;
    private Vector3 targetPosition;
    private bool isMoving = false;
    
    private void Update()
    {
        if (isMoving)
        {
            
            jumpTimer += Time.deltaTime;

            float normalizedTime = jumpTimer / jumpDuration;
            float jumpProgress = jumpCurve.Evaluate(normalizedTime);

            // Move towards the target position
            Vector3 newPosition = Vector3.Lerp(transform.position, targetPosition, jumpProgress);
            newPosition.y = jumpProgress * jumpHeight + 2f;

            transform.position = newPosition;

            // Check if we have reached the target position
            if (normalizedTime >= 1f)
            {
                isMoving = false;
                jumpTimer = 0f;
                transform.position = new Vector3(transform.position.x, 2f, transform.position.z);
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
        Vector3 gridPosition = GridManager.Instance.GetGridPosition(position);
        if (GridManager.Instance.GetGridCell((int) gridPosition.x, (int) gridPosition.z).isWater)
        {
            return false;
        }
        return true;
    }
}
