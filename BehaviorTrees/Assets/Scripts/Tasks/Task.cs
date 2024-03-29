using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Task
{
    public enum TaskType
    {
        Selector, Sequence, Conditional, Action
    }

    public abstract bool run();

    protected int eventID;
    const string EVENT_NAME_PREFIX = "FinishedTask";
    public string TaskFinished
    {
        get
        {
            return EVENT_NAME_PREFIX + eventID;
        }
    }
    public Task()
    {
        // NOTE: Potentially might be better to use an enum for success/fail/running instead of an event bus
        eventID = EventBus.GetEventID();
    }
}

public class Selector : Task
{
    public List<Task> children;
    public bool invert = false;

    public override bool run()
    {
        foreach (Task child in children)
        {
            if (child.run())
                return !invert ? true : false;
        }
        return !invert ? false : true;
    }
}

public class Sequence : Task
{
    public List<Task> children;
    public bool invert = false;

    public override bool run()
    {
        foreach (Task child in children)
        {
            if (!child.run())
                return !invert ? false : true;
        }
        return !invert ? true : false;
    }
}

public class Conditional : Task
{
    public TaskInterface target;
    public string key;
    public bool invert = false;
    public bool defaultValue = false;

    public override bool run()
    {
        bool output;
        if (!target.conditions.TryGetValue(key, out output))
            return !invert ? defaultValue : !defaultValue;
        return !invert ? output : !output;
    }
}

public class Action : Task
{
    public TaskInterface target;
    public string key;
    public bool invert = false;

    public override bool run()
    {
        Func<bool> output;
        // In this case, if the action wasn't found, return the normal "failure" state
        if (!target.actions.TryGetValue(key, out output))
            return !invert;
        return !invert ? output() : !output();
    }
}

public class Wait : Task
{
    float mTimeToWait;

    public Wait(float time)
    {
        mTimeToWait = time;
    }

    public override bool run()
    {
        EventBus.ScheduleTrigger(TaskFinished, mTimeToWait);
        return true;
    }
}