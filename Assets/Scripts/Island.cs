using UnityEngine;
using System.Collections.Generic;

public class Island : MonoBehaviour
{
    public Material terrainMaterial;
    public Material edgeMaterial;
    public GameObject groundPrefab;
    public GameObject edgePrefab;
    public AnimationCurve heightCurveX;
    public AnimationCurve heightCurveY;
    public float waterLevel = .4f;

    public int sizeX = 100;
    public int sizeY = 100;
    private Cell[,] grid;

    private void Start() {

        grid = IslandGenerator.GenerateIslandFromAnimationCurve(sizeX, sizeY, heightCurveX, heightCurveY, waterLevel);

        for (int y = 0; y < sizeY; y++) {
            for(int x = 0; x < sizeX; x++) {
                Cell cell = grid[x, y];
                if (!cell.isWater) {
                    GameObject ground = Instantiate(groundPrefab, new Vector3(x * 2, cell.height * 2, y * 2), Quaternion.identity);
                    ground.transform.parent = transform;
                } else {
                    // Check if a cell is an edge cell
                    for (int i = -1; i < 2; i+= 2) {
                        if (x + i > 0 && x + i < sizeX && y + i > 0 && y + i < sizeY) {
                            if (!grid[x + i,y].isWater || !grid[x,y+i].isWater) {
                                SpawnEdgeCell(x, y);
                                break;
                            }
                        }
                    }
                }
            }
        }
    }

    private void SpawnEdgeCell(int x, int y) {
        GameObject edge = Instantiate(edgePrefab, new Vector3(x * 2, 25f, y * 2), Quaternion.identity);
        edge.transform.parent = transform;
    }
}