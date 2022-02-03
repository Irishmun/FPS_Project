using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OptionsMenu : MonoBehaviour
{
    [SerializeField, Tooltip("Parent Object that holds the option UI Elements, not to be the object this script is on")]
    private Transform OptionsHolder;
    [SerializeField]
    private Animator animator;
    [SerializeField]
    private string OpenAnimation;
    [SerializeField]
    private string CloseAnimation;
    #region UIEvents
    //Things that the Ui elements do, buttons and stuff
    #endregion
    #region Animation Methods
    //methods to start the animations
    public void OpenOptions()
    {
        animator.Play(OpenAnimation);
    }
    public void CloseOptions()
    {
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
        }
    }

    public void Animation_HideOptions()
    {
        if (OptionsHolder)
        {
            OptionsHolder.gameObject.SetActive(false);
        }
    }
    #endregion
}
