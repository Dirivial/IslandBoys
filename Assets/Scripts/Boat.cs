using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boat : MonoBehaviour
{
  public Transform playerTransform;

  [Header("Player")]
  [Tooltip("Move speed of the boat in m/s")]
  public float MoveSpeed = 8.0f;

  [Tooltip("How fast the character turns to face movement direction")]
  [Range(0.0f, 1.2f)]
  public float RotationSmoothTime = 0.4f;

  [Tooltip("Acceleration and deceleration")]
  public float SpeedChangeRate = 10.0f;

  public AudioClip BoatingAudioClip;

  // Update is called once per frame
  void Update()
  {

  }

  public void Spawn()
  {
    transform.position = new Vector3(playerTransform.position.x, 0, playerTransform.position.z);
  }

  public void DeSpawn()
  {
    transform.position = new Vector3(0, -100, 0);
  }
}
