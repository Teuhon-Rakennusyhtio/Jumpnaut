using System;
using System.IO;
using UnityEngine;

public class SaverLoader
{
    const string SaveFileName = "Save.jumpnautsave";

    public static SaveFile ReadSaveFromBinary()
    {
        // Creates an empty save file
        SaveFile loadedSave = new();

        // Initialize objects for reading binaries
        FileStream fileStream;
        BinaryReader reader;
        
        // Gets the save file path
        string saveFilePath = GetSaveFilePath();
        
        // If there isn't a save file then return the empty save that was created
        if (!File.Exists(saveFilePath))
        {
            return loadedSave;
        }

        // Try to read the save file and return the empty save if that isn't possible
        try
        {
            fileStream = File.Open(saveFilePath, FileMode.Open, FileAccess.Read);
            reader = new(fileStream);
        }
        catch (Exception e)
        {
            Debug.LogError("Could not read the save!");
            Debug.LogException(e);
            return loadedSave;
        }

        // Read all the stuff
        loadedSave.HighScore = reader.ReadInt32();
        loadedSave.LowestTime = reader.ReadSingle();
        
        float checkPointXPos = reader.ReadSingle();
        float checkPointYPos = reader.ReadSingle();
        loadedSave.CurrentRunCheckPointPosition = new Vector2(checkPointXPos, checkPointYPos);

        loadedSave.CurrentRunScore = reader.ReadInt32();
        loadedSave.CurrentRunTime = reader.ReadSingle();

        // Close the objects
        reader.Close();
        fileStream.Close();

        return loadedSave;
    }

    public static bool WriteSaveToBinary(SaveFile savedSave)
    {
        // Initialize objects for writing binaries
        FileStream fileStream;
        BinaryWriter writer;

        // Gets the save file path
        string saveFilePath = GetSaveFilePath();

        // Try to create a save file directory if one does not exist yet and open the save
        try
        {
            string directory = Path.GetDirectoryName(saveFilePath);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
            
            fileStream = File.Open(saveFilePath, FileMode.Create, FileAccess.Write);
            writer = new(fileStream);
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            return false;
        }

        // Write all the stuff to the save file
        writer.Write(savedSave.HighScore);
        writer.Write(savedSave.LowestTime);

        writer.Write(savedSave.CurrentRunCheckPointPosition.x);
        writer.Write(savedSave.CurrentRunCheckPointPosition.y);

        writer.Write(savedSave.CurrentRunScore);
        writer.Write(savedSave.CurrentRunTime);

        // Close the objects
        writer.Close();
        fileStream.Close();

        return true;
    }
    static string GetSaveFilePath()
    {
        string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        return Path.Combine(documentsPath, "Jumpnaut", "Save", SaveFileName);;
    }
}


public struct SaveFile
{
    public int HighScore;
    public float LowestTime;
    public Vector2 CurrentRunCheckPointPosition;
    public int CurrentRunScore;
    public float CurrentRunTime;
    public SaveFile(bool UnityPlzUpdateCSSoIDontHaveToPutThisPointlessParameterHere = true)
    {
        HighScore = -1;
        LowestTime = -1f;
        CurrentRunCheckPointPosition = Vector2.negativeInfinity;
        CurrentRunScore = 0;
        CurrentRunTime = 0f;
    }
}
