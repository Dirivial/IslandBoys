using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteract : MonoBehaviour
{

  public float interactDistance = 3f;

  private bool stoppedController = false;
  private Transform playerTransform;

  private StarterAssets.ThirdPersonController playerController;

  void Start()
  {
    playerTransform = GetComponent<Transform>();
    playerController = GetComponent<StarterAssets.ThirdPersonController>();
  }

  // Update is called once per frame
  void Update()
  {
    if (Input.GetKeyDown(KeyCode.E))
    {
      Collider[] hitColliders = Physics.OverlapSphere(transform.position, interactDistance);

      foreach (Collider hitCollider in hitColliders)
      {
        if (playerTransform.position.y < 0 && hitCollider.gameObject.tag == "Sea")
        {
          //   hitCollider.gameObject.GetComponent<Interactable>().Interact();
          if (stoppedController)
          {
            GetComponent<StarterAssets.ThirdPersonController>().enabled = true;
            stoppedController = false;
            break;
          }
          else
          {
            GetComponent<StarterAssets.ThirdPersonController>().enabled = false;
            stoppedController = true;
            break;
          }
        }
      }
    }
  }
}
