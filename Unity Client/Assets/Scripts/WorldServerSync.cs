﻿using System;
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

    // What will store the results from the server, TODO: likely to be replaced by the constructed Json stuff
    public struct CharacterDownlinkData
    {
        public string m_Name; // TODO: consider replacing with ID instead of name
        public Vector3 m_Location; // Location in worldspace
        public Vector3 m_Rotation; // Direction of the body

        // TODO: use these for client side interpolation
        public Vector2 m_MovementInput;
        public Vector3 m_CameraRotation; // Direction of the body
    }

    public static void QueueNewUpdateData(List<CharacterDownlinkData> Data)
    {
        s_Instance.m_DataFromServer.Enqueue(Data);
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
            if (!CharacterManager.GetCharacters().ContainsKey(characterData.m_Name) || CharacterManager.GetCharacters()[characterData.m_Name] == null)
            {
                CharacterManager.GetCharacters().Remove(characterData.m_Name);
                CharacterSpawner.SpawnCharacter(characterData.m_Name);
            }

            Character test = CharacterManager.GetCharacters()[characterData.m_Name];

            UpdateCharacter(test, characterData);
        }

        m_DataFromServer.Dequeue();
    }

    //TODO: add banding mechanism
    void UpdateCharacter(Character UpdateCharacter, CharacterDownlinkData Data)
    {
        var cGameObjTrans = UpdateCharacter.gameObject.transform;

        cGameObjTrans.position = Data.m_Location;
        cGameObjTrans.rotation = Quaternion.Euler(Data.m_Rotation);
    }

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


}
