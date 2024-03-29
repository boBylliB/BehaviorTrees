using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Face : Align
{
    // returns the position that we want to face towards
    // Face will rotate to match the given target position
    // sub-classes can overwrite this function to set a different target position e.g. to face a non-game-object
    public virtual Vector3 getTargetPosition()
    {
        return target.transform.position;
    }
    // override Align's getTargetAngle to face the target instead of matching it's orientation
    public override float getTargetAngle()
    {
        // Get direction vector to target
        Vector3 difference = getTargetPosition() - character.transform.position;
        // If the direction vector is somehow zero, do not change the orientation, and return early to not error later
        if (difference.magnitude == 0)
        {
            return character.transform.eulerAngles.y;
        }
        // Set the target angle by calculating the angle of that vector, and then converting to degrees
        float targetAngle = Mathf.Atan2(difference.x, difference.z) * Mathf.Rad2Deg;

        return targetAngle;
    }
}
