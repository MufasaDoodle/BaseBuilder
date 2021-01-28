using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Path_TileGraph
{
    public Dictionary<Tile, Path_Node<Tile>> nodes;

    public Path_TileGraph(World world)
    {
        Debug.Log("Path_TileGraph");
        //assumptions:
        //create a node for each floor tile
        //we don't create nodes for tiles that are unwalkable (like walls)

        nodes = new Dictionary<Tile, Path_Node<Tile>>();

        for (int x = 0; x < world.Width; x++)
        {
            for (int y = 0; y < world.Height; y++)
            {
                Tile t = world.GetTileAt(x, y);
                
                //if (t.movementCost > 0) //tiles with movement of 0 is unwalkable
                //{
                    Path_Node<Tile> n = new Path_Node<Tile>();
                    n.data = t;
                    nodes.Add(t, n);
                //}
            }
        }

        Debug.Log($"Path_TileGraph: Created {nodes.Count} nodes");

        int edgeCount = 0;

        foreach (Tile t in nodes.Keys)
        {
            Path_Node<Tile> n = nodes[t];

            List<Path_Edge<Tile>> edges = new List<Path_Edge<Tile>>();

            Tile[] neighbours = t.GetNeighbours(true); //SOME ARRAY SPOTS COULD BE NULL

            for (int i = 0; i < neighbours.Length; i++)
            {
                if (neighbours[i] != null && neighbours[i].movementCost > 0)
                {
                    //neighbour tile is walkable, so create edge

                    //first we must make sure we are not clipping a diagonal inappropriately
                    if (IsClippingCorner(t, neighbours[i]))
                    {
                        continue;
                    }

                    Path_Edge<Tile> e = new Path_Edge<Tile>();
                    e.cost = neighbours[i].movementCost;
                    e.node = nodes[neighbours[i]];

                    //add edge to list 
                    edges.Add(e);

                    edgeCount++;
                }
            }

            n.edges = edges.ToArray();
        }

        Debug.Log($"Path_TileGraph: Created {edgeCount} edges");
    }

    bool IsClippingCorner(Tile curr, Tile neigh)
    {
        if(Mathf.Abs(curr.X - neigh.X) + Mathf.Abs(curr.Y - neigh.Y) == 2)
        {
            //we are diagonal
            int dX = curr.X - neigh.X;
            int dY = curr.Y - neigh.Y;

            if(curr.World.GetTileAt(curr.X - dX, curr.Y).movementCost == 0)
            {
                //east or west is unwalkable, therefore this would be a clipped movement
                return true;
            }
            if (curr.World.GetTileAt(curr.X, curr.Y - dY).movementCost == 0)
            {
                //north or south is unwalkable, therefore this would be a clipped movement
                return true;
            }
        }

        return false;
    }
}
