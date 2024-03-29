using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Seek : SteeringBehavior
{
    public Kinematic character;
    public GameObject target;

    public float maxAcceleration = 100f;

    public bool flee = false;

    protected virtual Vector3 getTargetPosition(out bool valid)
    {
        if (target != null)
        {
            valid = true;
            return target.transform.position;
        }
        else
        {
            valid = false;
            return Vector3.positiveInfinity;
        }
    }

    public override SteeringOutput getSteering()
    {
        SteeringOutput result = new SteeringOutput();
        bool valid = true;
        Vector3 targetPosition = getTargetPosition(out valid);
        if (targetPosition == Vector3.positiveInfinity)
        {
            return null;
        }
        else if (!valid)
        {
            // If this case occurs, then the valid flag was deliberately set to false
            // In this case, do nothing
            result.linear = Vector3.zero;
            result.angular = 0;
            return result;
        }

        // Get the direction to the target
        if (flee)
        {
            //result.linear = character.transform.position - target.transform.position;
            result.linear = character.transform.position - targetPosition;
        }
        else
        {
            //result.linear = target.transform.position - character.transform.position;
            result.linear = targetPosition - character.transform.position;
        }

        // give full acceleration along this direction
        result.linear.Normalize();
        result.linear *= maxAcceleration;

        result.angular = 0;
        return result;
    }
}
