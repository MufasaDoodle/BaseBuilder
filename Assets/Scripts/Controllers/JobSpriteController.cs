using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JobSpriteController : MonoBehaviour
{
    //this will be bare-bones. it mostly relies on InstalledObjectSpriteController
    //this is because i don't know what the job system will actually look like :(

    InstalledObjectSpriteController iosc;
    Dictionary<Job, GameObject> jobGameObjectMap;

    // Start is called before the first frame update
    void Start()
    {
        jobGameObjectMap = new Dictionary<Job, GameObject>();
        iosc = FindObjectOfType<InstalledObjectSpriteController>();

        //Todo: does not exist
        WorldController.World.jobQueue.RegisterJobCreationCallback(OnJobCreated);
    }

    void OnJobCreated(Job j)
    {
        //TODO: more than installed objects

        GameObject job_go = new GameObject();

        jobGameObjectMap.Add(j, job_go);

        job_go.name = $"JOB_{j.JobObjectType} ({j.Tile.X},{j.Tile.Y})";
        job_go.transform.position = new Vector3(j.Tile.X, j.Tile.Y, 0);
        job_go.transform.SetParent(this.transform, true);

        SpriteRenderer sr = job_go.AddComponent<SpriteRenderer>();
        sr.sprite = iosc.GetSpriteForInstalledObject(j.JobObjectType);
        sr.color = new Color(0.5f, 1f, 0.5f, 0.25f);
        sr.sortingLayerName = "Jobs";

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
