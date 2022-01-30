using InternalRealtimeCSG;
using RealtimeCSG.Components;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Created: 03-19-2019
/// Updated:
/// 
/// CoughE
/// 
/// Auto adds a mesh collider to each render mesh created via CSG on build or pressing play. I do this instead of 
/// just relying on the pregenerated mesh collider as that one creates a collider for all materials,
/// but I need to be able to get the material associated with a single collider for my MaterialEffects system,
/// to do things such as play footsteps on walking over a certain stone material for instance.
/// 
/// Updated June 16, 2020 by Kolton (Janooba)
/// Replaced build process with scene processor to correctly process all scenes on build.
/// There may still be issues with swapping scenes during editor time. Looking into it
/// </summary>
class CSGAutoColliderAddBuildProcessor : IProcessSceneWithReport
{
    public static bool enabled = true;
    public int callbackOrder { get { return 0; } }

    // This method is called before any Awake. It's the perfect callback for this feature
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void RunOnPressPlay()
    {
        if (!enabled)
            return;
        SetMeshes();
    }

    static void SetMeshes()
    {
        var models = GameObject.FindObjectsOfType<CSGModel>();

        foreach (var model in models)
        {
            if (!model.HaveCollider || model.IsTrigger) //Do not apply mesh colliders to Models that are set to not have a collider in the first place
                continue;
            if (!PrefabUtility.GetCorrespondingObjectFromSource(model)) //Do not remove stuff from prefabs
            {
                //I don't think there is a direct link to the generated collider so just get it by name
                var baseCollider = model.transform.FindDeepChild("[generated-collider-mesh]");
                if (baseCollider)
                    SafeDestroy(baseCollider.gameObject);

                var renderers = model.GetComponentsInChildren<GeneratedMeshInstance>();
                foreach (var rend in renderers)
                {
                    //Remove colliders that may have been attached before
                    var existingCollider = rend.GetComponent<MeshCollider>();
                    if (existingCollider)
                        SafeDestroy(existingCollider);
                    // If this renderer is set to have collision
                    if (rend.RenderSurfaceType == RenderSurfaceType.Normal || rend.RenderSurfaceType.HasFlag(RenderSurfaceType.Collider))
                    {
                        rend.gameObject.AddComponent<MeshCollider>();
                    }
                }
            }

        }
        Debug.Log($"Set Mesh Colliders on {models.Length} CSG Models.");
    }

    /// <summary>
    /// Run this whenever you build the game
    /// </summary>
    /// <param name="scene"></param>
    /// <param name="report"></param>
    public void OnProcessScene(Scene scene, BuildReport report)
    {
        if (!enabled)
            return;
        SetMeshes();
    }

    /// <summary>
    /// Used to destroy objects safely in editor
    /// </summary>
    /// <param name="obj"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T SafeDestroy<T>(T obj) where T : Object
    {
        if (Application.isPlaying)
            Object.Destroy(obj);
        else
            Object.DestroyImmediate(obj, false);

        return null;
    }


}