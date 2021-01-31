using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Xml.Serialization;
using System.Xml;
using System.Xml.Schema;

public class Structure : IXmlSerializable
{
    protected Dictionary<string, float> structureParameters;
    protected Action<Structure, float> updateActions;

    public Func<Structure, Enterability> isEnterable;

    public void Update(float deltaTime)
    {
        if(updateActions != null)
        {
            updateActions(this, deltaTime);
        }
    }

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

    public bool RoomEnclosure { get; protected set; }

    //fx, sofa might be 3x2 (graphics may only occupy 3x1, but the extra space is for leg room)
    int width;
    int height;

    public bool LinksToNeighbour { get; protected set; }

    public Action<Structure> OnChanged;

    Func<Tile, bool> funcPositionValidation;

    private Structure()
    {
        structureParameters = new Dictionary<string, float>();
    }

    protected Structure(Structure other)
    {
        this.ObjectType = other.ObjectType;
        this.MovementCost = other.MovementCost;
        this.RoomEnclosure = other.RoomEnclosure;
        this.width = other.width;
        this.height = other.height;
        this.LinksToNeighbour = other.LinksToNeighbour;

        this.structureParameters = new Dictionary<string, float>(other.structureParameters);
        if (other.updateActions != null)
        {
            updateActions = (Action<Structure, float>)other.updateActions.Clone();
        }

        this.isEnterable = other.isEnterable;
    }

    virtual public Structure Clone()
    {
        return new Structure(this);
    }

    public Structure (string objectType, float movementCost = 1f, int width = 1, int height = 1, bool linksToNeighbour = false, bool roomEnclosure = false)
    {
        this.ObjectType = objectType;
        this.MovementCost = movementCost;
        this.RoomEnclosure = roomEnclosure;
        this.width = width;
        this.height = height;
        this.LinksToNeighbour = linksToNeighbour;

        structureParameters = new Dictionary<string, float>();

        this.funcPositionValidation = this.__IsValidPosition;
    }

    static public Structure PlaceInstance(Structure proto, Tile tile)
    {
        //check if destination placement is valid
        if(proto.funcPositionValidation(tile) == false)
        {
            Debug.LogError($"PlaceInstance -> position validity function returned false");
            return null;
        }

        Structure obj = proto.Clone();

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

            if (t != null && t.Structure != null && t.Structure.OnChanged != null && t.Structure.ObjectType.Equals(obj.ObjectType))
            {
                t.Structure.OnChanged(t.Structure); //fires on change event, provoking the object to figure out its new graphic state
            }

            t = tile.World.GetTileAt(x + 1, y);

            if (t != null && t.Structure != null && t.Structure.OnChanged != null && t.Structure.ObjectType.Equals(obj.ObjectType))
            {
                t.Structure.OnChanged(t.Structure);
            }

            t = tile.World.GetTileAt(x, y - 1);

            if (t != null && t.Structure != null && t.Structure.OnChanged != null && t.Structure.ObjectType.Equals(obj.ObjectType))
            {
                t.Structure.OnChanged(t.Structure);
            }

            t = tile.World.GetTileAt(x - 1, y);

            if (t != null && t.Structure != null && t.Structure.OnChanged != null && t.Structure.ObjectType.Equals(obj.ObjectType))
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

    protected bool __IsValidPosition(Tile t)
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

    public float GetParameter(string key, float default_value = 0)
    {
        if (structureParameters.ContainsKey(key) == false)
        {
            return default_value;
        }
        return structureParameters[key];
    }

    public void SetParameter(string key, float value)
    {
        structureParameters[key] = value;
    }

    public void ChangeParameter(string key, float value)
    {
        if (structureParameters.ContainsKey(key) == false)
        {
            //structureParameters[key] = value;
            return;
        }
        structureParameters[key] += value;
    }

    public void RegisterUpdateAction(Action<Structure, float> a)
    {
        updateActions += a;
    }
    public void UnregisterUpdateAction(Action<Structure, float> a)
    {
        updateActions -= a;
    }

    public void RegisterOnChanged(Action<Structure> callbackFunc)
    {
        OnChanged += callbackFunc;
    }

    public void UnregisterOnChanged(Action<Structure> callbackFunc)
    {
        OnChanged -= callbackFunc;
    }

    //SERIALIZAING

    public XmlSchema GetSchema()
    {
        return null;
    }

    public void ReadXml(XmlReader reader)
    {
        //MovementCost = (float)float.Parse(reader.GetAttribute("movementCost"));

        if (reader.ReadToDescendant("Param"))
        {
            do
            {
                string k = reader.GetAttribute("name");
                float v = float.Parse(reader.GetAttribute("value"));
                structureParameters[k] = v;
            } 
            while (reader.ReadToNextSibling("Param"));
        }
    }

    public void WriteXml(XmlWriter writer)
    {
        writer.WriteAttributeString("X", Tile.X.ToString());
        writer.WriteAttributeString("Y", Tile.Y.ToString());
        writer.WriteAttributeString("objectType", ObjectType);
        //writer.WriteAttributeString("movementCost", MovementCost.ToString());

        foreach (var k in structureParameters.Keys)
        {
            writer.WriteStartElement("Param");
            writer.WriteAttributeString("name", k);
            writer.WriteAttributeString("value", structureParameters[k].ToString());
            writer.WriteEndElement();
        }
    }
}
