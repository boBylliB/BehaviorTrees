using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    public bool open;
    public bool locked;

    public Vector3 forceDirection;
    public float forceValue;

    public GameObject openObject;
    public GameObject closedObject;

    public TaskInterface taskInterface;

    private bool flying = false;

    private void Start()
    {
        taskInterface.init();
        taskInterface.conditions.Add("open", false);
        taskInterface.conditions.Add("locked", false);

        taskInterface.actions.Add("open", openDoor);
        taskInterface.actions.Add("barge", sendFlying);
    }

    public void setOpen(bool value)
    {
        if (open != value && !locked && !flying)
        {
            open = value;
            taskInterface.conditions["open"] = open;
            openObject.SetActive(open);
            closedObject.SetActive(!open);
        }
    }
    public void setLocked(bool value)
    {
        if (locked != value && !flying)
        {
            locked = value;
            taskInterface.conditions["locked"] = locked;
        }
    }
    public bool sendFlying()
    {
        if (open || flying)
            return false;
        else
        {
            closedObject.GetComponent<Rigidbody>().AddForce(forceDirection.normalized * forceValue);
            flying = true;
            return true;
        }
    }
    public bool openDoor()
    {
        setOpen(true);
        return open;
    }
}
