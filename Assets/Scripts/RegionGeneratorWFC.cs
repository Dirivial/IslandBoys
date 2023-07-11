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
                } else if (x == CHUNK_COUNT_XZ / 2 && z == CHUNK_COUNT_XZ / 2)
                {
                    chunkMap[x, z] = ChunkType.Plains;
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

        ProcessChunks();
        
        //PrintSelectedTiles();

        // Look if the steroids variable is correct
        /*        foreach (Adjacency adjacency in steroids)
                {
                    Debug.Log(adjacency.region);
                    foreach (ChunkType chunkType in adjacency.adjacentSouth)
                    {
                        Debug.Log(chunkType);
                    }
                    Debug.Log(" ");
                }*/
        GenerateMap();
        PrintSelectedTiles();

        for (int x = 0; x < CHUNK_COUNT_XZ; x++)
        {
            for (int z = 0; z < CHUNK_COUNT_XZ; z++)
            {
                if (chunkMap[x, z] == ChunkType.Unknown)
                {
                    chunkMap[x, z] = ChunkType.Sea;
                }
            }
        }

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
        PrintEntropy();
        Debug.Log("Done");
    }

    private void PickRandomTileAt(Vector2Int chunkPosition)
    {
        List<ChunkType> chunkTypes = new List<ChunkType>(chunkMapArray[chunkPosition.x, chunkPosition.y]);
        int randomIndex = Random.Range(0, chunkTypes.Count);
        
        chunkMap[chunkPosition.x, chunkPosition.y] = chunkTypes[randomIndex];
        Debug.Log("Picked " + chunkMap[chunkPosition.x, chunkPosition.y] + " at " + chunkPosition);
        chunkMapArray[chunkPosition.x, chunkPosition.y].Clear();
        UpdateNeighbors(chunkPosition);

        chunksToProcess.Push(chunkPosition);
    }

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

    private void UpdateNeighbors(Vector2Int chunkPosition)
    {
        //Debug.Log("Updating neighbors of " + chunkPosition.x + " " + chunkPosition.y);
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
                    //chunksToProcess.Push(neighborPosition);
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

/*                        if (neighborPosition.x == 1 && neighborPosition.y == 1)
                        {
                            string d = "";
                            Debug.Log("Allowed adjacent chunks ");
                            foreach (ChunkType chunkType in chunkMapArray[neighborPosition.x, neighborPosition.y])
                            {
                                d += " " + chunkType.ToString();
                            }
                            Debug.Log(d);
                        }*/

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

    private int GetEntropy(Vector2Int chunkPosition)
    {
        return chunkMapArray[chunkPosition.x, chunkPosition.y].Count;
    }

/*    private void AddPair(RegionAdjacency pair)
    {
        if (!chunkTypePairs.ContainsKey(pair.regionA))
        {
            chunkTypePairs.Add(pair.regionA, new HashSet<ChunkType>());
        }

        if (!chunkTypePairs.ContainsKey(pair.regionB))
        {
            chunkTypePairs.Add(pair.regionB, new HashSet<ChunkType>());
        }
        // Add region to each other's pairs

        chunkTypePairs[pair.regionA].Add(pair.regionB);
        chunkTypePairs[pair.regionB].Add(pair.regionA);
    }*/

/*    private void GenerateChunks()
    {
          for (int x = 0; x < CHUNK_COUNT_XZ; x++)
        {
            for (int z = 0; z < CHUNK_COUNT_XZ; z++)
            {
                Vector2Int chunkPosition = new Vector2Int(x, z);
                ChunkType chunkType = DecideRegion(chunkPosition);
                chunkMap.Add(chunkPosition, chunkType);
            }
        }
    }*/


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
/*        HashSet<ChunkType> validRegionSet = new HashSet<ChunkType>();

        if (neighbours.Count == 0) 
        { 
            Debug.Log("0 Neighbours"); 
            return ChunkType.Sea;
        } 
        else 
        {
            // Add all valid regions from first neighbour
            foreach (ChunkType type in chunkTypePairs[neighbours[0].chunkType])
            {
                validRegionSet.Add(type);
            }

            if (neighbours.Count > 1)
            {
                // Remove regions that are not valid for all neighbours
                for (int i = 1; i < neighbours.Count; i++)
                {
                    HashSet<ChunkType> neighbour = new HashSet<ChunkType>(chunkTypePairs[neighbours[1].chunkType]);
                    validRegionSet.IntersectWith(neighbour);
                }
            }
        }

        List<ChunkType> validRegions = new List<ChunkType>(validRegionSet);
        List<int> probabilities = new List<int>();

        int probabilitySum = 0;
        foreach (var region in regions)
        {
            if (validRegions.Contains(region.chunkType))
            {
                probabilitySum += region.probability;
                probabilities.Add(region.probability);
            }
        }

        float remaining = Mathf.FloorToInt(Random.Range(0f, 1f) * probabilitySum);

        for (int i = 0; i < validRegions.Count; i++)
        {
            remaining -= probabilities[i];
            if (remaining <= 0)
            {
                return validRegions[i];
            }
        }

        Debug.Log("Defaulting to sea " + remaining);*/
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
