using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BruceBanner : Kinematic
{
    public GameObject interior;
    public GameObject exterior;
    public GameObject frontOfDoor;

    public TaskInterface taskInterface;

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
