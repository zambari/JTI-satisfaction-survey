using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskGroupController
{
    public TaskGroupController (System.Action<int> startAction, System.Action<int> endAction)
    {
		this.startAction = startAction;
		this.endAction = endAction;
    }

    public TaskGroupController (System.Action<int> endAction)
    {
        this.endAction = endAction;
    }

    private System.Action<int> startAction;
    private System.Action<int> endAction;

    private Dictionary<int, int> tasks = new Dictionary<int, int>();

    public void AddTask(int number)
    {
        if (!tasks.ContainsKey(number))
            tasks.Add(number, 0);

        if (tasks[number] == 0)
        {
            if (startAction != null)
                startAction.Invoke(number);
        }

        tasks[number]++;
    }

    public void RemoveTask(int number)
    {
        if (tasks[number] > 0)
        {
            tasks[number]--;
            if (tasks[number] == 0)
            {
                if (endAction != null)
                    endAction.Invoke(number);
            }
        }
    }

	public void ClearActions()
	{
		startAction = null;
		endAction = null;
	}

    public int TaskOperationsCount(int number)
    {
        return tasks[number];
    }

    public bool TaskInProgress(int number)
    {
        if (tasks.ContainsKey(number))
            return tasks[number] > 0;

        return false;
    }
}