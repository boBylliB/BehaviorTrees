using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;
using UnityEngine.UI;

public class TaskBlock : MonoBehaviour
{
    public int taskID;
    public Task.TaskType taskType;
    public bool inverted;

    public GameObject target;
    public string condition;
    public string action;

    public List<TaskBlock> children;

    public Text typeLabel;
    public Toggle invertToggle;
    public Dropdown targetDropdown;
    public Dropdown conditionDropdown;
    public Dropdown actionDropdown;
    public BoxCollider2D childrenBox;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
