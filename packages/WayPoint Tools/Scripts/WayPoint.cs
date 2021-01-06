using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Waypoint defines a position and an orientation via the position and look-point
// Because we now have more than 1 piece of data, it makes sense to wrap the data
// inside a class for easier management in lists etc
[System.Serializable]
public class WayPoint
{
    // Where to be
    public Vector3 position;
    // Where to look
    public Vector3 lookPoint;


    // Class constructor
    public WayPoint(Vector3 _position, Vector3 _lookPoint)
    {
        position = _position;
        lookPoint = _lookPoint;
    }

}
