using UnityEngine;
using System.Collections.Generic;

public class IslandGenerator
{
  public static Cell[,] GenerateIslandFromAnimationCurve(int sizeX, int sizeY, AnimationCurve heightCurveX, AnimationCurve heightCurveY, AnimationCurve dropOffCurve, float waterLevel, float heightMultiplier)
  {

    Vector3 centerPosition = new Vector3(sizeX, 0f, sizeY) / 2f;

    float[,] heightMap = new float[sizeX, sizeY];

    for (int y = 0; y < sizeY; y++)
    {
      for (int x = 0; x < sizeX; x++)
      {

        float height = heightCurveX.Evaluate(x / (float)sizeX) + heightCurveY.Evaluate(y / (float)sizeY) - dropOffCurve.Evaluate(Vector3.Distance(new Vector3(x, 0f, y), centerPosition) / (sizeX / 2f));

        if ((x > 0 && x < sizeX - 1 && y > 0 && y < sizeY - 1))
        {
          float cellValue = Mathf.RoundToInt(height * heightMultiplier) / 2f;
          heightMap[x, y] = cellValue;
        }
        else
        {
          heightMap[x, y] = -1;
        }
      }
    }

    Cell[,] cells = new Cell[sizeX, sizeY];
    for (int y = 0; y < sizeY; y++)
    {
      for (int x = 0; x < sizeX; x++)
      {
        float height = heightMap[x, y];
        if (height < waterLevel)
        {
          cells[x, y] = new Cell(true, -1.5f);
        }
        else
        {
          cells[x, y] = new Cell(false, height);
        }
      }
    }
    return cells;
  }

}