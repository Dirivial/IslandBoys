using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public struct DirAndCon
{
    public Direction direction;
    public int connection;
}

[System.Serializable]
public class TileType
{
    public GameObject tileObject;
    public string name;
    public Symmetry symmetry;
    public List<DirAndCon> TileConnections;
    public Quaternion rotation;
    public bool CanTouchGround = false;
    public bool CanRepeatH = false;
    public bool CanRepeatV = false;
    public bool MustStandOn = false;
    public bool MustConnect = false;
    public float weight = 1.0f;

    [HideInInspector] public int[] connections = { 0, 0, 0, 0, 0, 0 };

    public TileType(GameObject tileObject, string name, Symmetry symmetry, 
        Quaternion rotation, int[] connections, float weight, 
        bool CanTouchGround, bool CanRepeatH, bool CanRepeatV, bool MustStandOn, bool MustConnect)
    {
        this.rotation = rotation;
        this.tileObject = tileObject;
        this.symmetry = symmetry;
        this.name = name;
        this.connections = new int[6];
        this.weight = weight;
        for (int i = 0; i < 6; i++)
        {
            this.connections[i] = connections[i];
        }
        this.CanTouchGround = CanTouchGround; this.CanRepeatH = CanRepeatH; this.CanRepeatV = CanRepeatV; this.MustStandOn = MustStandOn; this.MustConnect = MustConnect;
    }

    public void UpdateConnections()
    {
        this.connections = new int[6];

        foreach (DirAndCon dc in TileConnections)
        {
            this.connections[(int)dc.direction] = dc.connection;
        }
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

