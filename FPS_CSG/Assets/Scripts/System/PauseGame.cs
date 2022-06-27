using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseGame : MonoBehaviour
{
    [SerializeField]
    private UnityEngine.EventSystems.EventSystem UIEventSystem;
    [SerializeField, Tooltip("First option selected in options menu")]
    private GameObject PauseFirstSelected;
    [SerializeField]
    private PlayerInput Player;
    [SerializeField]
    private Transform PauseMenuUI;
    private bool _PauseState;
    private float _PrevTimeScale = 1f;

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (mode == LoadSceneMode.Additive)// would mean that this scene is added onto the currently active scene
        {
            if (Player == null)
            {
                Player = FindObjectOfType<PlayerInput>();
                if (Player == null)//couldn't find any PlayerInput
                {
                    Debug.Log("No Player found, disabling...");
                    gameObject.SetActive(false);
                }
            }
        }
    }

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
                _PrevTimeScale = Time.timeScale;
                Time.timeScale = 0f;
                if (Player.currentControlScheme.Equals("Keyboard&Mouse"))
                {
                    Cursor.lockState = CursorLockMode.Confined;
                }
                else
                {
                    Debug.Log("Not keyboard & mouse");
                    UIEventSystem.SetSelectedGameObject(PauseFirstSelected);
                }
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
                Cursor.lockState = CursorLockMode.Locked;
                Player.currentActionMap.Disable();
                Player.SwitchCurrentActionMap("Player");
                Player.currentActionMap.Enable();
                PauseMenuUI.gameObject.SetActive(false);
                Time.timeScale = _PrevTimeScale;
                _PauseState = false;
            }
        }
    }
    public void ResumeGame()
    {
        if (_PauseState == true)
        {
            //UnPause
            StartCoroutine(LockMouseNextFrame());
            Player.currentActionMap.Disable();
            Player.SwitchCurrentActionMap("Player");
            Player.currentActionMap.Enable();
            PauseMenuUI.gameObject.SetActive(false);
            Time.timeScale = _PrevTimeScale;
            _PauseState = false;
        }
    }

    public void RestartScene()
    {
        UnityEngine.SceneManagement.Scene scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        Time.timeScale = _PrevTimeScale;
        UnityEngine.SceneManagement.SceneManager.LoadScene(scene.name);
    }

    public void QuitGame()
    {
#if !UNITY_EDITOR
        Application.Quit();
#elif UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
    #endregion

    private IEnumerator LockMouseNextFrame()
    {
        yield return new WaitForEndOfFrame();
        Cursor.lockState = CursorLockMode.Locked;
    }
}
