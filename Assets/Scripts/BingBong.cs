using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BingBong : MonoBehaviour
{
    private ChunkGeneratorWFC ChunkGeneratorWFC;

    private void Awake()
    {
        ChunkGeneratorWFC = GetComponent<ChunkGeneratorWFC>();
    }

    public void TakeStep()
    {
        ChunkGeneratorWFC.TakeStep();
    }
}
