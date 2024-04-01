using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.UI;

public class TaskBlock : MonoBehaviour
{
    public int taskID;
    public Task.TaskType taskType;
    public bool inverted;

    public string kinematic;
    public string target;
    public string condition;
    public string action;

    public List<TaskBlock> children;

    public Text typeLabel;
    public Toggle invertToggle;
    public Dropdown kinematicDropdown;
    public Dropdown targetDropdown;
    public Dropdown conditionDropdown;
    public Dropdown actionDropdown;
    public BoxCollider2D childrenBox;

    public TextAsset sceneInfo;

    private bool initialized = false;

    public void Initialize(int taskID, Task.TaskType taskType, bool inverted, string kinematic, string target, string condition, string action, List<TaskBlock> children)
    {
        this.taskID = taskID;
        this.taskType = taskType;
        this.inverted = inverted;
        this.kinematic = kinematic;
        this.target = target;
        this.condition = condition;
        this.action = action;
        this.children = children;

        invertToggle.isOn = this.inverted;
        switch (this.taskType)
        {
            case Task.TaskType.Selector:
                typeLabel.text = "Selector";
                kinematicDropdown.gameObject.SetActive(false);
                targetDropdown.gameObject.SetActive(false);
                conditionDropdown.gameObject.SetActive(false);
                actionDropdown.gameObject.SetActive(false);
                childrenBox.gameObject.SetActive(true);
                break;
            case Task.TaskType.Sequence:
                typeLabel.text = "Sequence";
                kinematicDropdown.gameObject.SetActive(false);
                targetDropdown.gameObject.SetActive(false);
                conditionDropdown.gameObject.SetActive(false);
                actionDropdown.gameObject.SetActive(false);
                childrenBox.gameObject.SetActive(true);
                break;
            case Task.TaskType.Conditional:
                typeLabel.text = "Conditional";
                kinematicDropdown.gameObject.SetActive(false);
                targetDropdown.gameObject.SetActive(true);
                conditionDropdown.gameObject.SetActive(true);
                actionDropdown.gameObject.SetActive(false);
                childrenBox.gameObject.SetActive(false);

                fillTargetOptions();
                fillConditionalActionOptions();
                break;
            case Task.TaskType.Action:
                typeLabel.text = "Action";
                kinematicDropdown.gameObject.SetActive(false);
                targetDropdown.gameObject.SetActive(true);
                conditionDropdown.gameObject.SetActive(false);
                actionDropdown.gameObject.SetActive(true);
                childrenBox.gameObject.SetActive(false);

                fillTargetOptions();
                fillConditionalActionOptions();
                break;
            case Task.TaskType.Movement:
                typeLabel.text = "Movement";
                kinematicDropdown.gameObject.SetActive(true);
                targetDropdown.gameObject.SetActive(true);
                conditionDropdown.gameObject.SetActive(false);
                actionDropdown.gameObject.SetActive(false);
                childrenBox.gameObject.SetActive(false);

                fillKinematicOptions();
                fillTargetOptions();
                break;
        }

        initialized = true;
    }

    public void Update()
    {
        if (!initialized) return;


    }

    public void kinematicSelect()
    {
        kinematic = kinematicDropdown.options[kinematicDropdown.value].text;
    }
    public void targetSelect()
    {
        target = targetDropdown.options[targetDropdown.value].text;
        fillConditionalActionOptions();
    }
    public void conditionSelect()
    {
        condition = conditionDropdown.options[conditionDropdown.value].text;
    }
    public void actionSelect()
    {
        action = actionDropdown.options[actionDropdown.value].text;
    }

    private void fillKinematicOptions()
    {
        XDocument xdoc = XDocument.Parse(sceneInfo.text);

        List<string> kinematicOptions = new List<string>();

        foreach (XElement xelem in xdoc.Root.Elements("interactable"))
        {
            if (xelem.Get("kinematic", false))
            {
                kinematicOptions.Add(xelem.Get<string>("gameobject"));
            }
        }

        if (kinematicOptions.Count > 0) kinematicDropdown.AddOptions(kinematicOptions);

        for (int idx = 0; idx < kinematicOptions.Count; idx++)
        {
            if (kinematicOptions[idx] == kinematic)
            {
                kinematicDropdown.value = idx;
                break;
            }
        }
    }
    private void fillTargetOptions()
    {
        XDocument xdoc = XDocument.Parse(sceneInfo.text);

        List<string> targetOptions = new List<string>();

        if (taskType == Task.TaskType.Movement)
        {
            foreach (XElement xelem in xdoc.Root.Elements("target"))
            {
                targetOptions.Add(xelem.Get<string>("gameobject"));
            }
        }
        else
        {
            foreach (XElement xelem in xdoc.Root.Elements("interactable"))
            {
                if (xelem.Get("taskinterface", false))
                    targetOptions.Add(xelem.Get<string>("gameobject"));
            }
        }

        if (targetOptions.Count > 0) targetDropdown.AddOptions(targetOptions);

        for (int idx = 0; idx < targetOptions.Count; idx++)
        {
            if (targetOptions[idx] == target)
            {
                targetDropdown.value = idx;
                break;
            }
        }
    }
    private void fillConditionalActionOptions()
    {
        XDocument xdoc = XDocument.Parse(sceneInfo.text);

        List<string> conditionOptions = new List<string>();
        List<string> actionOptions = new List<string>();

        foreach (XElement parent in xdoc.Root.Elements("interactable"))
        {
            if (parent.Get<string>("gameobject") == targetDropdown.options[targetDropdown.value].text)
            {
                foreach (XElement xelem in parent.Elements("condition"))
                    conditionOptions.Add(xelem.Get<string>("name"));
                foreach (XElement xelem in parent.Elements("action"))
                    actionOptions.Add(xelem.Get<string>("name"));
            }
        }

        if (conditionOptions.Count > 0) conditionDropdown.AddOptions(conditionOptions);
        if (actionOptions.Count > 0) actionDropdown.AddOptions(actionOptions);

        for (int idx = 0; idx < conditionOptions.Count; idx++)
        {
            if (conditionOptions[idx] == condition)
            {
                conditionDropdown.value = idx;
                break;
            }
        }
        for (int idx = 0; idx < actionOptions.Count; idx++)
        {
            if (actionOptions[idx] == action)
            {
                actionDropdown.value = idx;
                break;
            }
        }
    }
}
