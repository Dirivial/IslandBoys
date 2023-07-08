using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class PlayerInteract : MonoBehaviour
{

  public float interactDistance = 3f;
  public GameObject boatObject;
  //   private bool stoppedController = false;
  private Transform playerTransform;


  private bool isOnBoat = false;

  private StarterAssets.ThirdPersonController playerController;
  private Boat boat;

  void Start()
  {
    playerTransform = GetComponent<Transform>();
    playerController = GetComponent<StarterAssets.ThirdPersonController>();
    boat = boatObject.GetComponent<Boat>();
    if (playerController.isOnBoat)
    {
      isOnBoat = true;
      boat.Spawn();
    }
  }

  // Update is called once per frame
  void Update()
  {
    if (isOnBoat && Input.GetKeyDown(KeyCode.F))
    {
      Debug.Log("Player wants to leave island");

      SceneManager.LoadScene("Exploration");
    }
    if (Input.GetKeyDown(KeyCode.E))
    {
      if (isOnBoat)
      {
        isOnBoat = false;
        playerController.ToggleOnBoat();
        boat.DeSpawn();
      }
      else
      {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, interactDistance);

        foreach (Collider hitCollider in hitColliders)
        {
          if (playerTransform.position.y < 0 && hitCollider.gameObject.tag == "Sea")
          {
            //   hitCollider.gameObject.GetComponent<Interactable>().Interact();
            if (!isOnBoat)
            {
              playerController.ToggleOnBoat();
              isOnBoat = true;
              boat.Spawn();
              break;
            }
          }
        }
      }
    }
  }
}
