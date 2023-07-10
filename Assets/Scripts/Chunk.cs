using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk : MonoBehaviour
{
    private GameObject[,] cells;
    private int size;
    private int x;
    private int z;
    private ChunkType type;

    public void SetChunk(int x, int z, int size, ChunkType type)
    {
        Debug.Log(x + ", " + z + ", " + size + ", " + type);
        this.type = type;
        this.x = x;
        this.z = z;

        // Note that 16 is the size of my water/sea prefab
        if (type == ChunkType.Sea && size % 16 == 0)
        {
            this.size = size / 16;
        }
        else
        {
            this.size = size;

        }
        this.cells = new GameObject[this.size, this.size];
    }

    public void InstantiateChunk(GameObject gameObject)
    {
        //Debug.Log("Instantiating chunk at " + x + ", " + z);
        // Generate the chunk
        // Instantiate the chunk
        // Add the chunk to the dictionary

        if (type == ChunkType.Sea)
        {
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    // Instantiate the block
                    // Set the block's position
                    // Set the block's parent
                    GameObject block = Instantiate(gameObject, new Vector3((i * 16 + x + 8), 0, (j * 16 + z + 8)), Quaternion.identity);
                    block.transform.parent = this.transform;
                    cells[i, j] = block;
                }
            }
            return;
        }
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                // Instantiate the block
                // Set the block's position
                // Set the block's parent
                GameObject block = Instantiate(gameObject, new Vector3(i + x, 0, j + z), Quaternion.identity);
                block.transform.parent = this.transform;
                cells[i, j] = block;
            }
        }

    }

    public void DestroyChunk()
    {
        //Debug.Log("Destroying chunk at " + x + ", " + z);
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                Destroy(cells[i, j]);
            }
        }
    }

    public ChunkType GetChunkType()
    {
        return type;
    }
}
