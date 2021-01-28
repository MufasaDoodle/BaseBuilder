using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BuildModeController : MonoBehaviour
{
    bool isBuildModeObjects = false;
    TileType buildModeTile = TileType.Floor;
    string buildModeObjectType;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void SetMode_BuildFloor()
    {
        isBuildModeObjects = false;
        buildModeTile = TileType.Floor;
    }
    public void SetMode_Bulldoze()
    {
        isBuildModeObjects = false;
        buildModeTile = TileType.Empty;
    }

    public void SetMode_BuildInstalledObject(string objectType)
    {
        isBuildModeObjects = true;
        buildModeObjectType = objectType;
    }

    public void DoBuild(Tile t)
    {
        if (isBuildModeObjects)
        {
            //installed object

            //this instantly build objects
            //WorldController.World.PlaceInstalledObject(buildModeObjectType, t);

            //check if object placement is valid in that tile
            //run validplacement function

            string objectType = buildModeObjectType;

            if (WorldController.World.IsInstalledObjectPlacementValid(objectType, t) && t.pendingInstalledObjectJob == null)
            {
                //this tile is valid for this object
                //create job for it to be built
                Job j = new Job(t, objectType ,(theJob) =>
                {
                    WorldController.World.PlaceInstalledObject(objectType, theJob.Tile);
                    t.pendingInstalledObjectJob = null;
                });

                //TODO this is not good, don't manually set flags that prevent conflicts, too easy to mess up
                t.pendingInstalledObjectJob = j;
                j.RegisterJobCancelCallback((theJob) => { theJob.Tile.pendingInstalledObjectJob = null; });

                //add job to queue
                WorldController.World.jobQueue.Enqueue(j);
            }
        }
        else
        {
            //tile changing mode
            t.Type = buildModeTile;
        }
    }
}