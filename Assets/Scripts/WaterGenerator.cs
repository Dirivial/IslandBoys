using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterGenerator : MonoBehaviour
{
    public GameObject chunkPrefab;
    public int chunkSize = 20;
    public Transform playerTransform;
    public int chunkCountX = 7;
    public int chunkCountY = 10;

    private Dictionary<Vector2Int, GameObject> chunks;
    private Vector2Int currentPlayerChunk;


    private void Start()
    {
        chunks = new Dictionary<Vector2Int, GameObject>();
    }

    private void Update()
    {
        // Calculate the player's current chunk position
        Vector2Int playerChunk = new Vector2Int(
            Mathf.FloorToInt(playerTransform.position.x / chunkSize),
            Mathf.FloorToInt(playerTransform.position.z / chunkSize)
        );

        // If the player has moved to a new chunk, update the chunks
        if (playerChunk != currentPlayerChunk)
        {
            currentPlayerChunk = playerChunk;
            UpdateChunks();
        }
    }

    private void UpdateChunks()
    {
        // Loop through the chunks surrounding the player and load/unload as needed
        for (int x = currentPlayerChunk.x - chunkCountX; x <= currentPlayerChunk.x + chunkCountX; x++)
        {
            for (int y = currentPlayerChunk.y - 1; y <= currentPlayerChunk.y + chunkCountY; y++)
            {
                Vector2Int chunkPos = new Vector2Int(x, y);

                // Check if the chunk is already loaded
                if (!chunks.ContainsKey(chunkPos))
                {
                    // Instantiate a new chunk prefab
                    GameObject chunk = Instantiate(chunkPrefab, new Vector3(x * chunkSize, 0f, y * chunkSize), Quaternion.identity);
                    chunk.transform.parent = transform;
                    chunks.Add(chunkPos, chunk);
                }
            }
        }

        // Unload chunks that are too far from the player
        List<Vector2Int> chunksToRemove = new List<Vector2Int>();
        foreach (var chunk in chunks)
        {
            if (Mathf.Abs(chunk.Key.x - currentPlayerChunk.x) > chunkCountX || chunk.Key.y - currentPlayerChunk.y > chunkCountY || chunk.Key.y - currentPlayerChunk.y < -1)
            {
                chunksToRemove.Add(chunk.Key);
            }
        }

        foreach (var chunkPos in chunksToRemove)
        {
            Destroy(chunks[chunkPos]);
            chunks.Remove(chunkPos);
        }
    }
}
