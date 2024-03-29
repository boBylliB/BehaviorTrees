using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TaskInterface : MonoBehaviour
{
    public Dictionary<string, bool> conditions;
    public Dictionary<string, Func<bool>> actions;

    public void init()
    {
        conditions = new Dictionary<string, bool>();
        actions = new Dictionary<string, Func<bool>>();
    }
}
