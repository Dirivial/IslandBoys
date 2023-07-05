using UnityEngine;
using System.Collections.Generic;

public class Island : MonoBehaviour
{
    public Material terrainMaterial;
    public Material edgeMaterial;
    public GameObject waterPrefab;
    public GameObject groundPrefab;
    public float waterLevel = .4f;
    public float scale = .1f;

    private int sizeX = 0;
    private int sizeY = 0;
    private GridManager grid;

    private int largestDistance = 0;
    private float waterBuffer = 20f;

    void Start() {

        sizeX = GridManager.Instance.gridSizeX;
        sizeY = GridManager.Instance.gridSizeY;

        Vector3 centerPosition = new Vector3(sizeX, 0f, sizeY) / 2f;

        int[,] heightMap = new int[sizeX, sizeY];

        for(int y = 0; y < sizeY; y++) {
            for(int x = 0; x < sizeX; x++) {
                if (x > waterBuffer && x < sizeX - waterBuffer && y > waterBuffer && y < sizeY - waterBuffer) {
                    // Calculate the distance from the center for each cell
                    Vector3 cellPosition = new Vector3(x, 0f, y);
                    float distance = Vector3.Distance(cellPosition, centerPosition) / 10f;

                    // Set the value of the cell based on the distance from the center
                    // You can modify this calculation to suit your needs
                    int cellValue = Mathf.RoundToInt(distance);
                    heightMap[x, y] = cellValue;

                    if (cellValue > largestDistance) {
                        largestDistance = cellValue;
                    }
                } else {
                    heightMap[x, y] = -1;
                }
            }
        }

        grid = GridManager.Instance;
        for(int y = 0; y < sizeY; y++) {
            for(int x = 0; x < sizeX; x++) {
                int height = heightMap[x, y];
                if (height < 0) {
                    grid.SetGridCell(x, y, new Cell(true, largestDistance - height));
                } else {
                    grid.SetGridCell(x, y, new Cell(false, largestDistance - height));
                    GameObject ground = Instantiate(groundPrefab, new Vector3(x * 2, (largestDistance - heightMap[x, y]) * GridManager.Instance.cellSize, y * 2), Quaternion.identity);
                    ground.transform.parent = transform;
                }
            }
        }
    }


    // This code is used to generate the noise map and falloff map
    private void funkyStuff() {
        float[,] noiseMap = new float[sizeX, sizeY];

        (float xOffset, float yOffset) = (Random.Range(-10000f, 10000f), Random.Range(-10000f, 10000f));
        for(int y = 0; y < sizeY; y++) {
            for(int x = 0; x < sizeX; x++) {
                float noiseValue = Mathf.PerlinNoise(x * scale + xOffset, y * scale + yOffset);
                noiseMap[x, y] = noiseValue;
            }
        }

        float[,] falloffMap = new float[sizeX, sizeY];
        for(int y = 0; y < sizeY; y++) {
            for(int x = 0; x < sizeX; x++) {
                float xv = x / (float)sizeX * 2 - 1;
                float yv = y / (float)sizeY * 2 - 1;
                float v = Mathf.Max(Mathf.Abs(xv), Mathf.Abs(yv));
                falloffMap[x, y] = Mathf.Pow(v, 3f) / (Mathf.Pow(v, 3f) + Mathf.Pow(2.2f - 2.2f * v, 3f));
            }
        }

        grid = GridManager.Instance;
        for(int y = 0; y < sizeY; y++) {
            for(int x = 0; x < sizeX; x++) {
                float noiseValue = noiseMap[x, y];
                noiseValue -= falloffMap[x, y];
                if (noiseValue < waterLevel) {
                    grid.SetGridCell(x, y, new Cell(true));
                } else {
                    GameObject ground = Instantiate(groundPrefab, new Vector3(x * 2, 0, y * 2), Quaternion.identity);
                    ground.transform.parent = transform;
                    grid.SetGridCell(x, y, new Cell(false));
                }
            }
        }
    }
}