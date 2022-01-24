using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialSoundBase : ScriptableObject
{
    /// <summary>
    /// Returns a random audio clip from the passed array if it has any.
    /// </summary>
    /// <param name="sounds">The <see cref="AudioClip"/> array to choose a sound from.</param>
    /// <returns>A random audio clip from the passed array if it has any</returns>
    protected AudioClip GetRandomClip(AudioClip[] sounds)
    {
        if (sounds != null)
        {
            if (sounds.Length == 1)
            {
                return sounds[0];
            }
            if (sounds.Length > 0)
            {
                return sounds[Random.Range(0, sounds.Length)];
            }
        }
        return null;
    }
}
