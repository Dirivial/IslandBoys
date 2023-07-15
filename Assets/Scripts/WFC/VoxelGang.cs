using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoxelGang : MonoBehaviour
{

    [SerializeField] private List<VoxelType> voxelTypes;
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

    public List<VoxelType> GetVoxelTypes()
    {
        return voxelTypes;
    }

    private void ComputeRotations()
    {
        List<VoxelType> newVoxelTypes = new List<VoxelType>();
        foreach (VoxelType voxelType in voxelTypes)
        {
            Symmetry sym = voxelType.symmetry;

            switch (sym)
            {
                case Symmetry.L:
                    for (int i = 1; i < 4; i++)
                    {
                        VoxelType voxel = new VoxelType(voxelType.voxelObject, voxelType.name + " " + i, voxelType.symmetry, Quaternion.Euler(0, 90 * i, 0), voxelType.connections);
                        List<int> directions = new List<int>();
                        for (int j = 0; j < 4; j++)
                        {
                            if (voxel.connections[j] > 0)
                            {
                                directions.Add((int)RotateClockwise((Direction)j, i));
                                directions.Add(voxel.connections[j]);
                            }
                        }

                        voxel.ClearConnections();

                        for (int j = 0; j < directions.Count; j += 2)
                        {
                            voxel.AddConnection((Direction)directions[j], directions[j + 1]);
                        }
                        newVoxelTypes.Add(voxel);
                    }
                    break;
                case Symmetry.T:
                    for (int i = 1; i < 4; i++)
                    {
                        VoxelType voxel = new VoxelType(voxelType.voxelObject, voxelType.name + " " + i, voxelType.symmetry, Quaternion.Euler(0, 90 * i, 0), voxelType.connections);
                        List<int> directions = new List<int>();
                        for (int j = 0; j < 4; j++)
                        {
                            if (voxel.connections[j] > 0)
                            {
                                directions.Add(j);
                            }
                        }
                        voxel.SwapConnectionsFromTo((Direction)directions[0], RotateClockwise((Direction)directions[0], i));
                        newVoxelTypes.Add(voxel);
                    }
                    break;
                case Symmetry.I:
                    VoxelType my_voxel = new VoxelType(voxelType.voxelObject, voxelType.name + " " + 1, voxelType.symmetry, Quaternion.Euler(0, 90, 0), voxelType.connections);
                    List<int> dirs = new List<int>();
                    for (int j = 0; j < 6; j++)
                    {
                        if (my_voxel.connections[j] > 0)
                        {
                            dirs.Add(j);
                        }
                    }

                    my_voxel.SwapConnectionsFromTo((Direction)dirs[0], RotateClockwise((Direction)dirs[0], 1));
                    my_voxel.SwapConnectionsFromTo((Direction)dirs[1], RotateClockwise((Direction)dirs[1], 1));
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
