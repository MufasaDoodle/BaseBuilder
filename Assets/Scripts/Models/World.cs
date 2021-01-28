using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class World
{
    Tile[,] tiles;
    List<Character> characters;

    Dictionary<string, InstalledObject> installedObjectPrototypes;

    public int Width { get; }
    public int Height { get; }

    Action<InstalledObject> InstalledObjectChanged;
    Action<Tile> TileChanged;
    Action<Character> characterCreated;

    //TODO replace this with proper job manager later
    public JobQueue jobQueue;

    public World(int width = 100, int height = 100)
    {
        jobQueue = new JobQueue();

        this.Width = width;
        this.Height = height;

        tiles = new Tile[width + 1, height + 1];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                tiles[x, y] = new Tile(this, x, y);
                tiles[x, y].RegisterTileTypeChangedCallback(OnTileChanged);
            }
        }

        Debug.Log($"World created with {width * height} tiles");

        CreateInstalledObjectPrototypes();

        characters = new List<Character>();
    }

    public void Update(float deltaTime)
    {
        foreach (var c in characters)
        {
            c.Update(deltaTime);
        }
    }

    public Character CreateCharacter(Tile t)
    {
        Character c = new Character(t);
        if (characterCreated != null)
        {
            characterCreated(c);
        }

        characters.Add(c);

        return c;
    }

    void CreateInstalledObjectPrototypes()
    {
        installedObjectPrototypes = new Dictionary<string, InstalledObject>();

        installedObjectPrototypes.Add("MetalWall", InstalledObject.CreatePrototype("MetalWall", 0f, 1, 1, true));
    }

    public void InitializeWorldWithEmptySpace()
    {
        Debug.Log("Initializing world tiles to dirt");
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                tiles[x, y].Type = TileType.Empty;
            }
        }
    }

    public void SetupPathfindingExample()
    {
        int l = Width / 2 - 5;
        int b = Height / 2 - 5;

        for (int x = l-5; x < l + 15; x++)
        {
            for (int y = b-5; y < b + 15; y++)
            {
                tiles[x, y].Type = TileType.Floor;

                if(x == l || x == (l+9) || y == b | y == (b + 9))
                {
                    if(x != (l+9) && y != (b + 4))
                    {
                        PlaceInstalledObject("MetalWall", tiles[x, y]);
                    }
                }
            }
        }
    }

    public Tile GetTileAt(int x, int y)
    {
        if (x > Width || x < 0 || y > Height || y < 0)
        {
            Debug.LogError($"Tile ({x},{y}) is out of range");
            return null;
        }
        return tiles[x, y];
    }

    public void PlaceInstalledObject(string objectType, Tile t)
    {
        //TODO: function assumes 1x1 tile. change later

        if (!installedObjectPrototypes.ContainsKey(objectType))
        {
            Debug.LogError($"InstalledObjectPrototypes does not contain a proto for key {objectType}");
            return;
        }

        InstalledObject obj = InstalledObject.PlaceInstance(installedObjectPrototypes[objectType], t);

        if (obj == null)
        {
            //failed to place object, most likely there is already something there
            return;
        }

        if (InstalledObjectChanged != null)
        {
            InstalledObjectChanged(obj);
        }
    }

    public InstalledObject GetInstalledObjectPrototype(string objectType)
    {
        if (installedObjectPrototypes.ContainsKey(objectType) == false)
        {
            Debug.LogError($"No furniture with type {objectType}");
            return null;
        }

        return installedObjectPrototypes[objectType];
    }

    public bool IsInstalledObjectPlacementValid(string objectType, Tile t)
    {
        return installedObjectPrototypes[objectType].IsValidPosition(t);
    }

    public void RegisterInstalledObject(Action<InstalledObject> callbackFunc)
    {
        InstalledObjectChanged += callbackFunc;
    }

    public void UnregisterInstalledObject(Action<InstalledObject> callbackFunc)
    {
        InstalledObjectChanged -= callbackFunc;
    }

    public void RegisterTileChanged(Action<Tile> callbackFunc)
    {
        TileChanged += callbackFunc;
    }

    public void UnregisterTileChanged(Action<Tile> callbackFunc)
    {
        TileChanged -= callbackFunc;
    }

    public void RegisterCharacterCreated(Action<Character> callbackFunc)
    {
        characterCreated += callbackFunc;
    }

    public void UnregisterCharacterCreated(Action<Character> callbackFunc)
    {
        characterCreated -= callbackFunc;
    }

    void OnTileChanged(Tile t)
    {
        if (TileChanged == null)
        {
            return;
        }
        TileChanged(t);
    }
}
