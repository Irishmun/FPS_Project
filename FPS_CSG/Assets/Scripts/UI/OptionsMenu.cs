using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using TMPro;
/*
Volume X
Vsync X
FullScreen
Resolution (maybe)
Keybinds (maybe)

Max Framerate (DEBUG)

Apply X

Return X
*/
public class OptionsMenu : MonoBehaviour
{
    [Header("Accessible Menus")]
    [SerializeField, Tooltip("Parent Object that holds the option UI Elements, not to be the object this script is on")]
    private Transform OptionsHolder;
    [SerializeField, Tooltip("Regular Pause menu transform")]
    private Transform PauseMenu;
    [Header("Menu selection")]
    [SerializeField]
    private UnityEngine.EventSystems.EventSystem UIEventSystem;
    [SerializeField, Tooltip("First option selected in options menu")]
    private GameObject OptionsFirstSelected;
    [SerializeField, Tooltip("Options menu button in pause menu screen")]
    private GameObject PauseMenuReturnedSelected;
    [Header("Option Values")]
    [SerializeField]
    private SaveFile Save;
    [SerializeField]
    private AudioMixer MasterVolume;
    [Header("Option Changers")]
    [SerializeField]
    private Slider VolumeSlider; //TODO: add deepfried toggle, increases volume by 80 db
    [SerializeField]
    private TextMeshProUGUI VolumeLevel;
    [SerializeField]
    private Toggle VsyncToggle;
    [SerializeField]
    private Slider FPSSlider;
    [SerializeField]
    private TextMeshProUGUI FPSLevel; //-1 is no limit
    [Header("Animation")]
    [SerializeField]
    private Animator animator;
    [SerializeField]
    private string OpenAnimation;
    [SerializeField]
    private string CloseAnimation;

    private bool _VSyncState, _PrevVSyncState;
    private float _Volume, _PrevVolume;
    private int _FPS, _PrevFPS;
    #region UIEvents
    //Things that the Ui elements do, buttons and stuff
    public void OnVsyncToggle(bool Value)
    {
        //change vsync value boolean
        _VSyncState = VsyncToggle.isOn;
    }

    public void OnVolumeSlide(float value)
    {
        //change volume value in dB (-80 to 20)
        int vol = (int)value - 80;
        _Volume = vol;
        VolumeLevel.text = value.ToString();
    }

    public void OnFPSSlide(float value)
    {
        int fps = (int)value;
        _FPS = fps;
        FPSLevel.text = fps.ToString();
    }

    public void OnFPSForceReset()
    {
        _FPS = -1;
        FPSLevel.text = _FPS.ToString();
        ApplySettings();
    }

    public void ApplySettings()
    {
        //apply the settings 
        _PrevVolume = _Volume;
        _PrevVSyncState = _VSyncState;
        _PrevFPS = _FPS;
        //set volume
        MasterVolume.SetFloat("MasterVol", _Volume);
        QualitySettings.vSyncCount = _VSyncState ? 1 : 0;
        MasterVolume.GetFloat("MasterVol", out float vol);
        Application.targetFrameRate = _FPS;
        Save.CurrentMasterVolume = _Volume;
        Save.CurrentVsyncLevel = _VSyncState ? 1 : 0;
        Save.CurrentMaxFPS = _FPS;
        Save.SaveToFile();
        Debug.Log("Saved: " + Save.ToString());
    }

    public void ResetSettings()
    {
        //set all the values to the _Prev values
        _Volume = _PrevVolume;
        _VSyncState = _PrevVSyncState;
        _FPS = _PrevFPS;
        ApplySettings();
    }
    public void GetSettings()
    {
        //get the current settings and apply them to the ui and prev values
        Save.LoadFile();
        Debug.Log("Loaded: " + Save.ToString());
        //get and apply volume settings
        VolumeSlider.value = Save.CurrentMasterVolume + 80;
        VolumeLevel.text = VolumeSlider.value.ToString();
        _PrevVolume = Save.CurrentMasterVolume;
        _Volume = _PrevVolume;
        //get and apply vsync settings
        VsyncToggle.isOn = Save.CurrentVsyncLevel == 0 ? false : true;
        _PrevVSyncState = VsyncToggle.isOn;
        _VSyncState = _PrevVSyncState;
        //get and apply fps settings
        FPSSlider.value = Save.CurrentMaxFPS;
        FPSLevel.text = FPSSlider.value.ToString();
        _PrevFPS = Save.CurrentMaxFPS;
        _FPS = _PrevFPS;
    }
    #endregion
    #region Animation Methods
    //methods to start the animations
    public void OpenOptions()
    {
        animator.Play(OpenAnimation);
    }
    public void CloseOptions()
    {
        ResetSettings();
        animator.Play(CloseAnimation);
    }
    #endregion
    #region AnimationEvents
    //events that happen in the animations
    public void Animation_ShowOptions()
    {
        if (OptionsHolder)
        {
            OptionsHolder.gameObject.SetActive(true);
            GetSettings();
            UIEventSystem.SetSelectedGameObject(OptionsFirstSelected);
        }
    }

    public void Animation_HideOptions()
    {
        if (OptionsHolder)
        {
            OptionsHolder.gameObject.SetActive(false);
        }
    }

    public void Animation_ShowPauseMenuAndHideThis()
    {
        if (PauseMenu)
        {
            PauseMenu.gameObject.SetActive(true);
            UIEventSystem.SetSelectedGameObject(PauseMenuReturnedSelected);
            gameObject.SetActive(false);
        }
    }
    #endregion
}
