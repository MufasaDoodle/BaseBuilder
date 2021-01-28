using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileSpriteController : MonoBehaviour
{
    public Sprite emptySprite;
    public Sprite floorSprite;

    Dictionary<Tile, GameObject> tileGameObjectMap;

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
        tileGameObjectMap = new Dictionary<Tile, GameObject>();

        for (int x = 0; x < World.Width; x++)
        {
            for (int y = 0; y < World.Height; y++)
            {
                Tile tileData = World.GetTileAt(x, y);
                GameObject tile_go = new GameObject();

                tileGameObjectMap.Add(tileData, tile_go);

                tile_go.name = $"Tile ({x},{y})";
                tile_go.transform.position = new Vector3(tileData.X, tileData.Y, 0);
                tile_go.transform.SetParent(this.transform, true);

                SpriteRenderer sr = tile_go.AddComponent<SpriteRenderer>();
                sr.sprite = emptySprite;
                sr.sortingLayerName = "Tiles";

                OnTileChanged(tileData);
            }
        }

        World.RegisterTileChanged(OnTileChanged);
    }

    void OnTileChanged(Tile tileData)
    {
        GameObject tile_go;
        if (!tileGameObjectMap.TryGetValue(tileData, out tile_go))
        {
            Debug.LogError("tileGameObjectMap doesn't contain the tile_data, did you forget to add the tile to the dictionary?");
            return;
        }

        if (tileData.Type == TileType.Empty)
        {
            tile_go.GetComponent<SpriteRenderer>().sprite = emptySprite;
        }
        else if (tileData.Type == TileType.Floor)
        {
            tile_go.GetComponent<SpriteRenderer>().sprite = floorSprite;
        }
        else
        {
            Debug.LogError("OnTileTypeChanged: Unrecognized tile type");
        }
    }
}
