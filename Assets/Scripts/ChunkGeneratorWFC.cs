using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

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

[Serializable]
public class Voxel
{
    public int x, y, z;
    public GameObject voxelObject;
    public string name;
    public Symmetry symmetry;
    private bool decided;
    public List<Connection> connections;
    public Quaternion rotation;

    public Voxel(int x, int y, int z, GameObject voxelObject, string name, Symmetry symmetry, List<Connection> connections)
    {
        this.x = x;
        this.y = y;
        this.z = z;
        this.rotation = Quaternion.identity;
        this.voxelObject = voxelObject;
        this.symmetry = symmetry;
        this.name = name;
        decided = false;
        this.connections = new List<Connection>();
        this.connections.AddRange(connections);
     }

    public Vector3Int GetCoordinate()
    {
        return new Vector3Int(x, y, z);
    }

    public void ToggleDecided()
    {
        decided = !decided;
    }

    public bool IsDecided()
    {
        return decided;
    }

    public void AddConnection(Direction dir, int id)
    {
        connections.Add(new Connection(dir, id));
    }

    public bool HasConnection(Direction dir)
    {
        return connections.Any(c => c.dir == dir);
    }

    public Connection GetConnection(Direction dir)
    {
        return connections.First(c => c.dir == dir);
    }

    public void SwapConnectionsFromTo(Direction dir1, Direction dir2)
    {
        for (int i = 0; i < connections.Count; i++)
        {
            if (connections[i].dir == dir1)
            {
                Connection temp = connections[i];
                connections.RemoveAt(i);
                temp.dir = dir2;
                connections.Add(temp);
            }
        }
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
        voxelsToProcess.Clear();
        voxelMap = new Voxel[dimensions.x, dimensions.y, dimensions.z];
        for (int i = boi.Count - 1; i >= 0; i--)
        {
            Destroy(boi[i]);
        }
        setupComplete = false;
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
                    if (y == 0)
                    {
                        voxelMap[x, y, z] = voxelTypes[1];
                        voxelMap[x, y, z].ToggleDecided();
                        voxelMapArray[x, y, z][1] = true;
                        voxelsToProcess.Push(new Vector3Int(x, y, z));
                    }
                    else // The rest should be empty
                    {
                        voxelMap[x, y, z] = new Voxel(x, y, z, null, "", Symmetry.X, new List<Connection>());
                        for (int i = 0; i < voxelTypes.Count; i++)
                        {
                            voxelMapArray[x, y, z][i] = true;
                        }
                        voxelMapArray[x, y, z][1] = false;
                    }
                }
            }
        }
        ProcessChunks();

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
            ProcessChunks();
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
        Debug.Log(nextWave);

        if (nextWave.x != -1 && nextWave.y != -1 && nextWave.z != -1)
        {
            PickTileAt(nextWave);
            ProcessChunks();
            InstantiateDeezNut(nextWave.x, nextWave.y, nextWave.z);
        } else
        {
            Debug.Log("Could not get any further");
        }
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

    private void InstantiateDeezNut(int x, int y, int z)
    {
        if (voxelMap[x, y, z].IsDecided())
        {
            //Debug.Log("Heey " + voxelMap[x, y, z].name + " " + x + " " + y + " " + z);
            GameObject obj = Instantiate(voxelMap[x, y, z].voxelObject, new Vector3(x * voxelSize, y * voxelSize, z * voxelSize), voxelMap[x, y, z].rotation);
            obj.transform.parent = transform;
            boi.Add(obj);
        }
        else
        {
            Debug.Log("Uhhh");
        }
    }

/*    private void ComputeRotations()
    {
        List<Voxel> newVoxelTypes = new List<Voxel>();
        foreach (Voxel voxelType in voxelTypes)
        {
            Symmetry sym = voxelType.symmetry;

            // Hacky implementation for now, if the connections has more than one connection id this will fail
            switch (sym)
            {
                case Symmetry.L:
                    for (int i = 1; i < 4; i++)
                    {
                        Voxel voxel = new Voxel(voxelType.x, voxelType.y, voxelType.z, voxelType.color, voxelType.voxelObject, voxelType.name + " " + i, voxelType.symmetry, voxelType.connections);
                        voxel.rotation = Quaternion.Euler(0, 90 * i, 0);
                        List<Direction> directions = new List<Direction>();
                        foreach (Connection c in voxel.connections)
                        {
                            directions.Add(c.dir);
                        }
                        Direction d1 = directions[0];
                        Direction d2 = directions[1];

                        voxel.SwapConnectionsFromTo(directions[0], RotateClockwise(d1, i));
                        voxel.SwapConnectionsFromTo(directions[1], RotateClockwise(d2, i));
                        newVoxelTypes.Add(voxel);
                    }
                    break;
                case Symmetry.T:
                    for (int i = 1; i < 4; i++)
                    {
                        Voxel voxel = new Voxel(voxelType.x, voxelType.y, voxelType.z, voxelType.color, voxelType.voxelObject, voxelType.name + " " + i, voxelType.symmetry, voxelType.connections);
                        voxel.rotation = Quaternion.Euler(0, 90 * i, 0);
                        Direction direction = voxel.connections[0].dir;
                        foreach (Connection c in voxel.connections)
                        {
                            if (c.dir != Direction.Up && c.dir != Direction.Down) direction = c.dir;
                        }
                        voxel.SwapConnectionsFromTo(direction, RotateClockwise(direction, i));
                        newVoxelTypes.Add(voxel);
                    }
                    break;
                case Symmetry.I:
                    Voxel my_voxel = new Voxel(voxelType.x, voxelType.y, voxelType.z, voxelType.color, voxelType.voxelObject, voxelType.name + " " + 1, voxelType.symmetry, voxelType.connections);
                    my_voxel.rotation = Quaternion.Euler(0, 90, 0);
                    List<Direction> dirs = new List<Direction>();
                    foreach (Connection c in my_voxel.connections)
                    {
                        dirs.Add(c.dir);
                    }
                    Direction dd1 = dirs[0];
                    Direction dd2 = dirs[1];

                    my_voxel.SwapConnectionsFromTo(dirs[0], RotateClockwise(dd1, 1));
                    my_voxel.SwapConnectionsFromTo(dirs[1], RotateClockwise(dd2, 1));
                    newVoxelTypes.Add(my_voxel);
                    break;
                case Symmetry.D:
                    // Todo, implement this shiz
                    break;
                default:
                    // No rotations / reflections
                    break;
            }
        }
        // Save the new voxel types
        voxelTypes.AddRange(newVoxelTypes);
    }*/

/*    // Rotate a direction clockwise, X times
    private Direction RotateClockwise(Direction dir, int times)
    {
        Direction direction;
        switch (dir)
        {
            case Direction.North:
                direction = Direction.East;
                break;
            case Direction.East:
                direction = Direction.South;
                break;
            case Direction.South:
                direction = Direction.West;
                break;
            case Direction.West:
                direction = Direction.North;
                break;
            default:
                Debug.LogError("Invalid direction passed");
                return Direction.North;
        }
        if (times > 1) return RotateClockwise(direction, times - 1);
        else return direction;
    }*/

    // Pick a random tile at the given chunk position, using the possible tiles at that position
    private void PickTileAt(Vector3Int voxelPosition)
    {
        Voxel v = ChooseVoxelTypeAt(voxelPosition.x, voxelPosition.y, voxelPosition.z);
        voxelMap[voxelPosition.x, voxelPosition.y, voxelPosition.z].voxelObject = v.voxelObject;
        voxelMap[voxelPosition.x, voxelPosition.y, voxelPosition.z].rotation = v.rotation;
        voxelMap[voxelPosition.x, voxelPosition.y, voxelPosition.z].name = v.name;
        voxelMap[voxelPosition.x, voxelPosition.y, voxelPosition.z].connections = v.connections;
        voxelMap[voxelPosition.x, voxelPosition.y, voxelPosition.z].ToggleDecided();
        for (int i = 0; i < voxelTypes.Count; i++) {
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
                    int possibleTileCount = voxelMapArray[x,y,z].Count(c => c == true);
                    //Debug.Log(possibleTileCount);

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
        } else
        {
            v = voxelTypesToChooseFrom[Random.Range(0, voxelTypesToChooseFrom.Count)];
        }
        /*Debug.Log("I chose " + v.name);*/
        return v;
    }

    // Process the chunks that were have been but in the chunksToProcess stack
    private void ProcessChunks()
    {
        int maxIterations = 1000;
        int i = 0;
        bool setVoxel = false;
        while (voxelsToProcess.Count > 0 && maxIterations > i)
        {
            Vector3Int voxelPosition = voxelsToProcess.Pop();
            if (!voxelMap[voxelPosition.x, voxelPosition.y, voxelPosition.z].IsDecided() && GetEntropy(voxelPosition) <= 2)
            {
                // We have a single chunk type, so we can set it
                for (int j = voxelCount - 1; j >= 0; j--)
                {
                    if (!setVoxel && voxelMapArray[voxelPosition.x, voxelPosition.y, voxelPosition.z][j])
                    {
                        voxelMap[voxelPosition.x, voxelPosition.y, voxelPosition.z] = voxelTypes[j];
                        voxelMap[voxelPosition.x, voxelPosition.y, voxelPosition.z].ToggleDecided();
                        //Debug.Log(voxelMap[voxelPosition.x, voxelPosition.y, voxelPosition.z].name + " has been decided");
                        setVoxel = true;
                    }
                    voxelMapArray[voxelPosition.x, voxelPosition.y, voxelPosition.z][j] = false;
                }
            }

            if (voxelMap[voxelPosition.x, voxelPosition.y, voxelPosition.z].IsDecided())
            {
                UpdateNeighbors(voxelPosition);
            }
            i++;
            setVoxel = false;
        }
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
                                } else if ((int)i % 2 == 1 && !voxel.HasConnection(i - 1))
                                {
                                    voxelMapArray[neighborPosition.x, neighborPosition.y, neighborPosition.z][j] = false;
                                }
                            }
                        }
                    } else
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

