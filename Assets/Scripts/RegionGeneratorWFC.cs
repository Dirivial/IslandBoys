using System.Collections;
using System.Collections.Generic;
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
    public float probability;
}

[System.Serializable]
public struct RegionPair
{
    public ChunkType regionA;
    public ChunkType regionB;
}

public struct Neighbor
{
    public ChunkType chunkType;
    public Direction direction;
}

public class RegionGeneratorWFC : MonoBehaviour
{

    [SerializeField] private List<Region> regions;
    [SerializeField] private List<RegionPair> validNeighbors;

    private Dictionary<ChunkType, ChunkType> chunkTypePairs;

    private float totalProbability;

    void Start()
    {
        chunkTypePairs = new Dictionary<ChunkType, ChunkType>();
        foreach (var pair in validNeighbors)
        {
            chunkTypePairs.Add(pair.regionA, pair.regionB);
            chunkTypePairs.Add(pair.regionB, pair.regionA);
        }

        // Compute probabilities
        totalProbability = 0f;
        foreach (var region in regions)
        {
            totalProbability += region.probability;
        }
    }

    public ChunkType DecideRegion(List<Neighbor> neighbours)
    {
        ChunkType chunkType = Mathf.FloorToInt(Random.Range(0f, 10f)) < 5 ? regions[0].chunkType : regions[1].chunkType;
        Debug.Log("Decided region: " + chunkType);
        return chunkType;
    }
}
