﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

using System.Xml.Serialization;
using System.Xml;
using System.Xml.Schema;

public enum TileType { Empty, Floor }
public enum Enterability { Yes, Never, Soon}

public class Tile : IXmlSerializable
{
    private TileType _type = TileType.Empty;

    public TileType Type
    {
        get { return _type; }
        set
        {
            TileType oldType = _type;
            _type = value;
            // Call the callback and let things know we've changed.

            if (tileChanged != null && oldType != _type)
            {
                tileChanged(this);
            }
        }
    }

    public LooseObject LooseObject { get; protected set; }
    public Structure Structure { get; protected set; }

    public Room room;

    public Job pendingStructureJob;

    Action<Tile> tileChanged;

    public World World { get; protected set; }

    public int X { get; protected set; }

    public int Y { get; protected set; }

    float baseTileMovementCost = 1; //TODO hardcoded, fix

    public float movementCost
    {
        get
        {
            if (Type == TileType.Empty)
            {
                return 0; //unwalkable
            }

            if (Structure == null)
            {
                return baseTileMovementCost;
            }

            return baseTileMovementCost * Structure.MovementCost;
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Tile"></see> class
    /// </summary>
    /// <param name="world"></param>
    /// <param name="X"></param>
    /// <param name="Y"></param>
    public Tile(World world, int x, int y)
    {
        this.World = world;
        this.X = x;
        this.Y = y;
    }

    /// <summary>
    /// Adds a method to the <see cref="tileChanged"/> delegate
    /// </summary>
    /// <param name="callback"></param>
    public void RegisterTileTypeChangedCallback(Action<Tile> callback)
    {
        tileChanged += callback;
    }

    public void UnregisterTileTypeChangedCallback(Action<Tile> callback)
    {
        tileChanged -= callback;
    }

    public bool PlaceObject(Structure objInstance)
    {
        if (objInstance == null)
        {
            //uninstalling object
            Structure = null;
            return true;
        }

        if (Structure != null)
        {
            Debug.LogError($"Trying to assign an installed object to tile that already has one");
            return false;
        }

        Structure = objInstance;
        return true;
    }

    public bool IsNeighbour(Tile tile, bool diagOkay = false)
    {
        return Mathf.Abs(tile.X - this.X) + Mathf.Abs(tile.Y - this.Y) == 1 || (diagOkay && Mathf.Abs(tile.X - this.X) == 1 && Mathf.Abs(tile.Y - this.Y) == 1);
    }

    public Tile[] GetNeighbours(bool diagOkay = false)
    {
        Tile[] ns;

        if (diagOkay == false)
        {
            ns = new Tile[4];
        }
        else
        {
            ns = new Tile[8];
        }

        Tile n;

        n = World.GetTileAt(X, Y + 1);
        ns[0] = n;

        n = World.GetTileAt(X + 1, Y);
        ns[1] = n;

        n = World.GetTileAt(X, Y - 1);
        ns[2] = n;

        n = World.GetTileAt(X - 1, Y);
        ns[3] = n;

        if (diagOkay)
        {
            //NE
            n = World.GetTileAt(X + 1, Y + 1);
            ns[4] = n;

            //SE
            n = World.GetTileAt(X + 1, Y - 1);
            ns[5] = n;

            //SW
            n = World.GetTileAt(X - 1, Y - 1);
            ns[6] = n;

            //NW
            n = World.GetTileAt(X - 1, Y + 1);
            ns[7] = n;
        }

        return ns;
    }

    public Enterability IsEnterable()
    {
        if(movementCost == 0)
        {
            return Enterability.Never;
        }

        if(Structure != null && Structure.isEnterable != null)
        {
            return Structure.isEnterable(Structure);
        }

        return Enterability.Yes;
    }

    public Tile North()
    {
        return World.GetTileAt(X, Y + 1);
    }
    public Tile South()
    {
        return World.GetTileAt(X, Y - 1);
    }
    public Tile East()
    {
        return World.GetTileAt(X + 1, Y);
    }
    public Tile West()
    {
        return World.GetTileAt(X - 1, Y);
    }

    // SERIALIZATION
    public XmlSchema GetSchema()
    {
        return null;
    }

    public void WriteXml(XmlWriter writer)
    {
        writer.WriteAttributeString("X", X.ToString());
        writer.WriteAttributeString("Y", Y.ToString());
        writer.WriteAttributeString("Type", ((int)Type).ToString());
    }

    public void ReadXml(XmlReader reader)
    {
        Type = (TileType)int.Parse(reader.GetAttribute("Type"));
    }
}
