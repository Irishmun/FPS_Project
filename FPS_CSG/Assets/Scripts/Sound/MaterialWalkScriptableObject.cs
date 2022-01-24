using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "new WalkSound", menuName = "ScriptableObjects/WalkSound", order = 2)]
public class MaterialWalkScriptableObject : MaterialSoundBase
{
    [SerializeField, Tooltip("Sound(s) to play when the player walks over material.")]
    private MaterialWalkSounds[] WalkSounds;

    [System.Serializable]
    public class MaterialWalkSounds
    {
        public Material material;
        public AudioClip[] sounds;
    }
    private Dictionary<Material, AudioClip[]> SoundsLookup = new Dictionary<Material, AudioClip[]>();

    public void Init()
    {
        foreach (MaterialWalkSounds mws in WalkSounds)
        {
            SoundsLookup.Add(mws.material, mws.sounds);
        }
    }

    public void PlayWalkSound(Material walkedMaterial, AudioSource source)
    {
        if (source.isActiveAndEnabled)
        {
            if (!source.isPlaying)
            {
                source.clip = GetRandomClip(SoundsLookup[walkedMaterial]);

                if (source.clip != null)
                {
                    source.Play();
                }
            }
        }
    }
}
