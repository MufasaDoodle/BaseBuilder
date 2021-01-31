using UnityEngine;
using System.Collections.Generic;

public class Room
{

    public float atmosO2 = 0;
    public float atmosN = 0;
    public float atmosCO2 = 0;

    List<Tile> tiles;

    public Room()
    {
        tiles = new List<Tile>();
    }

    public void AssignTile(Tile t)
    {
        if (tiles.Contains(t))
        {
            // This tile already in this room.
            return;
        }

        if (t.room != null)
        {
            // Belongs to some other room
            t.room.tiles.Remove(t);
        }

        t.room = this;
        tiles.Add(t);
    }

    public void UnassignAllTiles()
    {
        for (int i = 0; i < tiles.Count; i++)
        {
            tiles[i].room = tiles[i].World.GetOutsideRoom();    // Assign to outside
        }
        tiles = new List<Tile>();
    }

    public static void DoRoomFloodFill(Structure sourceStructure)
    {
        World world = sourceStructure.Tile.World;

        Room oldRoom = sourceStructure.Tile.room;

        // Try building new rooms for each of our NESW directions
        foreach (Tile t in sourceStructure.Tile.GetNeighbours())
        {
            ActualFloodFill(t, oldRoom);
        }

        sourceStructure.Tile.room = null;
        oldRoom.tiles.Remove(sourceStructure.Tile);

        if (oldRoom != world.GetOutsideRoom())
        {

            if (oldRoom.tiles.Count > 0)
            {
                Debug.LogError("'oldRoom' still has tiles assigned to it. This is clearly wrong.");
            }

            world.DeleteRoom(oldRoom);
        }

    }

    protected static void ActualFloodFill(Tile tile, Room oldRoom)
    {
        if (tile == null)
        {
            // We are trying to flood fill off the map, so just return
            // without doing anything.
            return;
        }

        if (tile.room != oldRoom)
        {
            // This tile was already assigned to another "new" room, which means
            // that the direction picked isn't isolated. So we can just return
            // without creating a new room.
            return;
        }

        if (tile.Structure != null && tile.Structure.RoomEnclosure)
        {
            // This tile has a wall/door/whatever in it, so clearly
            // we can't do a room here.
            return;
        }

        if (tile.Type == TileType.Empty)
        {
            // This tile is empty space and must remain part of the outside.
            return;
        }


        // If we get to this point, then we know that we need to create a new room.

        Room newRoom = new Room();
        Queue<Tile> tilesToCheck = new Queue<Tile>();
        tilesToCheck.Enqueue(tile);

        while (tilesToCheck.Count > 0)
        {
            Tile t = tilesToCheck.Dequeue();


            if (t.room == oldRoom)
            {
                newRoom.AssignTile(t);

                Tile[] ns = t.GetNeighbours();
                foreach (Tile t2 in ns)
                {
                    if (t2 == null || t2.Type == TileType.Empty)
                    {
                        // We have hit open space (either by being the edge of the map or being an empty tile)
                        newRoom.UnassignAllTiles();
                        return;
                    }

                    // We know t2 is not null nor is it an empty tile, so just make sure it
                    // hasn't already been processed and isn't a "wall" type tile.
                    if (t2.room == oldRoom && (t2.Structure == null || t2.Structure.RoomEnclosure == false))
                    {
                        tilesToCheck.Enqueue(t2);
                    }
                }

            }
        }

        newRoom.atmosCO2 = oldRoom.atmosCO2;
        newRoom.atmosN = oldRoom.atmosN;
        newRoom.atmosO2 = oldRoom.atmosO2;

        // Tell the world that a new room has been formed.
        tile.World.AddRoom(newRoom);
    }

}
