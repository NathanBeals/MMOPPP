using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class WorldServerSync : MonoBehaviour
{
    // What will store the results from the server, TODO: likely to be replaced by the constructed Json stuff
    struct CharacterDownlinkData
    {
        string Name; // HACK: I am considering having this as an id instead of a string, but I'd have to hash it on this end, and I'm uncertain the best way to do that but I'm pretty sure it's needed, I don't know, it seems like it would be fine.
        Vector3 Location; // Location in worldspace
        Vector3 Rotation; // Direction of the body
        //Vector3 Scale; // TODO: likely won't be used, used more for transform completeness

        Vector2 Input;
        Vector3 LookRotation; // Camera rotation (which drives the movement)
    }

    void SyncTransformData()
    {
        // For each character in server data, set rotation, set stransform (Allow characters to respond with their own smoothing, banding)
    }
}
