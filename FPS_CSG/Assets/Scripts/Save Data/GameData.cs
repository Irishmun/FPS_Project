using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameData
{
    public float MasterVolume;
    public int VsyncLevel;

    public GameData(float MasterVolume, int VsyncLevel)
    {
        this.MasterVolume = MasterVolume;
        this.VsyncLevel = VsyncLevel;
    }
    /// <summary>
    /// Writes to all the GameData Values
    /// </summary>
    /// <param name="masterVolume">The master volume value, in db (-80db to 20db)</param>
    /// <param name="vsyncLevel">The v-sync level. 0 is off, 1 is every v-blank, 2 every other, etc.</param>
    public void WriteData(float masterVolume, int vsyncLevel)
    {
        this.MasterVolume = masterVolume;
        this.VsyncLevel = vsyncLevel;
    }
}
