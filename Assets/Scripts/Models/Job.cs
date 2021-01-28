using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Job
{
    //This class holds info for a queued up job, which might include
    //things like placing installedobjects, moving stored inventory,
    //working at a desk, and maybe even fighting enemies

    public Tile Tile { get; protected set; }
    float jobTime;

    //todo: temporary
    public string JobObjectType { get; protected set; }

    Action<Job> JobComplete;
    Action<Job> JobCancelled;

    public Job(Tile tile, string jobObjectType, Action<Job> JobComplete, float jobTime = 0.1f)
    {
        this.Tile = tile;
        this.JobObjectType = jobObjectType;
        this.jobTime = jobTime;
        this.JobComplete += JobComplete;
    }

    public void DoWork(float workTime)
    {
        jobTime -= workTime;

        if(jobTime <= 0)
        {
            if (JobComplete != null)
            {
                JobComplete(this);
            }
        }
    }

    public void CancelJob()
    {
        if (JobCancelled != null)
        {
            JobCancelled(this);
        }
    }

    public void RegisterJobCompleteCallback(Action<Job> cb)
    {
        JobComplete += cb;
    }

    public void RegisterJobCancelCallback(Action<Job> cb)
    {
        JobCancelled += cb;
    }

    public void UnregisterJobCompleteCallback(Action<Job> cb)
    {
        JobComplete -= cb;
    }

    public void UnregisterJobCancelCallback(Action<Job> cb)
    {
        JobCancelled -= cb;
    }
}