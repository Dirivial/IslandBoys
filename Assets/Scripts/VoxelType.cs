using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


[System.Serializable]
public class VoxelType
{
    public int x, y, z;
    public GameObject voxelObject;
    public string name;
    public Symmetry symmetry;
    public int[] connections;
    public Quaternion rotation;

    public VoxelType(int x, int y, int z, GameObject voxelObject, string name, Symmetry symmetry, Quaternion rotation, int[] connections)
    {
        this.x = x;
        this.y = y;
        this.z = z;
        this.rotation = rotation;
        this.voxelObject = voxelObject;
        this.symmetry = symmetry;
        this.name = name;
        this.connections = new int[6];
        for (int i = 0; i < 6; i++)
        {
            this.connections[i] = connections[i];
        }
    }

    public Vector3Int GetCoordinate()
    {
        return new Vector3Int(x, y, z);
    }

    public void ClearConnections()
    {
        this.connections = new int[6];
    }

    public void SetConnectionsTo(int[] connections)
    {
        this.connections = new int[6];
        for (int i = 0;i < connections.Length;i++)
        {
            this.connections[i]= connections[i];
        }
    }

    public void AddConnection(Direction direction, int id)
    {
        this.connections[(int)direction] = id;
    }

    public void SwapConnectionsFromTo(Direction dir1, Direction dir2)
    {
        int temp = connections[(int)dir1];
        connections[(int)dir1] = 0;
        connections[(int)dir2] = temp;
    }
}

public class Voxel
{
    int x, y, z;
    int voxelTypeIndex;

    public int VoxelTypeIndex
    {
        get { return voxelTypeIndex; }
        set { voxelTypeIndex = value; }
    }

    public Vector3Int Coordinate
    {
        get { return new Vector3Int(x, y, z); }
    }

    public Voxel(int x, int y, int z, int voxelTypeIndex)
    {
        this.x = x;
        this.y = y;
        this.z = z;
        this.voxelTypeIndex = voxelTypeIndex;
    }


}

