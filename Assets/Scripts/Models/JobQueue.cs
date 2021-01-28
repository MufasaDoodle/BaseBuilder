using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class JobQueue
{
    protected Queue<Job> jobQueue;

    Action<Job> jobCreated;

    public JobQueue()
    {
        jobQueue = new Queue<Job>();
    }

    public void Enqueue(Job j)
    {
        jobQueue.Enqueue(j);

        //todo: call callbacks

        if(jobCreated != null)
        {
            jobCreated(j);
        }
    }

    public Job Dequeue()
    {
        if(jobQueue.Count == 0)
        {
            return null;
        }

        return jobQueue.Dequeue();
    }

    public void RegisterJobCreationCallback(Action<Job> cb)
    {
        jobCreated += cb;
    }

    public void UnregisterJobCreationCallback(Action<Job> cb)
    {
        jobCreated -= cb;
    }
}
