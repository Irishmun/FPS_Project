using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController)), RequireComponent(typeof(AudioSource))]
public class WalkSound : MonoBehaviour
{

    [SerializeField, Tooltip("material lookup table")]
    private MaterialWalkScriptableObject MaterialsLookup;

    private CharacterController _Controller;
    private AudioSource _AudioSource;


}
