using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public TaskList sourceTaskList;

    public float taskBlockHeight = 200f;
    public float verticalBuffer = 120f;
    public float taskBlockWidth = 300f;
    public float horizontalBuffer = 20f;

    public GameObject taskBlockPrefab;
    public GameObject taskBlockParent;
    public List<TaskBlock> taskBlocks = new List<TaskBlock>();

    public GameObject arrowPrefab;
    public GameObject arrowParent;

    public List<TaskBlock> selectedBlocks = new List<TaskBlock>();

    public bool editing = false;

    public List<GameObject> runtimeUI;
    public List<GameObject> editorUI;

    public TaskBlock selectedBlock = null;

    public ScrollRect scrollView;

    private int taskBlockID = 0;

    private List<List<TaskBlock>> initialTaskBlockLayout;

    void Start()
    {
        initialTaskBlockLayout = new List<List<TaskBlock>>();
        // Grab task info from the xml document
        getTasks();
        // Display the current tasks with automatic spacing
        DisplayBlocks();
    }

    void Update()
    {
        // Only run editor logic if the editor is enabled
        if (editing)
        {
            // Check for selected task blocks
            selectBlock();
            if (Input.GetMouseButtonDown(0))
            {
                if (selectedBlock != null)
                {
                    scrollView.enabled = false;
                    // Tell the correct task block that it was selected and clicked
                    // This may mean the block has to move, or it may mean the children box was clicked,
                    // but that specific logic is handled by the block itself
                    triggerBlockSelect();
                }
            }
            if (Input.GetMouseButtonUp(0))
                scrollView.enabled = true;
        }
    }

    public void deleteBlock(TaskBlock block)
    {
        removeFromSelectedBlocks(block);
        taskBlocks.Remove(block);
        Destroy(block.gameObject);
    }

    public void addToSelectedBlocks(TaskBlock block)
    {
        selectedBlocks.Add(block);
    }
    public void removeFromSelectedBlocks(TaskBlock block)
    {
        selectedBlocks.Remove(block);
    }
    public void selectBlock()
    {
        selectedBlock = null;
        foreach (TaskBlock current in selectedBlocks)
        {
            if (selectedBlock == null || current.taskID > selectedBlock.taskID)
            {
                selectedBlock = current;
            }
        }
    }
    private void triggerBlockSelect()
    {
        selectedBlock.triggerSelect();
    }

    public void toggleEditor()
    {
        editing = !editing;
        foreach (GameObject UIElement in runtimeUI)
            UIElement.SetActive(!editing);
        foreach (GameObject UIElement in editorUI)
            UIElement.SetActive(editing);

        if (!editing)
            outputTasks();
    }
    public void outputTasks()
    {
        // Now we do the opposite of startup, and rebuild the xml file from our current editor state
        List<TaskBlock> used = new List<TaskBlock>();
        sourceTaskList.tasks.Clear();
        foreach (TaskBlock block in taskBlocks)
        {
            if (used.Contains(block))
                continue;

            sourceTaskList.tasks.Add(convertBlockToTask(block, used));
            used.Add(block);
        }
    }
    private Task convertBlockToTask(TaskBlock block, List<TaskBlock> used)
    {
        List<Task> children;
        Debug.Log($"Task: {block.typeLabel.text}");
        switch (block.taskType)
        {
            case Task.TaskType.Selector:
                children = new List<Task>();
                Debug.Log("Enter level");
                foreach (TaskBlock childBlock in block.children)
                {
                    children.Add(convertBlockToTask(childBlock, used));
                    used.Add(childBlock);
                }
                Debug.Log("Exit Level");
                Selector sel = new Selector(children, block.inverted);
                return sel;
            case Task.TaskType.Sequence:
                children = new List<Task>();
                Debug.Log("Enter level");
                foreach (TaskBlock childBlock in block.children)
                {
                    children.Add(convertBlockToTask(childBlock, used));
                    used.Add(childBlock);
                }
                Debug.Log("Exit Level");
                Sequence seq = new Sequence(children, block.inverted);
                return seq;
            case Task.TaskType.Conditional:
                Conditional con = new Conditional(GameObject.Find(block.target).GetComponent<TaskInterface>(),
                                              block.condition, block.inverted, false);
                return con;
            case Task.TaskType.Action:
                Action act = new Action(GameObject.Find(block.target).GetComponent<TaskInterface>(),
                                    block.action, block.inverted);
                return act;
            case Task.TaskType.Movement:
                Movement mov = new Movement(GameObject.Find(block.kinematic).GetComponent<Kinematic>(),
                                        GameObject.Find(block.target), block.inverted,
                                        1.5f, 0.1f, 10f);
                return mov;
            default:
                throw new KeyNotFoundException($"Could not find task type {block.taskType}");
        }
    }

    public void getTasks()
    {
        // Since we have a TaskList, we can just use the TaskList instead of having to parse the document again
        // All we have to do now is flatten the list of tasks generated by the TaskList, and convert into task blocks
        initialTaskBlockLayout.Add(new List<TaskBlock>());
        foreach (Task task in sourceTaskList.tasks)
        {
            TaskBlock taskBlock = Object.Instantiate(taskBlockPrefab, taskBlockParent.transform).GetComponent<TaskBlock>();
            taskBlocks.Add(taskBlock);
            initialTaskBlockLayout[0].Add(taskBlock);
            List<TaskBlock> children = new List<TaskBlock>();
            switch (task.type)
            {
                case Task.TaskType.Selector:
                    Selector sel = task as Selector;
                    children.AddRange(createChildTaskBlocks(sel, 1, taskBlock));
                    taskBlock.Initialize(taskBlockID++, sel.type, sel.invert, null, null, null, null, children);
                    break;
                case Task.TaskType.Sequence:
                    Sequence seq = task as Sequence;
                    children.AddRange(createChildTaskBlocks(seq, 1, taskBlock));
                    taskBlock.Initialize(taskBlockID++, seq.type, seq.invert, null, null, null, null, children);
                    break;
                case Task.TaskType.Conditional:
                    Conditional con = task as Conditional;
                    taskBlock.Initialize(taskBlockID++, con.type, con.invert, null, con.target.name, con.key, null, children);
                    break;
                case Task.TaskType.Action:
                    Action act = task as Action;
                    taskBlock.Initialize(taskBlockID++, act.type, act.invert, null, act.target.name, null, act.key, children);
                    break;
                case Task.TaskType.Movement:
                    Movement mov = task as Movement;
                    taskBlock.Initialize(taskBlockID++, mov.type, mov.invert, mov.focus.name, mov.goal.name, null, null, children);
                    break;
            }
        }
    }
    public void DisplayBlocks()
    {
        // Iterate through all task blocks, and apply automatic spacing
        // Each iteration depth is a row of task blocks
        // Blocks are then spaced horizontally within the row to fit all of the same level into a single row
        // The initial layout should have been assembled when getting the current task list
        int height = initialTaskBlockLayout.Count;
        List<int> widths = new List<int>();
        int maxWidth = 0;
        foreach (List<TaskBlock> taskBlockList in initialTaskBlockLayout)
        {
            widths.Add(taskBlockList.Count);
            if (taskBlockList.Count > maxWidth) maxWidth = taskBlockList.Count;
        }
        // Set bounding box
        taskBlockParent.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, maxWidth * (taskBlockWidth + horizontalBuffer));
        taskBlockParent.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, (height + 1) * (taskBlockHeight + verticalBuffer));
        // Set spacing
        for (int row = 0; row < height; row++)
        {
            int width = widths[row];
            for (int col = 0; col < width; col++)
            {
                float xPlacement = ((maxWidth + 0.5f) * (taskBlockWidth + horizontalBuffer) / 2) - (taskBlockWidth + horizontalBuffer) * (((width - 1) / 2 - col) + 0.5f * ((width - 1) % 2));
                float yPlacement = ((height) * (taskBlockHeight + verticalBuffer) / 2) - row * (taskBlockHeight + verticalBuffer);
                initialTaskBlockLayout[row][col].transform.position = new Vector3(xPlacement, yPlacement);
            }
        }
    }

    public Arrow createArrow()
    {
        GameObject arrowObject = Instantiate(arrowPrefab, arrowParent.transform);
        return arrowObject.GetComponent<Arrow>();
    }

    private List<TaskBlock> createChildTaskBlocks(Selector parent, int level, TaskBlock parentBlock)
    {
        List<TaskBlock> output = new List<TaskBlock>();
        if (initialTaskBlockLayout.Count < level + 1) initialTaskBlockLayout.Add(new List<TaskBlock>());
        foreach (Task task in parent.children)
        {
            TaskBlock taskBlock = Object.Instantiate(taskBlockPrefab, taskBlockParent.transform).GetComponent<TaskBlock>();
            taskBlock.parent = parentBlock;
            output.Add(taskBlock);
            taskBlocks.Add(taskBlock);
            initialTaskBlockLayout[level].Add(taskBlock);
            List<TaskBlock> children = new List<TaskBlock>();
            switch (task.type)
            {
                case Task.TaskType.Selector:
                    Selector sel = task as Selector;
                    children.AddRange(createChildTaskBlocks(sel, level + 1, taskBlock));
                    taskBlock.Initialize(taskBlockID++, sel.type, sel.invert, null, null, null, null, children);
                    break;
                case Task.TaskType.Sequence:
                    Sequence seq = task as Sequence;
                    children.AddRange(createChildTaskBlocks(seq, level + 1, taskBlock));
                    taskBlock.Initialize(taskBlockID++, seq.type, seq.invert, null, null, null, null, children);
                    break;
                case Task.TaskType.Conditional:
                    Conditional con = task as Conditional;
                    taskBlock.Initialize(taskBlockID++, con.type, con.invert, null, con.target.name, con.key, null, children);
                    break;
                case Task.TaskType.Action:
                    Action act = task as Action;
                    taskBlock.Initialize(taskBlockID++, act.type, act.invert, null, act.target.name, null, act.key, children);
                    break;
                case Task.TaskType.Movement:
                    Movement mov = task as Movement;
                    taskBlock.Initialize(taskBlockID++, mov.type, mov.invert, mov.focus.name, mov.goal.name, null, null, children);
                    break;
            }
        }
        return output;
    }
    private List<TaskBlock> createChildTaskBlocks(Sequence parent, int level, TaskBlock parentBlock)
    {
        List<TaskBlock> output = new List<TaskBlock>();
        if (initialTaskBlockLayout.Count < level + 1) initialTaskBlockLayout.Add(new List<TaskBlock>());
        foreach (Task task in parent.children)
        {
            TaskBlock taskBlock = Object.Instantiate(taskBlockPrefab, taskBlockParent.transform).GetComponent<TaskBlock>();
            taskBlock.parent = parentBlock;
            output.Add(taskBlock);
            taskBlocks.Add(taskBlock);
            initialTaskBlockLayout[level].Add(taskBlock);
            List<TaskBlock> children = new List<TaskBlock>();
            switch (task.type)
            {
                case Task.TaskType.Selector:
                    Selector sel = task as Selector;
                    children.AddRange(createChildTaskBlocks(sel, level + 1, taskBlock));
                    taskBlock.Initialize(taskBlockID++, sel.type, sel.invert, null, null, null, null, children);
                    break;
                case Task.TaskType.Sequence:
                    Sequence seq = task as Sequence;
                    children.AddRange(createChildTaskBlocks(seq, level + 1, taskBlock));
                    taskBlock.Initialize(taskBlockID++, seq.type, seq.invert, null, null, null, null, children);
                    break;
                case Task.TaskType.Conditional:
                    Conditional con = task as Conditional;
                    taskBlock.Initialize(taskBlockID++, con.type, con.invert, null, con.target.name, con.key, null, children);
                    break;
                case Task.TaskType.Action:
                    Action act = task as Action;
                    taskBlock.Initialize(taskBlockID++, act.type, act.invert, null, act.target.name, null, act.key, children);
                    break;
                case Task.TaskType.Movement:
                    Movement mov = task as Movement;
                    taskBlock.Initialize(taskBlockID++, mov.type, mov.invert, mov.focus.name, mov.goal.name, null, null, children);
                    break;
            }
        }
        return output;
    }
}
