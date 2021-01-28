using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WorldController : MonoBehaviour
{
    public static WorldController Instance { get; protected set; }
    public static World World { get; protected set; }

    static bool loadWorld = false;

    [Range(1, 200)]
    public int worldWidth = 100;

    [Range(1, 200)]
    public int worldHeight = 100;


    // Start is called before the first frame update
    void OnEnable()
    {
        if (Instance != null)
        {
            Debug.LogError("Duplicate world controller");
        }
        Instance = this;
        if (loadWorld)
        {
            loadWorld = false;
            CreateWorldFromSaveFile();
        }
        else
        {
            CreateEmptyWorld();
        }
    }

    void Update()
    {
        //TODO: add pause, unpause, speed controls
        //essentially change what kind of delta time is being passed here
        World.Update(Time.deltaTime);
    }

    public Tile GetTileAtWorldCoord(Vector3 coord)
    {
        int x = Mathf.RoundToInt(coord.x);
        int y = Mathf.RoundToInt(coord.y);

        return World.GetTileAt(x, y);
    }    

    public void OnNewWorld()
    {
        Debug.Log("User reloaded scene");
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void SaveWorld()
    {
        Debug.Log("User saved world");
        XmlSerializer serializer = new XmlSerializer(typeof(World));
        TextWriter writer = new StringWriter();
        serializer.Serialize(writer, World);
        writer.Close();

        //DEBUG
        //Debug.Log(writer.ToString());

        PlayerPrefs.SetString("SaveGame00", writer.ToString());
    }

    public void LoadWorld()
    {
        loadWorld = true;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void CreateEmptyWorld()
    {
        World = new World(worldWidth, worldHeight);

        //center camera
        Camera.main.transform.position = new Vector3(World.Width / 2, World.Height / 2, Camera.main.transform.position.z);
    }

    void CreateWorldFromSaveFile()
    {
        //create world from save file data (currently in playerprefs)
        Debug.Log("User loaded world");
        XmlSerializer serializer = new XmlSerializer(typeof(World));
        TextReader reader = new StringReader(PlayerPrefs.GetString("SaveGame00"));
        World = (World)serializer.Deserialize(reader);
        reader.Close();

        //center camera
        Camera.main.transform.position = new Vector3(World.Width / 2, World.Height / 2, Camera.main.transform.position.z);
    }
}
