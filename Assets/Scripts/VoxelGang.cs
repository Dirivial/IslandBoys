using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoxelGang : MonoBehaviour
{

    [SerializeField] private List<Voxel> voxelTypes;
    [SerializeField] private int voxelSize = 3;

    private void Awake()
    {
        ComputeRotations();
        Debug.Log("Rotations computed, voxel types: " + voxelTypes.Count);
    }

    public int GetVoxelTypesCount()
    {
        return voxelTypes.Count;
    }

    public int GetVoxelSize()
    {
        return voxelSize;
    } 

    public List<Voxel> GetVoxelTypes()
    {
        return voxelTypes;
    }

    private void ComputeRotations()
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
                        Voxel voxel = new Voxel(voxelType.x, voxelType.y, voxelType.z, voxelType.voxelObject, voxelType.name + " " + i, voxelType.symmetry, voxelType.connections);
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
                        Voxel voxel = new Voxel(voxelType.x, voxelType.y, voxelType.z, voxelType.voxelObject, voxelType.name + " " + i, voxelType.symmetry, voxelType.connections);
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
                    Voxel my_voxel = new Voxel(voxelType.x, voxelType.y, voxelType.z, voxelType.voxelObject, voxelType.name + " " + 1, voxelType.symmetry, voxelType.connections);
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
    }

    // Rotate a direction clockwise, X times
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
    }
}
