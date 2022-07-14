using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CrossScenePauseEvent : MonoBehaviour
{
    public static event EventHandler OnPauseRequestEvent;

    public void OnPauseGame(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            OnPauseRequestEvent.Invoke(this, null);
        }
    }
}
