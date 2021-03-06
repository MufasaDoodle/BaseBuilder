﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JobSpriteController : MonoBehaviour
{
    //this will be bare-bones. it mostly relies on StructureSpriteController
    //this is because i don't know what the job system will actually look like :(

    StructureSpriteController iosc;
    Dictionary<Job, GameObject> jobGameObjectMap;

    // Start is called before the first frame update
    void Start()
    {
        jobGameObjectMap = new Dictionary<Job, GameObject>();
        iosc = FindObjectOfType<StructureSpriteController>();

        //Todo: does not exist
        WorldController.World.jobQueue.RegisterJobCreationCallback(OnJobCreated);
    }

    void OnJobCreated(Job j)
    {
        //TODO: more than installed objects

        if (jobGameObjectMap.ContainsKey(j))
        {
            Debug.LogError($"OnJobCreated for at jobGO that already exists -- most likely a job being re-queued instead of created.");
            return;
        }

        GameObject job_go = new GameObject();

        jobGameObjectMap.Add(j, job_go);

        job_go.name = $"JOB_{j.JobObjectType} ({j.Tile.X},{j.Tile.Y})";
        job_go.transform.position = new Vector3(j.Tile.X, j.Tile.Y, 0);
        job_go.transform.SetParent(this.transform, true);

        SpriteRenderer sr = job_go.AddComponent<SpriteRenderer>();
        sr.sprite = iosc.GetSpriteForStructure(j.JobObjectType);
        sr.color = new Color(0.5f, 1f, 0.5f, 0.25f);
        sr.sortingLayerName = "Jobs";

        //TODO: hardcoded, fix later
        if (j.JobObjectType == "Door")
        {
            Tile northTile = j.Tile.World.GetTileAt(j.Tile.X, j.Tile.Y + 1);
            Tile southTile = j.Tile.World.GetTileAt(j.Tile.X, j.Tile.Y - 1);

            if (northTile != null && southTile != null && northTile.Structure != null && southTile.Structure != null && northTile.Structure.ObjectType.Contains("Wall") && southTile.Structure.ObjectType.Contains("Wall"))
            {
                job_go.transform.rotation = Quaternion.Euler(0, 0, 90);
            }
        }

        j.RegisterJobCompleteCallback(OnJobEnded);
        j.RegisterJobCancelCallback(OnJobEnded);
    }

    void OnJobEnded(Job j)
    {
        //TODO: Executes wether a job was finished or cancelled

        GameObject job_go = jobGameObjectMap[j];
        j.UnregisterJobCancelCallback(OnJobEnded);
        j.UnregisterJobCompleteCallback(OnJobEnded);

        Destroy(job_go);
        jobGameObjectMap.Remove(j);
    }
}
