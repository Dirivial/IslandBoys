using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public struct PrefabToHeight
{
  public GameObject prefab;
  public float height;
}

public class Island : MonoBehaviour
{

  public List<PrefabToHeight> prefabHeights;
  public GameObject edgePrefab;
  public AnimationCurve heightCurveX;
  public AnimationCurve heightCurveY;
  public AnimationCurve dropOffCurve;
  public float waterLevel = .4f;
  public float heightMultiplier = 5f;

  public float cellSize = 1f;

  public int sizeX = 100;
  public int sizeY = 100;
  private Cell[,] grid;

  private void Start()
  {

    grid = IslandGenerator.GenerateIslandFromAnimationCurve(sizeX, sizeY, heightCurveX, heightCurveY, dropOffCurve, waterLevel, heightMultiplier);

    for (int y = 0; y < sizeY; y++)
    {
      for (int x = 0; x < sizeX; x++)
      {
        Cell cell = grid[x, y];
        for (int i = prefabHeights.Count - 1; i >= 0; i--)
        {
          if (cell.height >= prefabHeights[i].height)
          {
            GameObject prefab = prefabHeights[i].prefab;
            GameObject go = Instantiate(prefab, new Vector3(x * cellSize, cell.height * cellSize, y * cellSize), Quaternion.identity);
            go.transform.parent = transform;
            break;
          }
        }
        if (x == 0 || x == sizeX - 1 || y == 0 || y == sizeY - 1)
        {
          SpawnEdgeCell(x, y);
        }
      }
    }
  }

  private void SpawnEdgeCell(int x, int y)
  {
    GameObject edge = Instantiate(edgePrefab, new Vector3(x * cellSize, 25f, y * cellSize), Quaternion.identity);
    edge.transform.parent = transform;
  }
}