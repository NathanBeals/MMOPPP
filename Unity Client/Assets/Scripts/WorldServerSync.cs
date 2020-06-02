using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.Events;
using UnityEngine.PlayerLoop;

public class WorldServerSync : MonoBehaviour
{
    public static WorldServerSync s_Instance;

    Queue<List<CharacterDownlinkData>> m_DataFromServer = new Queue<List<CharacterDownlinkData>>();

    public void Awake()
    {
        if (s_Instance == null)
        {
            s_Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (s_Instance != this)
            Destroy(gameObject);
    }

    public void FixedUpdate()
    {
        //TODO: convert to an attempt to sync, axing on a try lock
        SyncTransformData();
    }

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

    public static void QueueNewUpdateData(List<CharacterDownlinkData> data)
    {
        s_Instance.m_DataFromServer.Enqueue(data);
    }

    void SyncTransformData()
    {
        if (m_DataFromServer.Count <= 0)
            return;

        var updateData = m_DataFromServer.Peek();

        foreach (var characterData in updateData)
        {
            // If the character does not exist locally, spawn it
            // HACK: slows the main loop
            if (!CharacterManager.GetCharacters().ContainsKey(characterData.Name) || CharacterManager.GetCharacters()[characterData.Name] == null)
            {
                CharacterManager.GetCharacters().Remove(characterData.Name);
                CharacterSpawner.SpawnCharacter(characterData.Name);
            }

            Character test = CharacterManager.GetCharacters()[characterData.Name];

            UpdateCharacter(test, characterData);
        }

        m_DataFromServer.Dequeue();
    }

    //TODO: add banding mechanism
    void UpdateCharacter(Character character, CharacterDownlinkData data)
    {
        var cGameObjTrans = character.gameObject.transform;

        cGameObjTrans.position = data.Location;
        cGameObjTrans.rotation = Quaternion.Euler(data.Rotation);
    }
}
