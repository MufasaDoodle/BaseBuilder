using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Xml.Serialization;
using System.Xml;
using System.Xml.Schema;

public class World : IXmlSerializable
{
    Tile[,] tiles;
    List<Character> characters;
    public List<InstalledObject> installedObjects;

    //used for pathfinding
    public Path_TileGraph tileGraph;

    Dictionary<string, InstalledObject> installedObjectPrototypes;

    public int Width { get; protected set; }
    public int Height { get; protected set; }

    Action<InstalledObject> InstalledObjectChanged;
    Action<Tile> TileChanged;
    Action<Character> characterCreated;

    //TODO replace this with proper job manager later
    public JobQueue jobQueue;

    public World(int width = 100, int height = 100)
    {
        SetupWorld(width, height);
    }

    void SetupWorld(int width, int height)
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
        installedObjects = new List<InstalledObject>();
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

        for (int x = l - 5; x < l + 15; x++)
        {
            for (int y = b - 5; y < b + 15; y++)
            {
                tiles[x, y].Type = TileType.Floor;

                if (x == l || x == (l + 9) || y == b | y == (b + 9))
                {
                    if (x != (l + 9) && y != (b + 4))
                    {
                        PlaceInstalledObject("MetalWall", tiles[x, y]);
                    }
                }
            }
        }
    }

    public Tile GetTileAt(int x, int y)
    {
        if (x >= Width || x < 0 || y >= Height || y < 0)
        {
            //Debug.LogError($"Tile ({x},{y}) is out of range");
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

        installedObjects.Add(obj);

        if (InstalledObjectChanged != null)
        {
            InstalledObjectChanged(obj);
            InvalidateTileGraph();
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

        InvalidateTileGraph();
    }

    public void InvalidateTileGraph()
    {
        tileGraph = null;
    }

    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ///
    ///                                                 SAVING AND LOADING
    ///
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    //serialization requires a private default constructor
    private World()
    {

    }
    public XmlSchema GetSchema()
    {
        return null;
    }

    public void ReadXml(XmlReader reader)
    {
        //read info here

        Width = int.Parse(reader.GetAttribute("Width"));
        Height = int.Parse(reader.GetAttribute("Height"));

        SetupWorld(Width, Height);

        while (reader.Read())
        {
            switch (reader.Name)
            {
                case "Tiles":
                    ReadXML_Tiles(reader);
                    break;
            }
        }
    }

    private void ReadXML_Tiles(XmlReader reader)
    {
        while (reader.Read())
        {
            if (reader.Name != "Tile")
            {
                return; //no more tiles
            }
            int x = int.Parse(reader.GetAttribute("X"));
            int y = int.Parse(reader.GetAttribute("Y"));

            tiles[x, y].ReadXml(reader);
        }
    }

    public void WriteXml(XmlWriter writer)
    {
        //save info here
        writer.WriteAttributeString("Width", Width.ToString());
        writer.WriteAttributeString("Height", Height.ToString());

        //Tiles
        writer.WriteStartElement("Tiles");
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                //TODO: ignore tiles with empty space
                writer.WriteStartElement("Tile");
                tiles[x, y].WriteXml(writer);
                writer.WriteEndElement();
            }
        }
        writer.WriteEndElement();

        //InstalledObjects
        writer.WriteStartElement("InstalledObjects");
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                //TODO: ignore tiles with empty space
                writer.WriteStartElement("InstalledObject");
                tiles[x, y].WriteXml(writer);
                writer.WriteEndElement();
            }
        }
        writer.WriteEndElement();

    }
}
