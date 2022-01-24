using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "new MaterialSound", menuName = "ScriptableObjects/CollisionSound", order = 1)]
public class MaterialSoundScriptableObject : MaterialSoundBase
{
    [SerializeField, Tooltip("Sound(s) to play when the object collides at low speed.")]
    private AudioClip[] SoftCollisionSounds;
    [SerializeField, Tooltip("Sound(s) to play when the object collides at high speed.")]
    private AudioClip[] HardCollisionSounds;
    [SerializeField, Tooltip("Sound(s) to play when the object is destroyed.")]
    private AudioClip[] DestructionSounds;

    public enum CollisionSpeed
    {
        /// <summary>
        /// when the object hits something at low speed
        /// </summary>
        Soft,
        /// <summary>
        /// when the object hits something at high speed
        /// </summary>
        Hard,
        /// <summary>
        /// when the object would be destroyed
        /// </summary>
        Destructive
    }

    public void PlayCollisionSound(CollisionSpeed speed, AudioSource source)
    {
        if (source.isActiveAndEnabled)
        {
            if (!source.isPlaying)
            {
                //TODO: play sound from other array if the requested one is empty
                switch (speed)
                {
                    case CollisionSpeed.Soft:
                        source.clip = GetRandomClip(SoftCollisionSounds);
                        break;
                    case CollisionSpeed.Hard:
                        source.clip = GetRandomClip(HardCollisionSounds);
                        break;
                    case CollisionSpeed.Destructive:
                        source.clip = GetRandomClip(DestructionSounds);
                        break;
                    default:
                        source.clip = null;
                        break;
                }

                if (source.clip != null)
                {
                    source.Play();
                }
            }
        }
    }
}
