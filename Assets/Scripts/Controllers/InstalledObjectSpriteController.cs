using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstalledObjectSpriteController : MonoBehaviour
{
    Dictionary<InstalledObject, GameObject> installedObjectGameObjectMap;
    Dictionary<string, Sprite> installedObjectSprites;

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

        installedObjectGameObjectMap = new Dictionary<InstalledObject, GameObject>();        

        World.RegisterInstalledObject(OnInstalledObjectCreated);
    }

    private void LoadSprites()
    {
        installedObjectSprites = new Dictionary<string, Sprite>();
        //right now, only wood walls are loaded
        Sprite[] sprites = Resources.LoadAll<Sprite>("Sprites/Structures/Walls/Metal/MetalSheet");

        foreach (var sprite in sprites)
        {
            installedObjectSprites[sprite.name] = sprite;
        }
    }

    public void OnInstalledObjectCreated(InstalledObject obj)
    {
        //create GameObject linked to this data

        //TODO does not consider multi-tile objects nor rotated objects

        GameObject obj_go = new GameObject();

        installedObjectGameObjectMap.Add(obj, obj_go);

        obj_go.name = $"{obj.ObjectType} ({obj.Tile.X},{obj.Tile.Y})";
        obj_go.transform.position = new Vector3(obj.Tile.X, obj.Tile.Y, 0);
        obj_go.transform.SetParent(this.transform, true);

        SpriteRenderer sr = obj_go.AddComponent<SpriteRenderer>();
        sr.sprite = GetSpriteForInstalledObject(obj);
        sr.sortingLayerName = "InstalledObjects";

        obj.RegisterOnChanged(OnInstalledObjectChanged);
    }

    public Sprite GetSpriteForInstalledObject(InstalledObject obj)
    {
        if(obj.LinksToNeighbour == false)
        {
            return installedObjectSprites[obj.ObjectType];
        }

        string spriteName = obj.ObjectType + "_";

        //check for neighbours NESW
        int x = obj.Tile.X;
        int y = obj.Tile.Y;

        Tile t;
        t = WorldController.World.GetTileAt(x, y + 1);

        if(t != null && t.InstalledObject != null && t.InstalledObject.ObjectType.Equals(obj.ObjectType))
        {
            spriteName += "N";
        }

        t = WorldController.World.GetTileAt(x + 1, y);

        if (t != null && t.InstalledObject != null && t.InstalledObject.ObjectType.Equals(obj.ObjectType))
        {
            spriteName += "E";
        }

        t = WorldController.World.GetTileAt(x, y - 1);

        if (t != null && t.InstalledObject != null && t.InstalledObject.ObjectType.Equals(obj.ObjectType))
        {
            spriteName += "S";
        }

        t = WorldController.World.GetTileAt(x - 1, y);

        if (t != null && t.InstalledObject != null && t.InstalledObject.ObjectType.Equals(obj.ObjectType))
        {
            spriteName += "W";
        }

        if(installedObjectSprites.ContainsKey(spriteName) == false)
        {
            Debug.LogError($"GetSpriteForInstalledObject -- no sprites with name {spriteName}");
            return null;
        }

        return installedObjectSprites[spriteName];
    }

    public Sprite GetSpriteForInstalledObject(string objectType)
    {
        if (installedObjectSprites.ContainsKey(objectType))
        {
            return installedObjectSprites[objectType];
        }

        if (installedObjectSprites.ContainsKey(objectType+"_"))
        {
            return installedObjectSprites[objectType+"_"];
        }

        Debug.LogError($"GetSpriteForInstalledObject -- no sprites with name {objectType}");
        return null;
    }

    void OnInstalledObjectChanged(InstalledObject obj)
    {
        //make sure object's graphics are correct
        if (installedObjectGameObjectMap.ContainsKey(obj) == false)
        {
            Debug.LogError("OnInstalledObjectChanged - trying to change visuals for installed object not in map");
            return;
        }

        GameObject obj_go = installedObjectGameObjectMap[obj];

        obj_go.GetComponent<SpriteRenderer>().sprite = GetSpriteForInstalledObject(obj);
    }
}
