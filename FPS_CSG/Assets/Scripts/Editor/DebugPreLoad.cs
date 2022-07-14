using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DebugPreLoad : EditorWindow
{
    static string scenePath;
    static SceneAsset scene;

    void OnGUI()
    {
        //EditorSceneManager.playModeStartScene = (SceneAsset)EditorGUILayout.ObjectField(new GUIContent("Start Scene"
        // Use the Object Picker to select the start SceneA), EditorSceneManager.playModeStartScene, typeof(SceneAsset), false);
        Debug.Log("Scene: " + scene.name);
        scene = (SceneAsset)EditorGUILayout.ObjectField(new GUIContent("Start Scene"), scene, typeof(SceneAsset), false);

        if (GUILayout.Button("Save"))
        {
            SetPlayModeStartScene();
        }
    }

    void SetPlayModeStartScene()
    {
        //try get path of SceneAsset
        scenePath = AssetDatabase.GetAssetOrScenePath(scene);
        if (string.IsNullOrEmpty(scenePath))
        {
            Debug.LogError("Unable to find scene at: \"" + scenePath + "\"");
        }
        else
        {
            //store path of SceneAsset in prefs
            PlayerPrefs.SetString(DebugSceneLoading.prefsKey, scenePath);
            Debug.Log("Saved scene to prefs as: \"" + scenePath + "\".");
        }
    }

    [MenuItem("Debug/Pre Load Scene")]
    static void Open()
    {
        if (PlayerPrefs.HasKey(DebugSceneLoading.prefsKey))
        {
            scenePath = PlayerPrefs.GetString(DebugSceneLoading.prefsKey);
            scene = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);
            if (scene == null)
            {
                Debug.LogError("Unable to find scene at: \"" + scenePath + "\"");
            }
        }
        else
        {
            Debug.Log("No \"" + DebugSceneLoading.prefsKey + "\" key in PlayerPrefs, creating...");
            PlayerPrefs.SetString(DebugSceneLoading.prefsKey, string.Empty);
        }
        GetWindow<DebugPreLoad>();

    }
}