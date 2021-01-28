using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Character
{
    public float X
    {
        get
        {
            return Mathf.Lerp(CurrTile.X, destTile.X, movementPercentage);
        }
    }
    public float Y
    {
        get
        {
            return Mathf.Lerp(CurrTile.Y, destTile.Y, movementPercentage);
        }
    }

    public Tile CurrTile { get; protected set; }
    Tile destTile;
    float movementPercentage; //goes from 0 to 1 as we move from current tile to destination tile

    float speed = 2f; //tiles per second

    Action<Character> characterChanged;

    Job myJob;

    public Character(Tile tile)
    {
        CurrTile = destTile = tile;
    }

    public void Update(float deltaTime)
    {
        if(myJob == null)
        {
            //grab a new job from queue
            myJob = CurrTile.World.jobQueue.Dequeue();

            if(myJob != null)
            {
                //we have a job now
                destTile = myJob.Tile;
                myJob.RegisterJobCancelCallback(OnJobEnded);
                myJob.RegisterJobCompleteCallback(OnJobEnded);
            }
        }

        //are we there?
        if(CurrTile == destTile)
        {
            if(myJob != null)
            {
                myJob.DoWork(deltaTime);
            }

            return;
        }

        //get distance from a to b
        float distToTravel = Mathf.Sqrt(Mathf.Pow(CurrTile.X - destTile.X, 2) + Mathf.Pow(CurrTile.Y - destTile.Y, 2));

        //how much distance are we travelling this update cycle
        float distThisFrame = speed * deltaTime;

        //how much is that in terms of percentage to our destination
        float percThisFrame = distThisFrame / distToTravel;

        // add that to overall percentage travelled
        movementPercentage += percThisFrame;

        if(movementPercentage >= 1)
        {
            //we have reached out destination
            CurrTile = destTile;
            movementPercentage = 0;
        }

        if(characterChanged != null)
        {
            characterChanged(this);
        }
    }

    public void SetDestination(Tile tile)
    {
        if (CurrTile.IsNeighbour(tile,true) == false)
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

        if(j != myJob)
        {
            Debug.LogError($"Character being told aobut job that isn't his. You forgot to unregister something");
            return;
        }

        myJob = null;
    }
}
