using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Task
{
    public enum TaskType
    {
        Selector, Sequence, Conditional, Action, Movement, // Public-facing tasks
        Wait, WaitUntil, Evaluate                          // Internal utility tasks
    }
    public TaskType type;
    public bool invert;

    public abstract void run();
    public bool succeeded;

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

    private Task currentTask;
    private int taskIdx;

    public Selector(List<Task> taskList, bool invert = false)
    {
        children = taskList;
        this.invert = invert;
        type = TaskType.Selector;
    }

    public override void run()
    {
        currentTask = children[taskIdx];
        EventBus.StartListening(currentTask.TaskFinished, OnChildTaskFinished);
        currentTask.run();
    }

    public void OnChildTaskFinished()
    {
        if (invert != currentTask.succeeded)
        {
            succeeded = true;
            EventBus.TriggerEvent(TaskFinished);
        }
        else
        {
            EventBus.StopListening(currentTask.TaskFinished, OnChildTaskFinished);
            taskIdx++;
            if (taskIdx < children.Count)
                this.run();
            else
            {
                succeeded = false;
                EventBus.TriggerEvent(TaskFinished);
            }
        }
    }
}

public class Sequence : Task
{
    public List<Task> children;

    private Task currentTask;
    private int taskIdx;

    public Sequence(List<Task> taskList, bool invert = false)
    {
        children = taskList;
        this.invert = invert;
        type = TaskType.Sequence;
    }

    public override void run()
    {
        currentTask = children[taskIdx];
        EventBus.StartListening(currentTask.TaskFinished, OnChildTaskFinished);
        currentTask.run();
    }

    public void OnChildTaskFinished()
    {
        if (invert != currentTask.succeeded)
        {
            EventBus.StopListening(currentTask.TaskFinished, OnChildTaskFinished);
            taskIdx++;
            if (taskIdx < children.Count)
                this.run();
            else
            {
                succeeded = true;
                EventBus.TriggerEvent(TaskFinished);
            }
        }
        else
        {
            succeeded = false;
            EventBus.TriggerEvent(TaskFinished);
        }
    }
}

public class Conditional : Task
{
    public TaskInterface target;
    public string key;
    public bool defaultValue;

    public Conditional(TaskInterface target, string key, bool invert = false, bool defaultValue = false)
    {
        this.target = target;
        this.key = key;
        this.defaultValue = defaultValue;
        this.invert = invert;
        type = TaskType.Conditional;
    }

    public override void run()
    {
        bool output;
        if (!target.conditions.TryGetValue(key, out output))
            succeeded = invert != defaultValue;
        else
            succeeded = invert != output;
        EventBus.TriggerEvent(TaskFinished);
    }
}

public class Action : Task
{
    public TaskInterface target;
    public string key;

    public Action(TaskInterface target, string key, bool invert = false)
    {
        this.target = target;
        this.key = key;
        this.invert = invert;
        type = TaskType.Action;
    }

    public override void run()
    {
        Func<bool> output;
        // In this case, if the action wasn't found, return the normal "failure" state
        if (!target.actions.TryGetValue(key, out output))
            succeeded = !invert;
        else
            succeeded = invert != output();
        EventBus.TriggerEvent(TaskFinished);
    }
}

public class Movement : Task
{
    public Kinematic focus;
    public GameObject goal;
    public float threshold;
    public float checkDelay;
    public float timeout;

    private WaitUntil<GameObject> subTask;

    public Movement(Kinematic focus, GameObject goal, bool invert = false, float threshold = 1.5f, float checkDelay = 0.1f, float timeout = 10f)
    {
        this.focus = focus;
        this.goal = goal;
        this.invert = invert;
        this.threshold = threshold;
        this.checkDelay = checkDelay;
        this.timeout = timeout;
        type = TaskType.Movement;
    }

    public override void run()
    {
        subTask = new WaitUntil<GameObject>(focus.gameObject, goal, checkThreshold, false, checkDelay, timeout);
        EventBus.StartListening(subTask.TaskFinished, OnSubTaskFinished);
        focus.myTarget = goal;
        subTask.run();
    }

    public bool checkThreshold(GameObject focus, GameObject target)
    {
        return Vector3.Distance(focus.transform.position, target.transform.position) <= threshold;
    }

    public void OnSubTaskFinished()
    {
        EventBus.StopListening(subTask.TaskFinished, OnSubTaskFinished);
        if (invert != subTask.succeeded)
        {
            succeeded = true;
        }
        else
        {
            succeeded = false;
        }
        EventBus.TriggerEvent(TaskFinished);
    }
}

public class Wait : Task
{
    public float mTimeToWait;

    public Wait(float time)
    {
        mTimeToWait = time;
        type = TaskType.Wait;
    }

    public override void run()
    {
        succeeded = true;
        EventBus.ScheduleTrigger(TaskFinished, mTimeToWait);
    }
}

public class WaitUntil<T> : Task
{
    // This waits until the "comparison" function returns the desired result (true, or false if inverted)
    public float timeout;

    private Evaluate<T> subTask;
    private Wait delayTask;
    private float startTime;
    // NOTE: This implementation is frankly TERRIBLE and tends to completely lock up Unity if we check the state too many times
    // I've found that a minimum delay of 1 second avoids this, but obviously this isn't an ideal solution
    private float minDelay = 1f;

    public WaitUntil(T valueToCheck, T goalValue, Func<T, T, bool> comparison, bool invert = false, float checkDelay = 0.1f, float timeout = 10f)
    {
        subTask = new Evaluate<T>(valueToCheck, goalValue, comparison, invert);
        this.invert = invert;
        delayTask = new Wait(checkDelay > minDelay ? checkDelay : minDelay);
        this.timeout = timeout;
        type = TaskType.WaitUntil;
    }

    public override void run()
    {
        EventBus.StopListening(delayTask.TaskFinished, this.run);
        EventBus.StartListening(subTask.TaskFinished, OnSubTaskFinished);
        startTime = Time.time;
        subTask.run();
    }

    public void OnSubTaskFinished()
    {
        if (invert != subTask.succeeded)
        {
            EventBus.StopListening(subTask.TaskFinished, OnSubTaskFinished);
            succeeded = true;
            EventBus.TriggerEvent(TaskFinished);
        }
        else
        {
            if (Time.time - startTime >= timeout)
            {
                succeeded = false;
                EventBus.TriggerEvent(TaskFinished);
            }
            else
            {
                EventBus.StartListening(delayTask.TaskFinished, this.run);
                delayTask.run();
            }
        }
    }
}

public class Evaluate<T> : Task
{
    // Returns the value of the "comparison" function when passed the two given inputs
    public T focus;
    public T target;
    public Func<T, T, bool> comparison;

    public Evaluate(T valueToCheck, T goalValue, Func<T, T, bool> comparison, bool invert = false)
    {
        focus = valueToCheck;
        target = goalValue;
        this.comparison = comparison;
        this.invert = invert;
        type = TaskType.Evaluate;
    }

    public override void run()
    {
        succeeded = invert != comparison(focus, target);
        EventBus.TriggerEvent(TaskFinished);
    }
}