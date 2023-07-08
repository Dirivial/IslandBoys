using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Boat : MonoBehaviour
{

  public Transform playerTransform;

  private bool isSpawned = false;

  //   Update is called once per frame
  void Update()
  {
    if (isSpawned)
    {
      transform.position = new Vector3(playerTransform.position.x, 0f, playerTransform.position.z);
      transform.rotation = playerTransform.rotation;
    }
  }

  public void Spawn() { isSpawned = true; }
  public void DeSpawn() { isSpawned = false; }

}


