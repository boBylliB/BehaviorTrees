using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Task
{
    public abstract bool run();
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