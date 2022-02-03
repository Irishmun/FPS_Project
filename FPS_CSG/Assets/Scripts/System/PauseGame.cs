using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PauseGame : MonoBehaviour
{
    [SerializeField]
    private PlayerInput Player;
    [SerializeField]
    private Transform PauseMenuUI;
    private bool _PauseState;
    private float _prevTimeScale = 1f;
    #region events
    public void OnPauseGame(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            if (_PauseState == false)
            {
                //Pause
                Player.currentActionMap.Disable();
                Player.SwitchCurrentActionMap("UI");
                Player.currentActionMap.Enable();
                PauseMenuUI.gameObject.SetActive(true);
                _prevTimeScale = Time.timeScale;
                Time.timeScale = 0f;
                _PauseState = true;
            }
        }
    }

    public void OnResumeGame(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            if (_PauseState == true)
            {
                //UnPause
                Player.currentActionMap.Disable();
                Player.SwitchCurrentActionMap("Player");
                Player.currentActionMap.Enable();
                PauseMenuUI.gameObject.SetActive(false);
                Time.timeScale = _prevTimeScale;
                _PauseState = false;
            }
        }
    }
    public void ResumeGame()
    {
        if (_PauseState == true)
        {
            //UnPause
            Player.currentActionMap.Disable();
            Player.SwitchCurrentActionMap("Player");
            Player.currentActionMap.Enable();
            PauseMenuUI.gameObject.SetActive(false);
            Time.timeScale = _prevTimeScale;
            _PauseState = false;
        }
    }
    #endregion
}
