using System;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "PlayerInputReader", menuName = "Scriptable Objects/PlayerInputReader")]
public class PlayerInputReader : ScriptableObject, InputSystem_Actions.IPlayerActions
{
    public Action<Vector2> OnMove;
    public Action<BulletType> OnAttack;
    private InputSystem_Actions _inputActions;
    
    private void OnEnable()
    {
        if (_inputActions == null)
        {
            _inputActions = new InputSystem_Actions();
            _inputActions.Player.SetCallbacks(this);
        }
        _inputActions.Player.Enable(); 
    }

    private void OnDisable()
    {
        _inputActions?.Player.Disable(); 
    }
    public void OnA(InputAction.CallbackContext context)
    {
        if (context.performed) OnAttack?.Invoke(BulletType.A);
    }

    public void OnB(InputAction.CallbackContext context)
    {
        if (context.performed) OnAttack?.Invoke(BulletType.B);
    }

    public void OnUp(InputAction.CallbackContext context)
    {
        if (context.performed) OnMove?.Invoke(Vector2.down);
    }

    public void OnDown(InputAction.CallbackContext context)
    {
        if (context.performed) OnMove?.Invoke(Vector2.up);
    }

    public void OnLeft(InputAction.CallbackContext context)
    {
        if (context.performed) OnMove?.Invoke(Vector2.left);
    }

    public void OnRight(InputAction.CallbackContext context)
    {
        if (context.performed) OnMove?.Invoke(Vector2.right);
    }
}
