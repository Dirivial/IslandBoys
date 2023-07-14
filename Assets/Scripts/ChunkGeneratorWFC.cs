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

[Serializable]
public struct Connection
{
    public Direction dir;
    public int id;

    public Connection(Direction dir, int id)
    {
        this.dir = dir;
        this.id = id;
    }
}

public class ChunkGeneratorWFC : MonoBehaviour
{

    private List<Voxel> voxelTypes;
    [SerializeField] public Vector3Int dimensions = new Vector3Int(5, 5, 5);

    private int voxelCount = 1;
    private bool setupComplete = false;
    private Voxel[,,] voxelMap;
    private bool[,,][] voxelMapArray;
    private Stack<Vector3Int> voxelsToProcess;
    /*    private Stack<SaveState> saveStates;*/

    private List<GameObject> boi;
    private VoxelGang voxelGang;
    private int voxelSize = 3;

    private void Awake()
    {
        voxelsToProcess = new Stack<Vector3Int>();
        voxelMap = new Voxel[dimensions.x, dimensions.y, dimensions.z];

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
    }

    public void Clear()
    {
        setupComplete = false;
        voxelsToProcess.Clear();
        voxelMap = new Voxel[dimensions.x, dimensions.y, dimensions.z];
        for (int i = boi.Count - 1; i >= 0; i--)
        {
            Destroy(boi[i]);
        }
    }

    public void Setup()
    {
        Debug.Log("Generating a new model");

        for (int i = boi.Count - 1; i >= 0; i--)
        {
            Destroy(boi[i]);
        }
        boi.Clear();
        voxelsToProcess = new Stack<Vector3Int>();
        voxelMap = new Voxel[dimensions.x, dimensions.y, dimensions.z];

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
                    if (y == 0 && x == 0)
                    {
                        Voxel v = new Voxel(x, y, z, voxelTypes[1].voxelObject, "", Symmetry.X, new List<Connection>(voxelTypes[1].connections));
                        voxelMap[x, y, z] = v;
                        voxelMap[x, y, z].ToggleDecided();
                        //voxelMapArray[x, y, z][1] = true;
                        voxelsToProcess.Push(new Vector3Int(x, y, z));
                    }
                    else // The rest should be empty
                    {

                        voxelMap[x, y, z] = new Voxel(x, y, z, null, "", Symmetry.X, new List<Connection>());
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
                    if (voxelMap[x, y, z].IsDecided())
                    {
                        Debug.Log("Decided: " + voxelMap[x, y, z].name + " " + voxelMap[x, y, z].GetCoordinate());
                    }
                }
            }
        }
    }

    private void PrintConnections()
    {
        // For every type of voxel, print out its connections
        foreach (Voxel voxel in voxelTypes)
        {
            Debug.Log("Name: " + voxel.name + " # " + voxel.connections.Count);
            foreach (Connection connection in voxel.connections)
            {
                Debug.Log(connection.dir + " " + connection.id);
            }
            Debug.Log("----------------");
        }
    }

    // Just to make sure that the rotations are correct - this should be kept for later when we add more types of voxels
    private void InstantiateVoxelTypes()
    {
        float i = 0.0f;
        foreach (Voxel voxel in voxelTypes)
        {
            GameObject obj = Instantiate(voxel.voxelObject, new Vector3(voxel.x * voxelSize + i * voxelSize, voxel.y * voxelSize, voxel.z * voxelSize), voxel.rotation);
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
                    if (voxelMap[x, y, z].IsDecided())
                    {
                        //Debug.Log("Heey " + voxelMap[x, y, z].name + " " + x + " " + y + " " + z);
                        GameObject obj = Instantiate(voxelMap[x, y, z].voxelObject, new Vector3(x * voxelSize, y * voxelSize, z * voxelSize), voxelMap[x, y, z].rotation);
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
            GameObject obj = Instantiate(voxelMap[v.x, v.y, v.z].voxelObject, new Vector3(v.x * voxelSize, v.y * voxelSize, v.z * voxelSize), voxelMap[v.x, v.y, v.z].rotation);
            obj.transform.parent = transform;
            boi.Add(obj);
        }
    }

    // Pick a random tile at the given chunk position, using the possible tiles at that position
    private void PickTileAt(Vector3Int voxelPosition)
    {
        Voxel v = ChooseVoxelTypeAt(voxelPosition.x, voxelPosition.y, voxelPosition.z);
        voxelMap[voxelPosition.x, voxelPosition.y, voxelPosition.z].voxelObject = v.voxelObject;
        voxelMap[voxelPosition.x, voxelPosition.y, voxelPosition.z].rotation = v.rotation;
        voxelMap[voxelPosition.x, voxelPosition.y, voxelPosition.z].name = v.name;
        voxelMap[voxelPosition.x, voxelPosition.y, voxelPosition.z].connections = v.connections;
        voxelMap[voxelPosition.x, voxelPosition.y, voxelPosition.z].ToggleDecided();
        for (int i = 0; i < voxelTypes.Count; i++)
        {
            voxelMapArray[voxelPosition.x, voxelPosition.y, voxelPosition.z][i] = false;
        }

        //UpdateNeighbors(voxelPosition); // I dunno walter, I guess we just update the neighbors ?? why would I do this?? 

        voxelsToProcess.Push(voxelPosition);
    }

    // Find the first chunk with the lowest entropy
    private Vector3Int FindLowestEntropy()
    {
        // Look for the lowest entropy in the chunk map
        int lowestEntropy = voxelCount + 1;
        Vector3Int lowestEntropyPosition = new Vector3Int(-1, -1, -1);
        for (int x = 0; x < dimensions.x; x++)
        {
            for (int z = 0; z < dimensions.z; z++)
            {
                for (int y = 0; y < dimensions.y; y++)
                {
                    if (voxelMap[x, y, z].IsDecided()) continue;
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

    private Voxel ChooseVoxelTypeAt(int x, int y, int z)
    {
        List<Voxel> voxelTypesToChooseFrom = new List<Voxel>();
        for (int i = 0; i < voxelCount; i++)
        {
            if (voxelMapArray[x, y, z][i])
            {
                voxelTypesToChooseFrom.Add(voxelTypes[i]);
            }
        }

        // Find a way to choose voxels that do not exit out to the world. i.e. add constraints
        /*        Debug.Log("I could have chosen from ");
                foreach (Voxel vo in voxelTypesToChooseFrom)
                {
                    Debug.Log(vo.name);
                }
                Debug.Log("-------------");*/

        Voxel v;

        // Choose a random voxel type from the list of possible voxel types
        if (voxelTypesToChooseFrom.Count > 1)
        {
            v = voxelTypesToChooseFrom[Random.Range(1, voxelTypesToChooseFrom.Count)];
        }
        else
        {
            v = voxelTypesToChooseFrom[0];
        }
        /*Debug.Log("I chose " + v.name);*/
        return v;
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

            // Is this even necessary? I'm not sure...
            if (!voxelMap[voxelPosition.x, voxelPosition.y, voxelPosition.z].IsDecided() && GetEntropy(voxelPosition) == 1)
            {
                // We have a single chunk type, so we can set it
                for (int j = voxelCount - 1; j >= 0; j--)
                {
                    if (!setVoxel && voxelMapArray[voxelPosition.x, voxelPosition.y, voxelPosition.z][j])
                    {
                        voxelMap[voxelPosition.x, voxelPosition.y, voxelPosition.z] = voxelTypes[j];
                        voxelMap[voxelPosition.x, voxelPosition.y, voxelPosition.z].ToggleDecided();
                        Debug.Log(voxelMap[voxelPosition.x, voxelPosition.y, voxelPosition.z].name + " has been decided");
                        setVoxel = true;
                        //setVoxels.Add(voxelPosition);
                    }
                    voxelMapArray[voxelPosition.x, voxelPosition.y, voxelPosition.z][j] = false;
                }
            }

            if (voxelMap[voxelPosition.x, voxelPosition.y, voxelPosition.z].IsDecided())
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

                if (!voxelMap[neighborPosition.x, neighborPosition.y, neighborPosition.z].IsDecided() && voxelMapArray[neighborPosition.x, neighborPosition.y, neighborPosition.z].Count(c => c == true) > 1)
                {
                    // See if there is a connection from the current voxel to the neighbor
                    Voxel v = voxelMap[voxelPosition.x, voxelPosition.y, voxelPosition.z];
                    //Debug.Log(v.name);
                    bool voxelHasConnection = voxelMap[voxelPosition.x, voxelPosition.y, voxelPosition.z].HasConnection(i);
                    bool found = false;

                    /*
                    Debug.Log(voxelPosition);
                    Debug.Log(neighborPosition);*/

                    if (voxelHasConnection)
                    {
                        // If there is a connection, the neighbor must have a connection to the current voxel as well
                        for (int j = 0; j < voxelCount; j++)
                        {
                            if (voxelMapArray[neighborPosition.x, neighborPosition.y, neighborPosition.z][j])
                            {
                                Voxel voxel = voxelTypes[j];

                                if ((int)i % 2 == 0 && !voxel.HasConnection(i + 1)) // TODO: Make this utilise id's
                                {
                                    voxelMapArray[neighborPosition.x, neighborPosition.y, neighborPosition.z][j] = false;
                                    found = true;
                                }
                                else if ((int)i % 2 == 1 && !voxel.HasConnection(i - 1))
                                {
                                    voxelMapArray[neighborPosition.x, neighborPosition.y, neighborPosition.z][j] = false;
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
                                Voxel voxel = voxelTypes[j];

                                if ((int)i % 2 == 0 && voxel.HasConnection(i + 1)) // TODO: Make this utilise id's
                                {
                                    voxelMapArray[neighborPosition.x, neighborPosition.y, neighborPosition.z][j] = false;
                                    found = true;
                                }
                                else if ((int)i % 2 == 1 && voxel.HasConnection(i - 1))
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

