using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class SaveFile : MonoBehaviour
{
    public float CurrentMasterVolume;
    public int CurrentVsyncLevel;

    void Start()
    {
        LoadFile();
        SaveToFile();
    }

    public void SaveToFile()
    {
        string destination = Application.persistentDataPath + "/save.dat";
        FileStream file;

        if (File.Exists(destination))
        {
            file = File.OpenWrite(destination);
        }
        else
        {
            file = File.Create(destination);
        }

        GameData data = new GameData(CurrentMasterVolume, CurrentVsyncLevel);
        BinaryFormatter bf = new BinaryFormatter();
        bf.Serialize(file, data);
        file.Close();
    }

    public void LoadFile()
    {
        string destination = Application.persistentDataPath + "/save.dat";
        FileStream file;

        if (File.Exists(destination)) file = File.OpenRead(destination);
        else
        {
            Debug.Log("File not found, this might cause errors.");
            return;
        }

        BinaryFormatter bf = new BinaryFormatter();
        GameData data = (GameData)bf.Deserialize(file);
        file.Close();

        CurrentMasterVolume = data.MasterVolume;
        CurrentVsyncLevel = data.VsyncLevel;
    }

}

