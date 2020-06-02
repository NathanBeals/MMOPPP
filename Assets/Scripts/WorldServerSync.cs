using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.Events;
using UnityEngine.PlayerLoop;

public class WorldServerSync : MonoBehaviour
{
    Queue<List<CharacterDownlinkData>> m_DataFromServer;

    // What will store the results from the server, TODO: likely to be replaced by the constructed Json stuff
    public struct CharacterDownlinkData
    {
        public string Name; // TODO: consider replacing with ID instead of name
        public Vector3 Location; // Location in worldspace
        public Vector3 Rotation; // Direction of the body

        // TODO: use these for client side interpolation
        public Vector2 MovementInput;
        public Vector3 CameraRotation; // Direction of the body
    }

    void QueueNewUpdateData(List<CharacterDownlinkData> data)
    {
        m_DataFromServer.Enqueue(data);
    }

    void SyncTransformData()
    {
        var updateData = m_DataFromServer.Peek();

        foreach (var characterData in updateData)
        {
            // If the character does not exist locally, spawn it
            // HACK: slows the main loop
            if (!CharacterManager.GetCharacters().ContainsKey(characterData.Name))
                CharacterSpawner.SpawnCharacter(characterData.Name);

            UpdateCharacter(CharacterManager.GetCharacters()[characterData.Name], characterData);
        }

        // For each character in server data, set rotation, set stransform (Allow characters to respond with their own smoothing, banding)
    }

    void UpdateCharacter(Character character, CharacterDownlinkData data)
    {
        var cGameObjTrans = character.gameObject.transform;

        cGameObjTrans.position = data.Location;
        cGameObjTrans.rotation = Quaternion.Euler(data.Rotation);
    }
}
