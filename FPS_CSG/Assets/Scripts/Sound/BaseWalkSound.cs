using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseWalkSound : MonoBehaviour
{

    [SerializeField, Tooltip("material lookup table")]
    private MaterialWalkScriptableObject MaterialsLookup;

    protected MaterialWalkScriptableObject _MaterialsLookup { get { return MaterialsLookup; } }

    public abstract void PlayMaterialWalkSound(Camera viewCamera, float RaycastHeight, AudioSource audio, int layerMask);

}
