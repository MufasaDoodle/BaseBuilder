using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldController : MonoBehaviour
{
    public static WorldController Instance { get; protected set; }
    public static World World { get; protected set; }

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
        World = new World(worldWidth, worldHeight);

        //center camera
        Camera.main.transform.position = new Vector3(World.Width / 2, World.Height / 2, Camera.main.transform.position.z);
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
}
