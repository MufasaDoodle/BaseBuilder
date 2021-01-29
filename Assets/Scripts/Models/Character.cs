using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Xml.Serialization;
using System.Xml;
using System.Xml.Schema;

public class Character : IXmlSerializable
{
    public float X
    {
        get
        {
            return Mathf.Lerp(CurrTile.X, nextTile.X, movementPercentage);
        }
    }
    public float Y
    {
        get
        {
            return Mathf.Lerp(CurrTile.Y, nextTile.Y, movementPercentage);
        }
    }

    public Tile CurrTile { get; protected set; }
    Tile destTile;
    Tile nextTile;
    Path_AStar pathAStar;
    float movementPercentage; //goes from 0 to 1 as we move from current tile to destination tile

    float speed = 5f; //tiles per second

    Action<Character> characterChanged;

    Job myJob;

    public Character(Tile tile)
    {
        CurrTile = destTile = nextTile = tile;
    }

    void Update_DoJob(float deltaTime)
    {
        if (myJob == null)
        {
            //grab a new job from queue
            myJob = CurrTile.World.jobQueue.Dequeue();

            if (myJob != null)
            {
                //we have a job now

                destTile = myJob.Tile;
                myJob.RegisterJobCancelCallback(OnJobEnded);
                myJob.RegisterJobCompleteCallback(OnJobEnded);
            }
        }

        //are we there?
        //if(pathAStar != null && pathAStar.Length() == 1)
        if (CurrTile == destTile)
        {
            if (myJob != null)
            {
                myJob.DoWork(deltaTime);
            }

            return;
        }
    }

    public void AbandonJob()
    {
        nextTile = destTile = CurrTile;
        pathAStar = null;
        CurrTile.World.jobQueue.Enqueue(myJob);
        myJob = null;
    }

    void Update_DoMovement(float deltaTime)
    {
        if (CurrTile == destTile)
        {
            pathAStar = null;
            return;
        }

        if (nextTile == null || nextTile == CurrTile)
        {
            //get the next tile from the pathfinder
            if (pathAStar == null || pathAStar.Length() == 0)
            {
                //gen a path to dest
                pathAStar = new Path_AStar(CurrTile.World, CurrTile, destTile);
                if (pathAStar.Length() == 0)
                {
                    Debug.LogError($"Path A star returned no path to destination");
                    //TODO: job should be readded to queue if cancelled
                    AbandonJob();
                    pathAStar = null;
                    return;
                }
                nextTile = pathAStar.Dequeue(); //burning through the first tile, as that is the tile we are currently in
            }


            nextTile = pathAStar.Dequeue();

            if (nextTile == CurrTile)
            {
                Debug.Log($"Update_DoMovement - nexttile is currtile??");
            }
        }

        //get distance from a to b
        float distToTravel = Mathf.Sqrt(
            Mathf.Pow(CurrTile.X - nextTile.X, 2) +
            Mathf.Pow(CurrTile.Y - nextTile.Y, 2));

        if(nextTile.movementCost == 0)
        {
            Debug.LogError($"FIXME: a character was trying to enter an unwalkable tile");
            nextTile = null;
            pathAStar = null;
            return;
        }

        //how much distance are we travelling this update cycle
        float distThisFrame = speed / nextTile.movementCost * deltaTime;

        //how much is that in terms of percentage to our destination
        float percThisFrame = distThisFrame / distToTravel;

        // add that to overall percentage travelled
        movementPercentage += percThisFrame;

        if (movementPercentage >= 1)
        {
            //we have reached out destination

            //todo: get the next destinatnion from the pathfinding system
            //if there are no more tiles then we have truly reached the destination

            CurrTile = nextTile;
            movementPercentage = 0;
        }
    }

    public void Update(float deltaTime)
    {
        Update_DoJob(deltaTime);

        Update_DoMovement(deltaTime);

        if (characterChanged != null)
        {
            characterChanged(this);
        }
    }

    public void SetDestination(Tile tile)
    {
        if (CurrTile.IsNeighbour(tile, true) == false)
        {
            Debug.Log("Character::SetDestination -- Out destination tile is not our neighbour");
        }

        destTile = tile;
    }

    public void RegisterCharacterChanged(Action<Character> cb)
    {
        characterChanged += cb;
    }

    public void UnregisterCharacterChanged(Action<Character> cb)
    {
        characterChanged -= cb;
    }

    void OnJobEnded(Job j)
    {
        //job completed or was cancelled

        if (j != myJob)
        {
            Debug.LogError($"Character being told aobut job that isn't his. You forgot to unregister something");
            return;
        }

        myJob = null;
    }

    //SERIALIZATION

    private Character()
    {

    }

    public XmlSchema GetSchema()
    {
        return null;
    }

    public void ReadXml(XmlReader reader)
    {
    }

    public void WriteXml(XmlWriter writer)
    {
        writer.WriteAttributeString("X", CurrTile.X.ToString());
        writer.WriteAttributeString("Y", CurrTile.Y.ToString());
    }
}
