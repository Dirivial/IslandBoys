using UnityEngine;
using System.Collections.Generic;

public class IslandGenerator
{
    public static Cell[,] GenerateIslandFromAnimationCurve(int sizeX, int sizeY, AnimationCurve heightCurveX, AnimationCurve heightCurveY, float waterLevel) {

      Vector3 centerPosition = new Vector3(sizeX, 0f, sizeY) / 2f;

      int[,] heightMap = new int[sizeX, sizeY];

      for(int y = 0; y < sizeY; y++) {
          for(int x = 0; x < sizeX; x++) {

            float height = heightCurveX.Evaluate(x / (float)sizeX) + heightCurveY.Evaluate(y / (float)sizeY);

              if ((x > 0 && x < sizeX - 1 && y > 0 && y < sizeY - 1) && height > waterLevel) {
                  int cellValue = Mathf.RoundToInt(height * 10f);
                  heightMap[x, y] = cellValue;
              } else {
                  heightMap[x, y] = -1;
              }
          }
      }

      Cell[,] cells = new Cell[sizeX, sizeY];
      for(int y = 0; y < sizeY; y++) {
          for(int x = 0; x < sizeX; x++) {
              int height = heightMap[x, y];
              if (height < 0) {
                  cells[x, y] = new Cell(true, 0);
              } else {
                  cells[x, y] = new Cell(false, height);
              }
          }
      }
      return cells;
    }

}