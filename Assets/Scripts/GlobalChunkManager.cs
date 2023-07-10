using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public enum ChunkType
{
    Sea,
    Green,
    Desert,
    Snow,
}

[System.Serializable]
public struct BlockAndChunkType
{
    public GameObject block;
    public ChunkType chunkType;
}

public struct ChunkObject
{
    public GameObject gameObject;
    public Chunk chunkComponent;
}

public class GlobalChunkManager : MonoBehaviour
{

    [SerializeField] private int chunkSize = 32;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private List<BlockAndChunkType> blockToChunkTypes;
    [SerializeField] private GameObject chunkPrefab;


    private RegionGeneratorWFC regionGenerator;
    private Dictionary<Vector2Int, ChunkObject> chunks;

    private Vector2Int currentPlayerChunk;
    // private ChunkManager chunkManager;

    void Awake()
    {
        chunks = new Dictionary<Vector2Int, ChunkObject>();
    }

    // Start is called before the first frame update
    void Start()
    {
        regionGenerator = GetComponent<RegionGeneratorWFC>();
        // chunkManager = GetComponent<ChunkManager>();
    }

    // Update is called once per frame
    void Update()
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

    void UpdateChunks()
    {
        // Loop through the chunks surrounding the player and load/unload as needed
        for (int x = currentPlayerChunk.x - 2; x <= currentPlayerChunk.x + 2; x++)
        {
            for (int y = currentPlayerChunk.y - 2; y <= currentPlayerChunk.y + 2; y++)
            {
                Vector2Int chunkPos = new Vector2Int(x, y);

                // Check if the chunk is already loaded
                if (!chunks.ContainsKey(chunkPos))
                {
                    // Instantiate a new chunk prefab
                    GameObject chunk = Instantiate(chunkPrefab, new Vector3(chunkPos.x * chunkSize, 0, chunkPos.y * chunkSize), Quaternion.identity);
                    chunk.transform.parent = this.transform;
                    Chunk chunkComponent = chunk.GetComponent<Chunk>();

                    // Set the chunk type
                    ChunkType chunkType = DecideChunkType(x, y);

                    // Create the chunk, for real
                    chunkComponent.SetChunk(chunkPos.x * chunkSize, chunkPos.y * chunkSize, chunkSize, chunkType);
                    chunkComponent.InstantiateChunk(blockToChunkTypes.Find(b => b.chunkType == chunkType).block);
                    chunks.Add(chunkPos, new ChunkObject { gameObject = chunk, chunkComponent = chunkComponent });
                }
            }
        }

        // Unload chunks that are too far from the player
        List<Vector2Int> chunksToRemove = new List<Vector2Int>();
        foreach (var chunk in chunks)
        {
            if (Vector2Int.Distance(chunk.Key, currentPlayerChunk) > 3.5f)
            {
                chunksToRemove.Add(chunk.Key);
            }
        }
        foreach (var chunk in chunksToRemove)
        {
            chunks[chunk].chunkComponent.DestroyChunk();
            chunks.Remove(chunk);
        }
    }

    public Chunk GetChunkAt(Vector2Int position)
    {
        return chunks[position].chunkComponent;
    }

    private ChunkType DecideChunkType(int x, int z)
    {
        List<Neighbor> neighbors = new List<Neighbor>();

        // Check if the chunk to the north is loaded
        if (chunks.ContainsKey(new Vector2Int(x, z + 1)))
        {
            neighbors.Add(new Neighbor { chunkType = chunks[new Vector2Int(x, z + 1)].chunkComponent.GetChunkType(), direction = Direction.North });
        }

        // Check if the chunk to the east is loaded
        if (chunks.ContainsKey(new Vector2Int(x + 1, z)))
        {
            neighbors.Add(new Neighbor { chunkType = chunks[new Vector2Int(x + 1, z)].chunkComponent.GetChunkType(), direction = Direction.East });
        }

        // Check if the chunk to the south is loaded
        if (chunks.ContainsKey(new Vector2Int(x, z - 1)))
        {
            neighbors.Add(new Neighbor { chunkType = chunks[new Vector2Int(x, z - 1)].chunkComponent.GetChunkType(), direction = Direction.South });
        }

        // Check if the chunk to the west is loaded
        if (chunks.ContainsKey(new Vector2Int(x - 1, z)))
        {
            neighbors.Add(new Neighbor { chunkType = chunks[new Vector2Int(x - 1, z)].chunkComponent.GetChunkType(), direction = Direction.West });
        }

        return regionGenerator.DecideRegion(neighbors);
    }
}
