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
    public int probability;
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

    private Dictionary<ChunkType, HashSet<ChunkType>> chunkTypePairs;

    private float totalProbability;

    void Start()
    {
        chunkTypePairs = new Dictionary<ChunkType, HashSet<ChunkType>>();
        foreach (var pair in validNeighbors)
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
        HashSet<ChunkType> validRegionSet = new HashSet<ChunkType>();
        foreach (var neighbour in neighbours)
        {
            if (chunkTypePairs.ContainsKey(neighbour.chunkType))
            {
                foreach (var type in chunkTypePairs[neighbour.chunkType])
                {
                    validRegionSet.Add(type);
                }
            }
        }
        List<ChunkType> validRegions = new List<ChunkType>(validRegionSet);
        List<int> probabilities = new List<int>();
        // for (int i = 0; i < validRegions.Count; i++)
        // {
        //     Debug.Log(validRegions[i].ToString());
        // }
        // Debug.Log("-----------------");
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
        Debug.Log("Defaulting to sea");
        return ChunkType.Sea;
    }
}
