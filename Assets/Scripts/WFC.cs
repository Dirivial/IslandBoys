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

    private List<TileType> tileTypes;
    [SerializeField] public Vector3Int dimensions = new Vector3Int(5, 5, 5);

    private int tileCount = 1;
    private bool setupComplete = false;
    private int[,,] tileMap;
    private bool[,,][] tileMapArray;
    private Stack<Vector3Int> tilesToProcess;
    private Stack<SaveState> saveStates;

    private List<GameObject> boi;
    private TileGang tileGang;
    private int tileSize = 3;

    private void Awake()
    {
        tilesToProcess = new Stack<Vector3Int>();
        tileMap = new int[dimensions.x, dimensions.y, dimensions.z];

        // Store the initial entropy of each coordinate in the chunk map
        tileMapArray = new bool[dimensions.x, dimensions.y, dimensions.z][];
        boi = new List<GameObject>();
        tileGang = GetComponentInChildren<TileGang>();
    }

    void Start()
    {
        tileCount = tileGang.GetTileTypesCount();
        tileSize = tileGang.GetTileSize();
        tileTypes = tileGang.GetTileTypes();

        // For testing
        //InstantiateTileTypes();

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
                    d += " " + tileMapArray[x, y, z].Count(c => c == true);
                }
                Debug.Log(d);
            }
        }
    }


    private void PrintConnectionDirs()
    {
        string debugString;
        for (int i = 0; i < tileCount; i++)
        {
            TileType v = tileTypes[i];
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
        tilesToProcess.Clear();
        tileMap = new int[dimensions.x, dimensions.y, dimensions.z];
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
        tilesToProcess = new Stack<Vector3Int>();
        tileMap = new int[dimensions.x, dimensions.y, dimensions.z];

        // Store the initial entropy of each coordinate in the chunk map
        tileMapArray = new bool[dimensions.x, dimensions.y, dimensions.z][];
        for (int x = 0; x < dimensions.x; x++)
        {
            for (int z = 0; z < dimensions.z; z++)
            {
                for (int y = 0; y < dimensions.y; y++)
                {
                    tileMapArray[x, y, z] = new bool[tileCount];

                    // Set floor to be ground tiles
/*                    if (y == 0)
                    {                        
                        //tileMap[x, y, z] = 1;
                        //tileMapArray[x, y, z][1] = true;
                        //tilesToProcess.Push(new Vector3Int(x, y, z));
                        tileMap[x, y, z] = -1;
                        for (int i = 1; i < tileTypes.Count; i++) // THIS MIGHT MAKE STUFF BREAK IN THE FUTURE
                        {
                            tileMapArray[x, y, z][i] = true;
                        }
                    }
                    else // The rest should be empty
                    {*/

                        tileMap[x, y, z] = -1;
                        for (int i = 0; i < tileTypes.Count; i++) // THIS MIGHT MAKE STUFF BREAK IN THE FUTURE
                        {
                            tileMapArray[x, y, z][i] = true;
                        }
/*                    }*/
                }
            }
        }
        int cx = Random.Range(0, dimensions.x);
        int cy = Random.Range(0, dimensions.y);
        int cz = Random.Range(0, dimensions.z);
        int c = Random.Range(0, tileTypes.Count);

        tileMap[cx, cy, cz] = c;
        for (int i = 0; i < tileTypes.Count; i++) // THIS MIGHT MAKE STUFF BREAK IN THE FUTURE
        {
            tileMapArray[cx, cy, cz][i] = false;
        }

        ProcessTiles();

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
            ProcessTiles();
            nextWave = FindLowestEntropy();
            iterations++;
        }
        InstantiateDeezNuts();
        //PrintArrayCount();
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
            toInstantiate = ProcessTiles();
            InstantiateDeezNut(toInstantiate);
        }
        else
        {
            Debug.Log("Could not get any further");
        }
        //Debug.Log(tileMap[0, 1, 1].name);
    }

    private void PrintStuff()
    {
        // Print out the tiles that have been fiddled on
        for (int x = 0; x < dimensions.x; x++)
        {
            for (int z = 0; z < dimensions.z; z++)
            {
                for (int y = 0; y < dimensions.y; y++)
                {
                    int index = tileMap[x, y, z];
                    if (index != -1)
                    {
                        Debug.Log("Decided: " + tileTypes[index] + " " + new Vector3Int(x, y, z));
                    }
                }
            }
        }
    }

    // Just to make sure that the rotations are correct - this should be kept for later when we add more types of tiles
    private void InstantiateTileTypes()
    {
        float i = 0.0f;
        foreach (TileType tile in tileTypes)
        {
            GameObject obj = Instantiate(tile.tileObject, new Vector3(i * tileSize, 0, 0), tile.rotation);
            obj.transform.localScale = new Vector3(tileGang.ScaleFactor, tileGang.ScaleFactor, tileGang.ScaleFactor);
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
                    int index = tileMap[x, y, z];
                    if (index > 0)
                    {
                        //Debug.Log("Heey " + tileMap[x, y, z].name + " " + x + " " + y + " " + z);
                        GameObject obj = Instantiate(tileTypes[index].tileObject, new Vector3(x * tileSize, y * tileSize, z * tileSize), tileTypes[index].rotation);
                        obj.transform.localScale = new Vector3(tileGang.ScaleFactor, tileGang.ScaleFactor, tileGang.ScaleFactor);
                        obj.transform.parent = transform;
                        boi.Add(obj);
                    } else
                    {
                        GameObject obj = Instantiate(tileTypes[0].tileObject, new Vector3(x * tileSize, y * tileSize, z * tileSize), Quaternion.identity);
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
            TileType tileType = tileTypes[tileMap[v.x, v.y, v.z]];
            GameObject obj = Instantiate(tileType.tileObject, new Vector3(v.x * tileSize, v.y * tileSize, v.z * tileSize), tileType.rotation);
            obj.transform.parent = transform;
            boi.Add(obj);
            //Debug.Log(tileType.name);
        }
    }

    // Pick a random tile at the given tile position, using the possible tiles at that position
    private void PickTileAt(Vector3Int pos)
    {
        TileType v = ChooseTileTypeAt(pos.x, pos.y, pos.z);
        tileMap[pos.x, pos.y, pos.z] = tileTypes.IndexOf(v);

        for (int i = 0; i < tileTypes.Count; i++)
        {
            tileMapArray[pos.x, pos.y, pos.z][i] = false;
        }

        //UpdateNeighbors(tilePosition); // I dunno walter, I guess we just update the neighbors ?? why would I do this?? 

        tilesToProcess.Push(pos);
    }

    // Find the first tile with the lowest entropy
    private Vector3Int FindLowestEntropy()
    {
        // Look for the lowest entropy in the tile map
        int lowestEntropy = tileCount + 1;
        Vector3Int lowestEntropyPosition = new Vector3Int(-1, -1, -1);
        for (int x = 0; x < dimensions.x; x++)
        {
            for (int z = 0; z < dimensions.z; z++)
            {
                for (int y = 0; y < dimensions.y; y++)
                {
                    if (tileMap[x, y, z] != -1) continue;
                    int possibleTileCount = tileMapArray[x, y, z].Count(c => c == true);
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

    private TileType ChooseTileTypeAt(int x, int y, int z)
    {
        List<TileType> choices = new List<TileType>();
        List<TileType> withinVolume = new List<TileType>(); // No connections to the outside
        List<TileType> groundTiles = new List<TileType>(); 
        
        if (y == 0)
        {
            for (int i = 0; i < tileCount; i++)
            {
                if (tileMapArray[x, y, z][i])
                {
                    choices.Add(tileTypes[i]);
                    if (!ConnectsOutside(x, y, z, i)) withinVolume.Add(tileTypes[i]);
                    if (tileTypes[i].CanTouchGround) groundTiles.Add(tileTypes[i]);
                }
            }
            if (groundTiles.Count > 0)
            {
                return groundTiles[Random.Range(0, groundTiles.Count)];
            } else
            {
                Debug.Log("This should force a restart, or just an empty block");
            }
        } else
        {
            for (int i = 0; i < tileCount; i++)
            {
                if (tileMapArray[x, y, z][i])
                {
                    choices.Add(tileTypes[i]);
                    if (!ConnectsOutside(x, y, z, i)) withinVolume.Add(tileTypes[i]);
                }
            }
        }



        // Try to take one from the within volume list first
        if (withinVolume.Count > 0)
        {
            return withinVolume[Random.Range(0, withinVolume.Count)];
        }

        // Choose a random tile type from the list of possible tile types
        if (choices.Count > 1)
        {
            return choices[Random.Range(1, choices.Count)];
        }
        else
        {
            return choices[0];
        }
    }

    // Used to see if a tiles connects to the outside
    private bool ConnectsOutside(int x, int y, int z, int i)
    {
        // Lower bounds
        if (x == 0 && tileTypes[i].connections[(int)Direction.West] > 0) { return true; }
        if (y == 0 && tileTypes[i].connections[(int)Direction.Down] > 0) { return true; }
        if (z == 0 && tileTypes[i].connections[(int)Direction.South] > 0) { return true; }

        // Upper bounds
        if (x == dimensions.x - 1 && tileTypes[i].connections[(int)Direction.East] > 0) { return true; }
        if (y == dimensions.y - 1 && tileTypes[i].connections[(int)Direction.Up] > 0) { return true; }
        if (z == dimensions.z - 1 && tileTypes[i].connections[(int)Direction.North] > 0) { return true; }

        // Could not find any connection to the outside
        return false;
    }

    // Process the tiles that were have been but in the tilesToProcess stack. Returns list of coordinates for tiles that have been set
    private List<Vector3Int> ProcessTiles()
    {
        int maxIterations = 1000;
        int i = 0;
        bool setTile = false;
        List<Vector3Int> setTiles = new List<Vector3Int>();
        while (tilesToProcess.Count > 0 && maxIterations > i)
        {
            Vector3Int tilePosition = tilesToProcess.Pop();

            if (tileMap[tilePosition.x, tilePosition.y, tilePosition.z] == -1 && GetEntropy(tilePosition) == 1)
            {
                // We have a single chunk type, so we can set it
                for (int j = tileCount - 1; j >= 0; j--)
                {
                    if (!setTile && tileMapArray[tilePosition.x, tilePosition.y, tilePosition.z][j])
                    {
                        tileMap[tilePosition.x, tilePosition.y, tilePosition.z] = j;
                        //Debug.Log(tileMap[tilePosition.x, tilePosition.y, tilePosition.z].name + " has been decided");
                        setTile = true;
                    }
                    tileMapArray[tilePosition.x, tilePosition.y, tilePosition.z][j] = false;
                }
            }

            if (tileMap[tilePosition.x, tilePosition.y, tilePosition.z] != -1)
            {
                // I've realized that at least in step mode, this will make some tiles not be instantiated
                //Debug.Log("I have decided on " + tileMap[tilePosition.x, tilePosition.y, tilePosition.z].name);
                UpdateNeighbors(tilePosition);
                setTiles.Add(tilePosition);
            }
            i++;
            setTile = false;
        }
        return setTiles;
    }

    // Update the neighbors of the given chunk position
    private void UpdateNeighbors(Vector3Int tilePosition)
    {
        for (Direction i = 0; i <= Direction.Down; i++)
        {
            Vector3Int neighborPosition = tilePosition;
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

                if (tileMap[neighborPosition.x, neighborPosition.y, neighborPosition.z] == -1 && tileMapArray[neighborPosition.x, neighborPosition.y, neighborPosition.z].Count(c => c == true) > 1)
                {
                    // See if there is a connection from the current tile to the neighbor
                    TileType v = tileTypes[tileMap[tilePosition.x, tilePosition.y, tilePosition.z]];
                    //Debug.Log(v.name);
                    int originConnection = v.connections[(int)i];
                    bool found = false;

                    /*
                    Debug.Log(tilePosition);
                    Debug.Log(neighborPosition);*/
                    // Remove all possible tiles from neighbor
                    if (originConnection == -1)
                    {
                        for (int j = 0; j < tileCount; j++)
                        {
                            if (tileMapArray[neighborPosition.x, neighborPosition.y, neighborPosition.z][j])
                            {
                                tileMapArray[neighborPosition.x, neighborPosition.y, neighborPosition.z][j] = false;
                                found = true;
                            }
                        }
                    }
                    else
                    {
                        if (originConnection != 0)
                        {
                            // If there is a connection, the neighbor must have a connection to the current tile as well, with the same connection ID
                            for (int j = 0; j < tileCount; j++)
                            {
                                if (tileMapArray[neighborPosition.x, neighborPosition.y, neighborPosition.z][j])
                                {
                                    TileType tile = tileTypes[j];

                                    int d = (int)i % 2 == 0 ? (int)i + 1 : (int)i - 1; // Get the opposite direction

                                    // Remove if there is *NO* connection
                                    if (tile.connections[d] != originConnection)
                                    {
                                        tileMapArray[neighborPosition.x, neighborPosition.y, neighborPosition.z][j] = false;
                                        found = true;
                                    }
                                }
                            }
                        }
                        else
                        {
                            // If there is no connection, the neighbor must not have a connection to the current tile
                            for (int j = 0; j < tileCount; j++)
                            {
                                if (tileMapArray[neighborPosition.x, neighborPosition.y, neighborPosition.z][j])
                                {
                                    TileType tile = tileTypes[j];

                                    int d = (int)i % 2 == 0 ? (int)i + 1 : (int)i - 1;  // Get the opposite direction

                                    // Remove if there IS a connection, no matter the connection ID
                                    if (tile.connections[d] > 0)
                                    {
                                        tileMapArray[neighborPosition.x, neighborPosition.y, neighborPosition.z][j] = false;
                                        found = true;
                                    }
                                }
                            }
                        }
                    }



                    if (found)
                    {
                        tilesToProcess.Push(neighborPosition);
                    }
                }
            }
        }
    }

    // Get the entropy of a chunk
    private int GetEntropy(Vector3Int chunkPosition)
    {
        return tileMapArray[chunkPosition.x, chunkPosition.y, chunkPosition.z].Count(c => c == true);
    }

    private void OnDrawGizmos()
    {
        if (tileMap != null)
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

