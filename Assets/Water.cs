using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Water : MonoBehaviour
{
    public Material waterMaterial;
    public int size = 100;

    Cell[,] grid;

    void Start() {
        for(int y = 0; y < size; y++) {
            for(int x = 0; x < size; x++) {
                Cell cell = new Cell(true);
                grid[x, y] = cell;
            }
        }
    }
}
