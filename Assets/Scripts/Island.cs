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

    void Start() {

        sizeX = GridManager.Instance.gridSizeX;
        sizeY = GridManager.Instance.gridSizeY;

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
                    //GameObject water = Instantiate(waterPrefab, new Vector3(x * 2, 0, y * 2), Quaternion.identity);
                    //water.transform.parent = transform;
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