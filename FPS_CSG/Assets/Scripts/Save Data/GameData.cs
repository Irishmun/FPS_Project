using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameData
{
    public float MasterVolume { get; private set; }
    public int VsyncLevel { get; private set; }
    public int MaxFPS { get; private set; }

    public GameData(float MasterVolume, int VsyncLevel, int MaxFPS)
    {
        this.MasterVolume = MasterVolume;
        this.VsyncLevel = VsyncLevel;
        this.MaxFPS = MaxFPS;
    }
    /// <summary>
    /// Writes to all the GameData Values
    /// </summary>
    /// <param name="masterVolume">The master volume value, in db (-80db to 20db)</param>
    /// <param name="vsyncLevel">The v-sync level. 0 is off, 1 is every v-blank, 2 every other, etc.</param>
    public void WriteData(float masterVolume, int vsyncLevel, int MaxFPS)
    {
        this.MasterVolume = masterVolume;
        this.VsyncLevel = vsyncLevel;
        this.MaxFPS = MaxFPS;
    }
}
