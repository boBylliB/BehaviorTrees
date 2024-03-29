using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserControlled : SteeringBehavior
{
    public Kinematic character;
    public float maxAcceleration = 1f;
    public float frictionPercent = 1f;

    public override SteeringOutput getSteering()
    {
        SteeringOutput output = new SteeringOutput();

        Vector3 requestedMotion = Vector3.zero;
        int xReq = 0;
        int zReq = 0;

        //if (Input.GetKey("w")) requestedMotion += character.transform.forward;
        //if (Input.GetKey("a")) requestedMotion -= character.transform.right;
        //if (Input.GetKey("s")) requestedMotion -= character.transform.forward;
        //if (Input.GetKey("d")) requestedMotion += character.transform.right;

        if (Input.GetKey("w")) zReq += 1;
        if (Input.GetKey("a")) xReq -= 1;
        if (Input.GetKey("s")) zReq -= 1;
        if (Input.GetKey("d")) xReq += 1;

        // Apply friction first
        // We set this by projecting the current velocity onto the forward and right vectors to get an x and z component
        requestedMotion.z -= Vector3.Dot(character.linearVelocity, character.transform.forward);
        requestedMotion.x -= Vector3.Dot(character.linearVelocity, character.transform.right);
        // Normalize and scale it by the friction percent
        requestedMotion.Normalize();
        requestedMotion *= frictionPercent;
        // Override the requested motion if that direction is pressed
        requestedMotion.x = xReq == 0 ? requestedMotion.x : xReq;
        requestedMotion.z = zReq == 0 ? requestedMotion.z : zReq;
        // Apply the acceleration back onto the forward and right vectors
        output.linear += character.transform.forward * requestedMotion.z;
        output.linear += character.transform.right * requestedMotion.x;
        // Scale it by the max acceleration
        output.linear *= maxAcceleration;

        return output;
    }
}
