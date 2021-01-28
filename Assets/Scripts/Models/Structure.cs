using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Structure
{
    //represents BASE tile, in practice large objects may occupy more tiles
    public Tile Tile { get; protected set; }

    //this objectType will be queried by the visual system to know what sprite to render for this object
    public string ObjectType { get; protected set; }

    //movement cost for walking on top of this object. it is a multiplier, Value of 2 means walking at half speed, 1 is full speed.
    //tile types and other envrionment effects may be combined
    //fx a rough tile with a cost of 2 with a table on it with a cost of 3 that is on fire with cost of 3
    //would have a total movement cost of (2+3+3) = 8, so you would move through here at 1/8th speed
    //IF MOVEMENT COST = 0, TILE IS IMPASSIBLE. fx walls
    public float MovementCost { get; protected set; }

    //fx, sofa might be 3x2 (graphics may only occupy 3x1, but the extra space is for leg room)
    int width;
    int height;

    public bool LinksToNeighbour { get; protected set; }

    Action<Structure> OnChanged;

    Func<Tile, bool> funcPositionValidation;

    protected Structure()
    {

    }

    static public Structure CreatePrototype(string objectType, float movementCost = 1f, int width = 1, int height = 1, bool linksToNeighbour = false)
    {
        Structure obj = new Structure();
        obj.ObjectType = objectType;
        obj.MovementCost = movementCost;
        obj.width = width;
        obj.height = height;
        obj.LinksToNeighbour = linksToNeighbour;

        obj.funcPositionValidation = obj.__IsValidPosition;

        return obj;
    }

    static public Structure PlaceInstance(Structure proto, Tile tile)
    {
        //check if destination placement is valid
        if(proto.funcPositionValidation(tile) == false)
        {
            Debug.LogError($"PlaceInstance -> position validity function returned false");
            return null;
        }

        Structure obj = new Structure();
        obj.ObjectType = proto.ObjectType;
        obj.MovementCost = proto.MovementCost;
        obj.width = proto.width;
        obj.height = proto.height;
        obj.LinksToNeighbour = proto.LinksToNeighbour;

        obj.Tile = tile;

        //TODO: This assumes the object is 1x1!!!
        if (!tile.PlaceObject(obj))
        {
            //was not able to place object on that tile
            //could already be occupied

            return null;
        }

        if (obj.LinksToNeighbour)
        {
            //might have neighbours that must be notified of its placement

            Tile t;
            int x = tile.X;
            int y = tile.Y;

            t = tile.World.GetTileAt(x, y + 1);

            if (t != null && t.Structure != null && t.Structure.ObjectType.Equals(obj.ObjectType))
            {
                t.Structure.OnChanged(t.Structure); //fires on change event, provoking the object to figure out its new graphic state
            }

            t = tile.World.GetTileAt(x + 1, y);

            if (t != null && t.Structure != null && t.Structure.ObjectType.Equals(obj.ObjectType))
            {
                t.Structure.OnChanged(t.Structure);
            }

            t = tile.World.GetTileAt(x, y - 1);

            if (t != null && t.Structure != null && t.Structure.ObjectType.Equals(obj.ObjectType))
            {
                t.Structure.OnChanged(t.Structure);
            }

            t = tile.World.GetTileAt(x - 1, y);

            if (t != null && t.Structure != null && t.Structure.ObjectType.Equals(obj.ObjectType))
            {
                t.Structure.OnChanged(t.Structure);
            }
        }

        return obj;
    }

    public bool IsValidPosition(Tile t)
    {
        return funcPositionValidation(t);
    }

    public bool __IsValidPosition(Tile t)
    {
        //check if tile is a floor or other invalid tiles
        if(t.Type != TileType.Floor)
        {
            return false;
        }

        //check if tile does not already contain installed object
        if(t.Structure != null)
        {
            return false;
        }

        return true;
    }

    public bool __IsValidPosition_Door(Tile t)
    {
        //make sure we have a pair of E/W walls or N/S walls to ensure valid door placement
        if (__IsValidPosition(t) == false)
        {
            return false;
        }

        return true;
    }

    public void RegisterOnChanged(Action<Structure> callbackFunc)
    {
        OnChanged += callbackFunc;
    }

    public void UnregisterOnChanged(Action<Structure> callbackFunc)
    {
        OnChanged -= callbackFunc;
    }
}
