using System.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;
using Debug = UnityEngine.Debug;

public enum Direction
{
    North,
    South,
    East,
    West,
    Up,
    Down,
}

public enum Symmetry
{
    X,
    L,
    I,
    T,
    D, // This should be '\\', I chose D for diagonal
}

public class WFC : MonoBehaviour
{

    private List<VoxelType> voxelTypes;
    [SerializeField] public Vector3Int dimensions = new Vector3Int(5, 5, 5);

    private int voxelCount = 1;
    private bool setupComplete = false;
    private int[,,] voxelMap;
    private bool[,,][] voxelMapArray;
    private Stack<Vector3Int> voxelsToProcess;
    private Stack<SaveState> saveStates;

    private List<GameObject> boi;
    private VoxelGang voxelGang;
    private int voxelSize = 3;

    private void Awake()
    {
        voxelsToProcess = new Stack<Vector3Int>();
        voxelMap = new int[dimensions.x, dimensions.y, dimensions.z];

        // Store the initial entropy of each coordinate in the chunk map
        voxelMapArray = new bool[dimensions.x, dimensions.y, dimensions.z][];
        boi = new List<GameObject>();
        voxelGang = GetComponentInChildren<VoxelGang>();
    }

    void Start()
    {
        voxelCount = voxelGang.GetVoxelTypesCount();
        voxelSize = voxelGang.GetVoxelSize();
        voxelTypes = voxelGang.GetVoxelTypes();

        // For testing
        //InstantiateVoxelTypes();

        //PrintConnectionDirs();
    }

    private void PrintArrayCount()
    {
        string d;
        for (int x = 0; x < dimensions.x; x++)
        {
            for (int y  = 0; y < dimensions.y; y++)
            {
                d = "";
                for (int z = 0; z < dimensions.z; z++)
                {
                    d += " " + voxelMapArray[x, y, z].Count(c => c == true);
                }
                Debug.Log(d);
            }
        }
    }


    private void PrintConnectionDirs()
    {
        string debugString;
        for (int i = 0; i < voxelCount; i++)
        {
            VoxelType v = voxelTypes[i];
            debugString = v.name;
            for (int j = 0; j < 6; j++)
            {
                if (v.connections[j] > 0)
                { 
                    debugString += ", " + (Direction)j;
                }
            }
            Debug.Log(debugString);
        }
    }

    public void Clear()
    {
        setupComplete = false;
        voxelsToProcess.Clear();
        voxelMap = new int[dimensions.x, dimensions.y, dimensions.z];
        for (int i = boi.Count - 1; i >= 0; i--)
        {
            Destroy(boi[i]);
        }
    }

    public void Setup()
    {
        Debug.Log("Generating a new model - Clearing " + boi.Count + " items");

        for (int i = boi.Count - 1; i >= 0; i--)
        {
            Destroy(boi[i]);
        }
        boi.Clear();
        voxelsToProcess = new Stack<Vector3Int>();
        voxelMap = new int[dimensions.x, dimensions.y, dimensions.z];

        // Store the initial entropy of each coordinate in the chunk map
        voxelMapArray = new bool[dimensions.x, dimensions.y, dimensions.z][];
        for (int x = 0; x < dimensions.x; x++)
        {
            for (int z = 0; z < dimensions.z; z++)
            {
                for (int y = 0; y < dimensions.y; y++)
                {
                    voxelMapArray[x, y, z] = new bool[voxelCount];

                    // Set floor to be ground tiles
                    if (y == 0)
                    {                        
                        //voxelMap[x, y, z] = 1;
                        //voxelMapArray[x, y, z][1] = true;
                        //voxelsToProcess.Push(new Vector3Int(x, y, z));
                        voxelMap[x, y, z] = -1;
                        for (int i = 1; i < voxelTypes.Count; i++) // THIS MIGHT MAKE STUFF BREAK IN THE FUTURE
                        {
                            voxelMapArray[x, y, z][i] = true;
                        }
                    }
                    else // The rest should be empty
                    {

                        voxelMap[x, y, z] = -1;
                        for (int i = 2; i < voxelTypes.Count; i++) // THIS MIGHT MAKE STUFF BREAK IN THE FUTURE
                        {
                            voxelMapArray[x, y, z][i] = true;
                        }
                    }
                }
            }
        }
        ProcessVoxels();

        InstantiateDeezNuts();
        setupComplete = true;
    }

    public void GenerateFull()
    {
        Setup();

        int maxIterations = dimensions.x * dimensions.y * dimensions.z;
        int iterations = 0;
        Vector3Int nextWave = FindLowestEntropy();

        while (nextWave.x != -1 && nextWave.y != -1 && iterations < maxIterations)
        {
            PickTileAt(nextWave);
            ProcessVoxels();
            nextWave = FindLowestEntropy();
            iterations++;
        }
        InstantiateDeezNuts();
        PrintArrayCount();
    }

    public void TakeStep()
    {
        if (!setupComplete)
        {
            Setup();
            setupComplete = true;
        }

        Vector3Int nextWave = FindLowestEntropy();
        //Debug.Log(nextWave);
        List<Vector3Int> toInstantiate;

        if (nextWave.x != -1 && nextWave.y != -1 && nextWave.z != -1)
        {
            PickTileAt(nextWave);
            toInstantiate = ProcessVoxels();
            InstantiateDeezNut(toInstantiate);
        }
        else
        {
            Debug.Log("Could not get any further");
        }
        //Debug.Log(voxelMap[0, 1, 1].name);
    }

    private void PrintStuff()
    {
        // Print out the voxels that have been fiddled on
        for (int x = 0; x < dimensions.x; x++)
        {
            for (int z = 0; z < dimensions.z; z++)
            {
                for (int y = 0; y < dimensions.y; y++)
                {
                    int index = voxelMap[x, y, z];
                    if (index != -1)
                    {
                        Debug.Log("Decided: " + voxelTypes[index] + " " + new Vector3Int(x, y, z));
                    }
                }
            }
        }
    }

    // Just to make sure that the rotations are correct - this should be kept for later when we add more types of voxels
    private void InstantiateVoxelTypes()
    {
        float i = 0.0f;
        foreach (VoxelType voxel in voxelTypes)
        {
            GameObject obj = Instantiate(voxel.voxelObject, new Vector3(i * voxelSize, 0, 0), voxel.rotation);
            obj.transform.parent = transform;
            i += 1.1f;
        }
    }

    private void InstantiateDeezNuts()
    {
        for (int x = 0; x < dimensions.x; x++)
        {
            for (int z = 0; z < dimensions.z; z++)
            {
                for (int y = 0; y < dimensions.y; y++)
                {
                    int index = voxelMap[x, y, z];
                    if (index != -1)
                    {
                        //Debug.Log("Heey " + voxelMap[x, y, z].name + " " + x + " " + y + " " + z);
                        GameObject obj = Instantiate(voxelTypes[index].voxelObject, new Vector3(x * voxelSize, y * voxelSize, z * voxelSize), voxelTypes[index].rotation);
                        obj.transform.parent = transform;
                        boi.Add(obj);
                    } else
                    {
                        GameObject obj = Instantiate(voxelTypes[0].voxelObject, new Vector3(x * voxelSize, y * voxelSize, z * voxelSize), Quaternion.identity);
                        obj.transform.parent = transform;
                        boi.Add(obj);
                    }
                }
            }
        }
    }

    private void InstantiateDeezNut(List<Vector3Int> toInstantiate)
    {
        foreach (Vector3Int v in toInstantiate)
        {
            VoxelType voxelType = voxelTypes[voxelMap[v.x, v.y, v.z]];
            GameObject obj = Instantiate(voxelType.voxelObject, new Vector3(v.x * voxelSize, v.y * voxelSize, v.z * voxelSize), voxelType.rotation);
            obj.transform.parent = transform;
            boi.Add(obj);
            //Debug.Log(voxelType.name);
        }
    }

    // Pick a random tile at the given voxel position, using the possible tiles at that position
    private void PickTileAt(Vector3Int pos)
    {
        VoxelType v = ChooseVoxelTypeAt(pos.x, pos.y, pos.z);
        voxelMap[pos.x, pos.y, pos.z] = voxelTypes.IndexOf(v);

        for (int i = 0; i < voxelTypes.Count; i++)
        {
            voxelMapArray[pos.x, pos.y, pos.z][i] = false;
        }

        //UpdateNeighbors(voxelPosition); // I dunno walter, I guess we just update the neighbors ?? why would I do this?? 

        voxelsToProcess.Push(pos);
    }

    // Find the first voxel with the lowest entropy
    private Vector3Int FindLowestEntropy()
    {
        // Look for the lowest entropy in the voxel map
        int lowestEntropy = voxelCount + 1;
        Vector3Int lowestEntropyPosition = new Vector3Int(-1, -1, -1);
        for (int x = 0; x < dimensions.x; x++)
        {
            for (int z = 0; z < dimensions.z; z++)
            {
                for (int y = 0; y < dimensions.y; y++)
                {
                    if (voxelMap[x, y, z] != -1) continue;
                    int possibleTileCount = voxelMapArray[x, y, z].Count(c => c == true);
                    //Debug.Log(possibleTileCount);
                    //if (x == 0 && y == 1 && z == 1) Debug.Log(possibleTileCount);

                    if (possibleTileCount < lowestEntropy && possibleTileCount > 0)
                    {
                        lowestEntropy = possibleTileCount;
                        lowestEntropyPosition = new Vector3Int(x, y, z);
                    }
                }
            }
        }
        return lowestEntropyPosition;
    }

    private VoxelType ChooseVoxelTypeAt(int x, int y, int z)
    {
        List<VoxelType> choices = new List<VoxelType>();
        List<VoxelType> withinVolume = new List<VoxelType>(); // No connections to the outside
        for (int i = 0; i < voxelCount; i++)
        {
            if (voxelMapArray[x, y, z][i])
            {
                choices.Add(voxelTypes[i]);
                if (!ConnectsOutside(x, y, z, i)) withinVolume.Add(voxelTypes[i]);
            }
        }

        // Try to take one from the within volume list first
        if (withinVolume.Count > 0)
        {
            return withinVolume[Random.Range(0, withinVolume.Count)];
        }

        // Choose a random voxel type from the list of possible voxel types
        if (choices.Count > 1)
        {
            return choices[Random.Range(1, choices.Count)];
        }
        else
        {
            return choices[0];
        }
    }

    private bool ConnectsOutside(int x, int y, int z, int i)
    {
        // Lower bounds
        if (x == 0 && voxelTypes[i].connections[(int)Direction.West] != 0) { return true; }
        if (y == 0 && voxelTypes[i].connections[(int)Direction.Down] != 0) { return true; }
        if (z == 0 && voxelTypes[i].connections[(int)Direction.South] != 0) { return true; }

        // Upper bounds
        if (x == dimensions.x-1 && voxelTypes[i].connections[(int)Direction.East] != 0) { return true; }
        if (y == dimensions.y - 1 && voxelTypes[i].connections[(int)Direction.Up] != 0) { return true; }
        if (z == dimensions.z - 1 && voxelTypes[i].connections[(int)Direction.North] != 0) { return true; }

        // Could not find any connection to the outside
        return false;
    }

    // Process the voxels that were have been but in the voxelsToProcess stack. Returns list of coordinates for voxels that have been set
    private List<Vector3Int> ProcessVoxels()
    {
        int maxIterations = 1000;
        int i = 0;
        bool setVoxel = false;
        List<Vector3Int> setVoxels = new List<Vector3Int>();
        while (voxelsToProcess.Count > 0 && maxIterations > i)
        {
            Vector3Int voxelPosition = voxelsToProcess.Pop();

            if (voxelMap[voxelPosition.x, voxelPosition.y, voxelPosition.z] == -1 && GetEntropy(voxelPosition) == 1)
            {
                // We have a single chunk type, so we can set it
                for (int j = voxelCount - 1; j >= 0; j--)
                {
                    if (!setVoxel && voxelMapArray[voxelPosition.x, voxelPosition.y, voxelPosition.z][j])
                    {
                        voxelMap[voxelPosition.x, voxelPosition.y, voxelPosition.z] = j;
                        //Debug.Log(voxelMap[voxelPosition.x, voxelPosition.y, voxelPosition.z].name + " has been decided");
                        setVoxel = true;
                    }
                    voxelMapArray[voxelPosition.x, voxelPosition.y, voxelPosition.z][j] = false;
                }
            }

            if (voxelMap[voxelPosition.x, voxelPosition.y, voxelPosition.z] != -1)
            {
                // I've realized that at least in step mode, this will make some voxels not be instantiated
                //Debug.Log("I have decided on " + voxelMap[voxelPosition.x, voxelPosition.y, voxelPosition.z].name);
                UpdateNeighbors(voxelPosition);
                setVoxels.Add(voxelPosition);
            }
            i++;
            setVoxel = false;
        }
        return setVoxels;
    }

    // Update the neighbors of the given chunk position
    private void UpdateNeighbors(Vector3Int voxelPosition)
    {
        for (Direction i = 0; i <= Direction.Down; i++)
        {
            Vector3Int neighborPosition = voxelPosition;
            switch (i)
            {
                case Direction.North:
                    neighborPosition.z += 1;
                    break;
                case Direction.South:
                    neighborPosition.z -= 1;
                    break;
                case Direction.East:
                    neighborPosition.x += 1;
                    break;
                case Direction.West:
                    neighborPosition.x -= 1;
                    break;
                case Direction.Down:
                    neighborPosition.y -= 1;
                    break;
                case Direction.Up:
                    neighborPosition.y += 1;
                    break;
            }

            if (neighborPosition.x >= 0 && neighborPosition.x < dimensions.x && neighborPosition.y >= 0 && neighborPosition.y < dimensions.y && neighborPosition.z >= 0 && neighborPosition.z < dimensions.z)
            {

                if (voxelMap[neighborPosition.x, neighborPosition.y, neighborPosition.z] == -1 && voxelMapArray[neighborPosition.x, neighborPosition.y, neighborPosition.z].Count(c => c == true) > 1)
                {
                    // See if there is a connection from the current voxel to the neighbor
                    VoxelType v = voxelTypes[voxelMap[voxelPosition.x, voxelPosition.y, voxelPosition.z]];
                    //Debug.Log(v.name);
                    int originConnection = v.connections[(int)i];
                    bool found = false;

                    /*
                    Debug.Log(voxelPosition);
                    Debug.Log(neighborPosition);*/

                    if (originConnection != 0)
                    {
                        // If there is a connection, the neighbor must have a connection to the current voxel as well, with the same connection ID
                        for (int j = 0; j < voxelCount; j++)
                        {
                            if (voxelMapArray[neighborPosition.x, neighborPosition.y, neighborPosition.z][j])
                            {
                                VoxelType voxel = voxelTypes[j];

                                int d = (int)i % 2 == 0 ? (int)i + 1 : (int)i - 1; // Get the opposite direction

                                // Remove if there is *NO* connection
                                if (voxel.connections[d] != originConnection)
                                {
                                    voxelMapArray[neighborPosition.x, neighborPosition.y, neighborPosition.z][j] = false;
                                    found = true;
                                }
                            }
                        }
                    }
                    else
                    {
                        // If there is no connection, the neighbor must not have a connection to the current voxel
                        for (int j = 0; j < voxelCount; j++)
                        {
                            if (voxelMapArray[neighborPosition.x, neighborPosition.y, neighborPosition.z][j])
                            {
                                VoxelType voxel = voxelTypes[j];

                                int d = (int)i % 2 == 0 ? (int)i + 1 : (int)i - 1;  // Get the opposite direction

                                // Remove if there IS a connection, no matter the connection ID
                                if (voxel.connections[d] > 0)
                                {
                                    voxelMapArray[neighborPosition.x, neighborPosition.y, neighborPosition.z][j] = false;
                                    found = true;
                                }
                            }
                        }
                    }

                    if (found)
                    {
                        voxelsToProcess.Push(neighborPosition);
                    }
                }
            }
        }
    }

    // Get the entropy of a chunk
    private int GetEntropy(Vector3Int chunkPosition)
    {
        return voxelMapArray[chunkPosition.x, chunkPosition.y, chunkPosition.z].Count(c => c == true);
    }

    private void OnDrawGizmos()
    {
        if (voxelMap != null)
        {
            for (int x = 0; x < dimensions.x; x++)
            {
                for (int z = 0; z < dimensions.z; z++)
                {
                    for (int y = 0; y < dimensions.y; y++)
                    {
                        Vector3 chunkPosition = new Vector3(x, 0, z);
                        Gizmos.color = Color.black;
                        Gizmos.DrawCube(chunkPosition, new Vector3(1, 1, 1));
                    }
                }
            }
        }
    }
}

