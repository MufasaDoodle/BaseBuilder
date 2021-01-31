using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StructureSpriteController : MonoBehaviour
{
    Dictionary<Structure, GameObject> structureGameObjectMap;
    Dictionary<string, Sprite> structureSprites;

    World World
    {
        get
        {
            return WorldController.World;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        LoadSprites();

        structureGameObjectMap = new Dictionary<Structure, GameObject>();

        World.RegisterStructureChanged(OnStructureCreated);

        foreach (var obj in World.structures)
        {
            OnStructureCreated(obj);
        }
    }

    private void LoadSprites()
    {
        structureSprites = new Dictionary<string, Sprite>();
        //right now, only wood walls are loaded
        Sprite[] sprites = Resources.LoadAll<Sprite>("Sprites/Structures/");

        foreach (var sprite in sprites)
        {
            structureSprites[sprite.name] = sprite;
        }
    }

    public void OnStructureCreated(Structure obj)
    {
        //create GameObject linked to this data

        //TODO does not consider multi-tile objects nor rotated objects

        GameObject obj_go = new GameObject();        

        structureGameObjectMap.Add(obj, obj_go);

        obj_go.name = $"{obj.ObjectType} ({obj.Tile.X},{obj.Tile.Y})";
        obj_go.transform.position = new Vector3(obj.Tile.X, obj.Tile.Y, 0);
        obj_go.transform.SetParent(this.transform, true);

        //TODO: hardcoded, fix later
        if (obj.ObjectType == "Door")
        {
            Tile northTile = World.GetTileAt(obj.Tile.X, obj.Tile.Y + 1);
            Tile southTile = World.GetTileAt(obj.Tile.X, obj.Tile.Y - 1);

            if (northTile != null && southTile != null && northTile.Structure != null && southTile.Structure != null && northTile.Structure.ObjectType.Contains("Wall") && southTile.Structure.ObjectType.Contains("Wall"))
            {
                obj_go.transform.rotation = Quaternion.Euler(0, 0, 90);
            }
        }

        SpriteRenderer sr = obj_go.AddComponent<SpriteRenderer>();
        sr.sprite = GetSpriteForStructure(obj);
        sr.sortingLayerName = "Structures";

        obj.RegisterOnChanged(OnStructureChanged);
    }

    public Sprite GetSpriteForStructure(Structure structure)
    {
        string spriteName = structure.ObjectType;

        if (structure.LinksToNeighbour == false)
        {
            //if door, check openness and update sprite
            //todo: all hardcoded for now, fix later
            if (structure.ObjectType == "Door")
            {
                if (structure.GetParameter("openness") < 0.1f)
                {
                    //door is closed
                    spriteName = "Door";
                }
                else if (structure.GetParameter("openness") < 0.5f)
                {
                    //door is a bit open
                    spriteName = "Door_openness_1";
                }
                else if (structure.GetParameter("openness") < 0.9f)
                {
                    //door is a lot open
                    spriteName = "Door_openness_2";
                }
                else
                {
                    //door is completely open
                    spriteName = "Door_openness_3";
                }

                return structureSprites[spriteName];
            }
            return structureSprites[structure.ObjectType];
        }

        spriteName = structure.ObjectType + "_";

        //check for neighbours NESW
        int x = structure.Tile.X;
        int y = structure.Tile.Y;

        Tile t;
        t = WorldController.World.GetTileAt(x, y + 1);

        if (t != null && t.Structure != null && t.Structure.ObjectType.Equals(structure.ObjectType))
        {
            spriteName += "N";
        }

        t = WorldController.World.GetTileAt(x + 1, y);

        if (t != null && t.Structure != null && t.Structure.ObjectType.Equals(structure.ObjectType))
        {
            spriteName += "E";
        }

        t = WorldController.World.GetTileAt(x, y - 1);

        if (t != null && t.Structure != null && t.Structure.ObjectType.Equals(structure.ObjectType))
        {
            spriteName += "S";
        }

        t = WorldController.World.GetTileAt(x - 1, y);

        if (t != null && t.Structure != null && t.Structure.ObjectType.Equals(structure.ObjectType))
        {
            spriteName += "W";
        }

        if (structureSprites.ContainsKey(spriteName) == false)
        {
            Debug.LogError($"GetSpriteForStructure -- no sprites with name {spriteName}");
            return null;
        }

        return structureSprites[spriteName];
    }

    public Sprite GetSpriteForStructure(string objectType)
    {
        if (structureSprites.ContainsKey(objectType))
        {
            return structureSprites[objectType];
        }

        if (structureSprites.ContainsKey(objectType + "_"))
        {
            return structureSprites[objectType + "_"];
        }

        Debug.LogError($"GetSpriteForStructure -- no sprites with name {objectType}");
        return null;
    }

    void OnStructureChanged(Structure obj)
    {
        //make sure object's graphics are correct
        if (structureGameObjectMap.ContainsKey(obj) == false)
        {
            Debug.LogError("OnStructureChanged - trying to change visuals for installed object not in map");
            return;
        }

        GameObject obj_go = structureGameObjectMap[obj];

        obj_go.GetComponent<SpriteRenderer>().sprite = GetSpriteForStructure(obj);

    }
}
