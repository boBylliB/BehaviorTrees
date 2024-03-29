using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Linq;
using UnityEngine;

public class TaskList : MonoBehaviour
{
    public List<Task> tasks;
    public TextAsset taskFile;

    public void Start()
    {
        XDocument xdoc = XDocument.Parse(taskFile.text);
        tasks = new List<Task>();
        foreach (XElement xelem in xdoc.Root.Elements())
        {
            tasks.Add(readTask(xelem));
        }
    }

    public void run()
    {
        foreach (Task task in tasks)
        {
            task.run();
        }
    }

    private Task readTask(XElement xelem)
    {
        if (xelem.Name == "sequence")
        {
            Sequence seq = new Sequence();
            seq.children = new List<Task>();
            foreach (XElement child in xelem.Elements())
                seq.children.Add(readTask(child));
            return seq;
        }
        else if (xelem.Name == "selector")
        {
            Selector sel = new Selector();
            sel.children = new List<Task>();
            foreach (XElement child in xelem.Elements())
                sel.children.Add(readTask(child));
            return sel;
        }
        else if (xelem.Name == "conditional")
        {
            Conditional con = new Conditional();
            con.invert = xelem.Get("invert", false);
            con.defaultValue = xelem.Get("default", false);
            con.key = xelem.Get<string>("condition");
            con.target = GameObject.Find(xelem.Get<string>("gameobject")).GetComponent<TaskInterface>();
            return con;
        }
        else if (xelem.Name == "action")
        {
            Action act = new Action();
            act.invert = xelem.Get("invert", false);
            act.key = xelem.Get<string>("action");
            act.target = GameObject.Find(xelem.Get<string>("gameobject")).GetComponent<TaskInterface>();
            return act;
        }
        else
        {
            throw new KeyNotFoundException($"Unknown task name {xelem.Name}");
        }
    }
}

static class Helper
{
    public static T Get<T>(this XElement xelem, string attribute, T defaultT = default)
    {
        XAttribute a = xelem.Attribute(attribute);
        return a == null ? defaultT : (T)TypeDescriptor.GetConverter(typeof(T)).ConvertFromInvariantString(a.Value);
    }
}