using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class DebugSceneLoading
{
    // Start is called before the first frame update
    public static readonly string prefsKey = "PreLoadScene";
    static DebugSceneLoading()
    {
        EditorApplication.playModeStateChanged += LogPlayModeState;
    }

    private static void LogPlayModeState(PlayModeStateChange state)
    {
        if (PlayerPrefs.HasKey(prefsKey))
        {
            //load scene stored in said playerprefs
            string path = PlayerPrefs.GetString(prefsKey);
            if (!string.IsNullOrEmpty(path))
            {
                //Debug.Log(path);
                EditorSceneManager.LoadSceneInPlayMode(path, new LoadSceneParameters(LoadSceneMode.Additive));
            }
        }
    }
}
