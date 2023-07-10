using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct WFCPair
{
  public GameObject prefab;
  public float height;
}

[System.Serializable]
public struct WFCSet
{
  public List<WFCPair> prefabs;
  public ChunkType chunkType;
}

public class ChunkGeneratorWFC : MonoBehaviour
{

  [SerializeField] private List<WFCSet> wfcSets;

  // Start is called before the first frame update
  void Start()
  {

  }

  // Update is called once per frame
  void Update()
  {

  }
}
