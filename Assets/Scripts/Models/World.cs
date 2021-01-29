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
    public List<Character> characters;
    public List<Structure> structures;

    //used for pathfinding
    public Path_TileGraph tileGraph;

    Dictionary<string, Structure> structurePrototypes;

    public int Width { get; protected set; }
    public int Height { get; protected set; }

    Action<Structure> StructureChanged;
    Action<Tile> TileChanged;
    Action<Character> characterCreated;

    //TODO replace this with proper job manager later
    public JobQueue jobQueue;

    public World(int width = 100, int height = 100)
    {
        SetupWorld(width, height);

        Character c = CreateCharacter(GetTileAt(Width / 2, Height / 2));
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

        CreateStructurePrototypes();

        characters = new List<Character>();
        structures = new List<Structure>();
    }

    public void Update(float deltaTime)
    {
        foreach (var c in characters)
        {
            c.Update(deltaTime);
        }
        foreach (var s in structures)
        {
            s.Update(deltaTime);
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

    void CreateStructurePrototypes()
    {
        //TODO: replace this with a function that reads data from a file instead

        structurePrototypes = new Dictionary<string, Structure>();

        structurePrototypes.Add("MetalWall", new Structure("MetalWall", 0f, 1, 1, true));
        structurePrototypes.Add("Door", new Structure("Door", 1f, 1, 1, false));

        structurePrototypes["Door"].structureParameters["Openness"] = 0;
        structurePrototypes["Door"].updateActions += StructureActions.Door_UpdateAction;
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
                        PlaceStructure("MetalWall", tiles[x, y]);
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

    public Structure PlaceStructure(string objectType, Tile t)
    {
        //TODO: function assumes 1x1 tile. change later

        if (!structurePrototypes.ContainsKey(objectType))
        {
            Debug.LogError($"StructurePrototypes does not contain a proto for key {objectType}");
            return null;
        }

        Structure obj = Structure.PlaceInstance(structurePrototypes[objectType], t);

        if (obj == null)
        {
            //failed to place object, most likely there is already something there
            return null;
        }

        structures.Add(obj);

        if (StructureChanged != null)
        {
            StructureChanged(obj);
            InvalidateTileGraph();
        }

        return obj;
    }

    public Structure GetStructurePrototype(string objectType)
    {
        if (structurePrototypes.ContainsKey(objectType) == false)
        {
            Debug.LogError($"No furniture with type {objectType}");
            return null;
        }

        return structurePrototypes[objectType];
    }

    public bool IsStructurePlacementValid(string objectType, Tile t)
    {
        return structurePrototypes[objectType].IsValidPosition(t);
    }

    public void InvalidateTileGraph()
    {
        tileGraph = null;
    }

    #region callbacks
    public void RegisterStructureChanged(Action<Structure> callbackFunc)
    {
        StructureChanged += callbackFunc;
    }

    public void UnregisterStructureChanged(Action<Structure> callbackFunc)
    {
        StructureChanged -= callbackFunc;
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
    #endregion

    #region SAVING&LOADING
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
                case "Structures":
                    ReadXML_Structures(reader);
                    break;
                case "Characters":
                    ReadXML_Characters(reader);
                    break;
            }
        }
    }

    private void ReadXML_Characters(XmlReader reader)
    {
        if (reader.ReadToDescendant("Character"))
        {
            do
            {
                int x = int.Parse(reader.GetAttribute("X"));
                int y = int.Parse(reader.GetAttribute("Y"));

                Character c = CreateCharacter(tiles[x, y]);

                c.ReadXml(reader);
            } 
            while (reader.ReadToNextSibling("Character"));
        }
    }

    private void ReadXML_Tiles(XmlReader reader)
    {
        if (reader.ReadToDescendant("Tile"))
        {
            do
            {
                int x = int.Parse(reader.GetAttribute("X"));
                int y = int.Parse(reader.GetAttribute("Y"));
                tiles[x, y].ReadXml(reader);
            }
            while (reader.ReadToNextSibling("Tile"));
        }
    }

    private void ReadXML_Structures(XmlReader reader)
    {
        if (reader.ReadToDescendant("Structure"))
        {
            do
            {
                int x = int.Parse(reader.GetAttribute("X"));
                int y = int.Parse(reader.GetAttribute("Y"));

                Structure structure = PlaceStructure(reader.GetAttribute("objectType"), tiles[x, y]);
                structure.ReadXml(reader);
            } 
            while (reader.ReadToNextSibling("Structure"));
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
                if (tiles[x, y].Type == TileType.Empty)
                {
                    continue;
                }
                //TODO: ignore tiles with empty space
                writer.WriteStartElement("Tile");
                tiles[x, y].WriteXml(writer);
                writer.WriteEndElement();
            }
        }
        writer.WriteEndElement();

        //Structures
        writer.WriteStartElement("Structures");
        foreach (var structure in structures)
        {
            writer.WriteStartElement("Structure");
            structure.WriteXml(writer);
            writer.WriteEndElement();
        }
        writer.WriteEndElement();

        //Characters
        writer.WriteStartElement("Characters");
        foreach (var character in characters)
        {
            writer.WriteStartElement("Character");
            character.WriteXml(writer);
            writer.WriteEndElement();
        }
        writer.WriteEndElement();
    }
    #endregion SAVING&LOADING
}
