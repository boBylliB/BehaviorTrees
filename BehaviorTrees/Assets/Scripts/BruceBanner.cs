using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BruceBanner : Kinematic
{
    public GameObject interior;
    public GameObject exterior;
    public GameObject frontOfDoor;

    public TaskList taskList;
    public TaskInterface taskInterface;

    public bool running = false;

    protected Arrive myMoveType;

    private void Start()
    {
        taskInterface.init();
        taskInterface.actions.Add("moveIntoRoom", moveIntoRoom);
        taskInterface.actions.Add("moveToDoor", moveToDoor);

        myMoveType = new Arrive();
        myMoveType.character = this;
        myMoveType.target = exterior;
    }
    protected override void Update()
    {
        if (!running && Input.GetKeyDown("g"))
        {
            running = true;
            taskList.run();
        }
        if (Input.GetKeyDown("r"))
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        steeringUpdate = new SteeringOutput();
        steeringUpdate.linear = myMoveType.getSteering().linear;
        base.Update();
    }

    public bool moveIntoRoom()
    {
        myMoveType.target = interior;
        return true;
    }
    public bool moveToDoor()
    {
        myMoveType.target = frontOfDoor;
        return true;
    }
}
