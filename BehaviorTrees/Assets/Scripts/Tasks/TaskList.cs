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
            List<Task> children = new List<Task>();
            foreach (XElement child in xelem.Elements())
                children.Add(readTask(child));
            Sequence seq = new Sequence(children, xelem.Get("invert",false));
            return seq;
        }
        else if (xelem.Name == "selector")
        {
            List<Task> children = new List<Task>();
            foreach (XElement child in xelem.Elements())
                children.Add(readTask(child));
            Selector sel = new Selector(children, xelem.Get("invert", false));
            return sel;
        }
        else if (xelem.Name == "conditional")
        {
            Conditional con = new Conditional(GameObject.Find(xelem.Get<string>("gameobject")).GetComponent<TaskInterface>(),
                                              xelem.Get<string>("condition"), xelem.Get("invert", false), xelem.Get("default", false));
            return con;
        }
        else if (xelem.Name == "action")
        {
            Action act = new Action(GameObject.Find(xelem.Get<string>("gameobject")).GetComponent<TaskInterface>(),
                                    xelem.Get<string>("action"), xelem.Get("invert", false));
            return act;
        }
        else if (xelem.Name == "movement")
        {
            Movement mov = new Movement(GameObject.Find(xelem.Get<string>("gameobject")).GetComponent<Kinematic>(),
                                        GameObject.Find(xelem.Get<string>("target")), xelem.Get("invert", false),
                                        xelem.Get("threshold", 1.5f), xelem.Get("checkdelay", 0.1f), xelem.Get("timeout", 10f));
            return mov;
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