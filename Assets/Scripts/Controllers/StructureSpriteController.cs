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

        SpriteRenderer sr = obj_go.AddComponent<SpriteRenderer>();
        sr.sprite = GetSpriteForStructure(obj);
        sr.sortingLayerName = "Structures";

        obj.RegisterOnChanged(OnStructureChanged);
    }

    public Sprite GetSpriteForStructure(Structure obj)
    {
        if(obj.LinksToNeighbour == false)
        {
            return structureSprites[obj.ObjectType];
        }

        string spriteName = obj.ObjectType + "_";

        //check for neighbours NESW
        int x = obj.Tile.X;
        int y = obj.Tile.Y;

        Tile t;
        t = WorldController.World.GetTileAt(x, y + 1);

        if(t != null && t.Structure != null && t.Structure.ObjectType.Equals(obj.ObjectType))
        {
            spriteName += "N";
        }

        t = WorldController.World.GetTileAt(x + 1, y);

        if (t != null && t.Structure != null && t.Structure.ObjectType.Equals(obj.ObjectType))
        {
            spriteName += "E";
        }

        t = WorldController.World.GetTileAt(x, y - 1);

        if (t != null && t.Structure != null && t.Structure.ObjectType.Equals(obj.ObjectType))
        {
            spriteName += "S";
        }

        t = WorldController.World.GetTileAt(x - 1, y);

        if (t != null && t.Structure != null && t.Structure.ObjectType.Equals(obj.ObjectType))
        {
            spriteName += "W";
        }

        if(structureSprites.ContainsKey(spriteName) == false)
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

        if (structureSprites.ContainsKey(objectType+"_"))
        {
            return structureSprites[objectType+"_"];
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
