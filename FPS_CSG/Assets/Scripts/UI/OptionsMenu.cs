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
    [Header("Animation")]
    [SerializeField]
    private Animator animator;
    [SerializeField]
    private string OpenAnimation;
    [SerializeField]
    private string CloseAnimation;

    private bool _VSyncState, _PrevVSyncState;
    private float _Volume, _PrevVolume;
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

    public void ApplySettings()
    {
        //apply the settings 
        _PrevVolume = _Volume;
        _PrevVSyncState = _VSyncState;
        MasterVolume.SetFloat("MasterVol", _Volume);
        QualitySettings.vSyncCount = _VSyncState ? 1 : 0;
        MasterVolume.GetFloat("MasterVol", out float vol);
        Debug.Log($"Volume: {vol}db, Vsync: {QualitySettings.vSyncCount}");
        Save.CurrentMasterVolume = _Volume;
        Save.CurrentVsyncLevel = _VSyncState ? 1 : 0;
        Save.SaveToFile();
    }

    public void ResetSettings()
    {
        //set all the values to the _Prev values
        _Volume = _PrevVolume;
        _VSyncState = _PrevVSyncState;
        ApplySettings();
    }
    public void GetSettings()
    {
        //get the current settings and apply them to the ui and prev values
        Save.LoadFile();
        VolumeSlider.value = Save.CurrentMasterVolume + 80;
        VolumeLevel.text = VolumeSlider.value.ToString();
        _PrevVolume = Save.CurrentMasterVolume;
        _Volume = _PrevVolume;

        VsyncToggle.isOn = Save.CurrentVsyncLevel == 0 ? false : true;
        _PrevVSyncState = VsyncToggle.isOn;
        _VSyncState = _PrevVSyncState;
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
            gameObject.SetActive(false);
        }
    }
    #endregion
}
