using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class ActionManager : MonoBehaviour
{
    public UnityEvent<Vector2> move = new UnityEvent<Vector2>();
    public UnityEvent dash = new UnityEvent();
    public UnityEvent shoo = new UnityEvent();

    public void OnMoveAction(InputAction.CallbackContext context)
    {
        if (context.performed)
            move.Invoke(context.ReadValue<Vector2>());
        else if (context.canceled)
            move.Invoke(Vector2.zero);
    }

    public void OnDashAction(InputAction.CallbackContext context)
    {
        if (context.performed)
            dash.Invoke();
    }

    public void OnShooAction(InputAction.CallbackContext context)
    {
        if (context.performed)
            shoo.Invoke();
    }

    private void OnDisable()
    {
        move.Invoke(Vector2.zero);
    }
}
