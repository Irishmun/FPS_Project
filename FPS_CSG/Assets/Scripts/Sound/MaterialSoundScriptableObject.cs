using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "new MaterialSound", menuName = "ScriptableObjects/MaterialSound", order = 1)]
public class MaterialSoundScriptableObject : ScriptableObject
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

    /// <summary>
    /// Returns a random audio clip from the passed array if it has any.
    /// </summary>
    /// <param name="sounds">The <see cref="AudioClip"/> array to choose a sound from.</param>
    /// <returns>A random audio clip from the passed array if it has any</returns>
    private AudioClip GetRandomClip(AudioClip[] sounds)
    {
        if (sounds.Length > 0)
        {
            if (sounds.Length == 1)
            {
                return sounds[0];
            }
            int val = Random.Range(0, sounds.Length);
            //Debug.Log(val);
            return sounds[val];
        }
        return null;
    }
}
