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
public struct RegionAdjacency
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
    [SerializeField] private List<RegionAdjacency> adjacancies;

    private Dictionary<ChunkType, HashSet<ChunkType>> chunkTypePairs;

    private float totalProbability;

    void Start()
    {
        chunkTypePairs = new Dictionary<ChunkType, HashSet<ChunkType>>();
        foreach (var pair in adjacancies)
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

        Debug.Log("Defaulting to sea " + remaining);
        return ChunkType.Sea;
    }
}
