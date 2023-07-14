using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


[System.Serializable]
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
