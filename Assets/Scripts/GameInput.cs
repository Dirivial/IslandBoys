using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameInput : MonoBehaviour
{

  private PlayerInput playerInput;

  // Start is called before the first frame update
  void Awake()
  {
    playerInput = new PlayerInput();
    playerInput.Player.Enable();
  }

  public Vector2 GetMovementInputNormalized()
  {
    return playerInput.Player.Move.ReadValue<Vector2>().normalized;
  }
  public Vector2 GetMovementInput()
  {
    return playerInput.Player.Move.ReadValue<Vector2>();
  }

  public bool GetJumpInput()
  {
    return playerInput.Player.Jump.triggered;
  }

  public bool GetInteractInput()
  {
    return playerInput.Player.Interact.triggered;
  }

  public bool GetSprintInput()
  {
    return playerInput.Player.Sprint.ReadValue<float>() == 1 ? true : false;
  }

  public Vector2 GetLookInput()
  {
    return playerInput.Player.Look.ReadValue<Vector2>();
  }
}
