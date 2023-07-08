using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
  private static GridManager instance;
  public static GridManager Instance { get { return instance; } }

  public int gridSizeX;
  public int gridSizeY;
  public int cellSize;

  private Cell[,] grid;

  private void Awake()
  {
    // Ensure only one instance of the GridManager exists
    if (instance != null && instance != this)
    {
      Destroy(this.gameObject);
      return;
    }

    instance = this;
    grid = new Cell[gridSizeX, gridSizeY];
  }

  public Vector3 GetWorldPosition(int x, int y)
  {
    // Convert grid coordinates to world coordinates
    return new Vector3(x * cellSize, 0, y * cellSize);
  }

  public Vector3 GetGridPosition(Vector3 worldPosition)
  {
    // Convert world coordinates to grid coordinates
    return new Vector3(Mathf.FloorToInt(worldPosition.x / cellSize), worldPosition.y, Mathf.FloorToInt(worldPosition.z / cellSize));
  }

  public Cell GetGridCell(int x, int y)
  {
    // Return the cell at the given grid coordinates
    return grid[x, y];
  }

  public void SetGridCell(int x, int y, Cell cell)
  {
    // Set the cell at the given grid coordinates
    grid[x, y] = cell;
  }
}
