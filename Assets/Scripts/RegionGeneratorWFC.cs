using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public enum Direction
{
    North,
    East,
    South,
    West,
}


[System.Serializable]
public struct Region
{
    public ChunkType chunkType;
    public int probability;
}

[System.Serializable]
public struct RegionAdjacency
{
    public ChunkType regionA;
    public ChunkType regionB;
}

[System.Serializable]
public struct Adjacency
{
    public ChunkType region;
    public List<ChunkType> adjacentWest;
    public List<ChunkType> adjacentEast;
    public List<ChunkType> adjacentNorth;
    public List<ChunkType> adjacentSouth;
}

public struct Neighbor
{
    public ChunkType chunkType;
    public Direction direction;
}

public class RegionGeneratorWFC : MonoBehaviour
{

    [SerializeField] private List<Region> regions;
    [SerializeField] private List<Adjacency> adjacancies;
    [SerializeField] private List<Adjacency> backup;
    [SerializeField] private int CHUNK_COUNT_XZ = 20;

    private ChunkType[,] chunkMap;
    private HashSet<ChunkType>[,] chunkMapArray;
    private Stack<Vector2Int> chunksToProcess;

    private GlobalChunkManager globalChunkManager;

    private void Awake()
    {
        globalChunkManager = GetComponent<GlobalChunkManager>();
    }

    void Start()
    {
        Debug.Log("Start");
        chunksToProcess = new Stack<Vector2Int>();
        chunkMap = new ChunkType[CHUNK_COUNT_XZ, CHUNK_COUNT_XZ];

        // Store the initial entropy of each coordinate in the chunk map
        chunkMapArray = new HashSet<ChunkType>[CHUNK_COUNT_XZ, CHUNK_COUNT_XZ];

        // Set the edges to be sea
        for (int x = 0; x < CHUNK_COUNT_XZ; x++)
        {
            for (int z = 0; z < CHUNK_COUNT_XZ; z++)
            {
                chunkMapArray[x, z] = new HashSet<ChunkType>();
                if (x == 0 || x == CHUNK_COUNT_XZ - 1 || z == 0 || z == CHUNK_COUNT_XZ - 1)
                {
                    chunkMap[x, z] = ChunkType.Sea;
                    chunksToProcess.Push(new Vector2Int(x, z));
                }
                else
                {   
                    for (int i = 0; i < regions.Count; i++)
                    {
                        chunkMapArray[x, z].Add(regions[i].chunkType);
                    }
                }
            }
        }
        
        // Process the chunks that were set to sea
        ProcessChunks();

        // Fill the rest of the map
        GenerateMap();

        // Draw the chunks
        globalChunkManager.DrawSomeChunks(chunkMap, CHUNK_COUNT_XZ);
    }

    private void GenerateMap()
    {
        int maxIterations = CHUNK_COUNT_XZ * CHUNK_COUNT_XZ;
        int iterations = 0;
        Vector2Int nextWave = FindLowestEntropy();

        while (nextWave.x != -1 && nextWave.y != -1 && iterations < maxIterations)
        {
            PickRandomTileAt(nextWave);
            ProcessChunks();
            nextWave = FindLowestEntropy();
            iterations++;
        }
        Debug.Log("Done");
    }

    // Pick a random tile at the given chunk position, using the possible tiles at that position
    private void PickRandomTileAt(Vector2Int chunkPosition)
    {
        List<ChunkType> chunkTypes = new List<ChunkType>(chunkMapArray[chunkPosition.x, chunkPosition.y]);
        int randomIndex = Random.Range(0, chunkTypes.Count);
        
        chunkMap[chunkPosition.x, chunkPosition.y] = chunkTypes[randomIndex];
        chunkMapArray[chunkPosition.x, chunkPosition.y].Clear();
        UpdateNeighbors(chunkPosition);

        chunksToProcess.Push(chunkPosition);
    }

    // Find the first chunk with the lowest entropy
    private Vector2Int FindLowestEntropy()
    {
        // Look for the lowest entropy in the chunk map
        int lowestEntropy = regions.Count;
        Vector2Int lowestEntropyPosition = new Vector2Int(-1, -1);
        for (int x = 1; x < CHUNK_COUNT_XZ - 1; x++)
        {
            for (int z = 1; z < CHUNK_COUNT_XZ - 1; z++)
            {
                if (chunkMapArray[x, z].Count < lowestEntropy && chunkMapArray[x, z].Count > 0)
                {
                    lowestEntropy = chunkMapArray[x, z].Count;
                    lowestEntropyPosition = new Vector2Int(x, z);
                }
            }
        }
        return lowestEntropyPosition;
    }

    private bool CheckDone()
    {
        // Check if all chunks have a selected chunk type
        for (int x = 0; x < CHUNK_COUNT_XZ; x++)
        {
            for (int z = 0; z < CHUNK_COUNT_XZ; z++)
            {
                if (chunkMap[x, z] == ChunkType.Unknown)
                {
                    return false;
                }
            }
        }
        return true;
    }

    // Process the chunks that were have been but in the chunksToProcess stack
    private void ProcessChunks()
    {
        int maxIterations = 1000;
        int i = 0;
        while (chunksToProcess.Count > 0 && maxIterations > i)
        {
            Vector2Int chunkPosition = chunksToProcess.Pop();
            /*Debug.Log(chunkPosition);*/
            if (GetEntropy(chunkPosition) == 1)
            {
                // We have a single chunk type, so we can set it
                foreach (int chunkType in chunkMapArray[chunkPosition.x, chunkPosition.y])
                {
                    chunkMap[chunkPosition.x, chunkPosition.y] = (ChunkType)chunkType;
                }
                chunkMapArray[chunkPosition.x, chunkPosition.y].Clear();
            }
            UpdateNeighbors(chunkPosition);
            i++;
        }
    }

    private void PrintEntropy()
    {
        string output = "";
        for (int x = 0; x < CHUNK_COUNT_XZ; x++)
        {
            for (int z = 0; z < CHUNK_COUNT_XZ; z++)
            {
                output += " " + chunkMapArray[x, z].Count.ToString();
            }
            Debug.Log(output);
            output = "";
        }
    }

    private void PrintSelectedTiles()
    {
        string output = "";
        for (int x = 0; x < CHUNK_COUNT_XZ; x++)
        {
            for (int z = 0; z < CHUNK_COUNT_XZ; z++)
            {
                if (chunkMapArray[x, z].Count == 0)
                {
                    output += " " + chunkMap[x, z].ToString();
                } else if (chunkMapArray[x, z].Count == 1)
                {
                    foreach (ChunkType chunkType in chunkMapArray[x, z])
                    {
                        output += " " + chunkType.ToString();
                    }
                } else
                {
                    output += " " + chunkMapArray[x, z].Count.ToString();
                }
            }
            Debug.Log(output);
            output = "";
        }
    }

    // Update the neighbors of the given chunk position
    private void UpdateNeighbors(Vector2Int chunkPosition)
    {
        for (Direction i = 0; i <= Direction.West; i++)
        {
            Vector2Int neighborPosition = chunkPosition;
            switch (i)
            {
                case Direction.North:
                    neighborPosition.y += 1;
                    break;
                case Direction.South:
                    neighborPosition.y -= 1;
                    break;
                case Direction.East:
                    neighborPosition.x += 1;
                    break;
                case Direction.West:
                    neighborPosition.x -= 1;
                    break;
            }

            if (neighborPosition.x >= 0 && neighborPosition.x < CHUNK_COUNT_XZ - 1 && neighborPosition.y >= 0 && neighborPosition.y < CHUNK_COUNT_XZ - 1)
            {
                if (chunkMap[neighborPosition.x, neighborPosition.y] == ChunkType.Unknown && chunkMapArray[neighborPosition.x, neighborPosition.y].Count > 1)
                {
                    // Get the allowed adjacent chunks
                    HashSet<ChunkType> allowedAdjacentChunks = null;
                    foreach (Adjacency adjacency in adjacancies)
                    {
                        if (adjacency.region == chunkMap[chunkPosition.x, chunkPosition.y])
                        {
                            switch (i)
                            {
                                case Direction.North:
                                    allowedAdjacentChunks = new HashSet<ChunkType>(adjacency.adjacentNorth);
                                    break;
                                case Direction.South:
                                    allowedAdjacentChunks = new HashSet<ChunkType>(adjacency.adjacentSouth);
                                    break;
                                case Direction.East:
                                    allowedAdjacentChunks = new HashSet<ChunkType>(adjacency.adjacentEast);
                                    break;
                                case Direction.West:
                                    allowedAdjacentChunks = new HashSet<ChunkType>(adjacency.adjacentWest);
                                    break;
                            }
                        }
                    }
                    if (allowedAdjacentChunks != null)
                    {
                        int entropy = GetEntropy(neighborPosition);

                        // Update the entropy of the neighbor
                        chunkMapArray[neighborPosition.x, neighborPosition.y].IntersectWith(allowedAdjacentChunks);

                        int newEntropy = GetEntropy(neighborPosition);
                        if (entropy != newEntropy)
                        {
                            //Debug.Log("Entropy decreased for " + neighborPosition.x + ", " + neighborPosition.y);
                            chunksToProcess.Push(neighborPosition);
                        }
                    }
                }
            }
        }
    }

    // Get the entropy of a chunk
    private int GetEntropy(Vector2Int chunkPosition)
    {
        return chunkMapArray[chunkPosition.x, chunkPosition.y].Count;
    }

    public ChunkType GetChunkType(Vector2Int chunkPosition)
    {
        if (chunkPosition.x < CHUNK_COUNT_XZ && chunkPosition.y < CHUNK_COUNT_XZ)
        {
            return chunkMap[chunkPosition.x, chunkPosition.y];
        }
        else
        {
            return ChunkType.Sea;
        }
    }

    public ChunkType DecideRegion(List<Neighbor> neighbours)
    {
        return ChunkType.Sea;
    }

    private void OnDrawGizmos()
    {
        if (chunkMap != null)
        {
            for (int x = 0; x < CHUNK_COUNT_XZ; x++)
            {
                for (int z = 0; z < CHUNK_COUNT_XZ; z++)
                {
                    Vector3 chunkPosition = new Vector3(x, 0, z);
                    switch (chunkMap[x, z])
                    {
                        case ChunkType.Sea:
                            Gizmos.color = Color.blue;
                            break;
                        case ChunkType.Plains:
                            Gizmos.color = Color.green;
                            break;
                        case ChunkType.Desert:
                            Gizmos.color = Color.yellow;
                            break;
                        case ChunkType.ShoreS:
                            Gizmos.color = Color.black;
                            break;
                        case ChunkType.ShoreE:
                            Gizmos.color = Color.cyan;
                            break;
                        case ChunkType.ShoreW:
                            Gizmos.color = Color.cyan;
                            break;
                        case ChunkType.ShoreN:
                            Gizmos.color = Color.white;
                            break;
                        case ChunkType.ShoreNW:
                            Gizmos.color = Color.white;
                            break;
                        case ChunkType.ShoreNE:
                            Gizmos.color = Color.white;
                            break;
                        case ChunkType.ShoreSW:
                            Gizmos.color = Color.black;
                            break;
                        case ChunkType.ShoreSE:
                            Gizmos.color = Color.black;
                            break;
                        case ChunkType.Unknown:
                            Gizmos.color = Color.clear;
                            break;
                    }
                    Gizmos.DrawCube(chunkPosition, new Vector3(1, 1, 1));
                }
            }
        }
    }
}
