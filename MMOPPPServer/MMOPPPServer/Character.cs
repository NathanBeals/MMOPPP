using Google.Protobuf.MMOPPP.Messages;
using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Dynamic;
using MMOPPPLibrary;
using System.ComponentModel.DataAnnotations;

namespace MMOPPPServer
{
  using V3 = System.Numerics.Vector3;
  using GV3 = Google.Protobuf.MMOPPP.Messages.Vector3;

  class Character
  {
    public string m_Name = "";
    public string m_Tags = "";
    public V3 m_Location = V3.Zero;
    public V3 m_MoveInputs = V3.Zero;
    public V3 m_CameraRotation = V3.Zero;
    public V3 m_BodyRotation = V3.Zero;
    public bool m_strafe = false;
    public bool m_sprint = false;

    public float m_TimeOfLastUpdate = -1; // Miliseconds since UTC Epoc
    public float m_TimeSinceLastUpdate = 0; // Miliseconds

    public ServerUpdate m_ServerUpdate;

    public Character(string Name, V3 Location = new V3(), V3 MoveInputs = new V3(), V3 BodyRotation = new V3(), V3 CameraRotation = new V3(), bool Strafing = false, bool Sprinting = false)
    {
      m_Name = Name;
      m_Tags = "";
      m_Location = Location;
      m_MoveInputs = MoveInputs;
      m_BodyRotation = BodyRotation;
      m_CameraRotation = CameraRotation;
      m_strafe = Strafing;
      m_sprint = Sprinting;

      ServerUpdateReset();
    }


    public ServerUpdate GetServerUpdate()
    {
      return m_ServerUpdate;
    }

    public void ServerUpdateReset()
    {
      m_ServerUpdate = new ServerUpdate();
      m_ServerUpdate.Name = m_Name;
    }

    public void ServerUpdateAppendInputs(ClientInput PastInput)
    {
      var tempInput = new Input
      {
        Strafe = false,
        Sprint = false,
        PlayerMoveInputs = V3ToGV3(m_MoveInputs),
        EulerBodyRotation = V3ToGV3(m_BodyRotation),
        EulerCameraRotation = V3ToGV3(m_CameraRotation),
        SentTime = PastInput.Input.SentTime
      };

      m_ServerUpdate.PastInputs.Add(tempInput);
    }

    public void ServerUpdatePositionUpdate()
    {
      m_ServerUpdate.Location = V3ToGV3(m_Location);
    }

    public void ServerUpdateBodyRotationUpdate()
    {
      m_ServerUpdate.BodyRotation = V3ToGV3(m_BodyRotation);
    }

    public void Update(ClientInput Input, float DeltaTime)
    {
      if (DeltaTime < 0.0f) //TODO: still not sure why this happens
        return;

      m_MoveInputs = GV3ToV3(Input.Input.PlayerMoveInputs);
      m_BodyRotation = GV3ToV3(Input.Input.EulerBodyRotation);
      m_CameraRotation = GV3ToV3(Input.Input.EulerCameraRotation);

      ServerUpdateAppendInputs(Input);

      //forward * rotation * move input
      var forward = m_MoveInputs.Z;
      var right = m_MoveInputs.X;
      m_MoveInputs.Z = MathF.Cos(m_CameraRotation.Y * (float)Math.PI / 180.0f) * forward;
      m_MoveInputs.X = MathF.Sin(m_CameraRotation.Y * (float)Math.PI / 180.0f) * forward;
      m_MoveInputs.Z += MathF.Cos((m_CameraRotation.Y + 90) * (float)Math.PI / 180.0f) * right;
      m_MoveInputs.X += MathF.Sin((m_CameraRotation.Y + 90) * (float)Math.PI / 180.0f) * right;

      m_MoveInputs = V3.Multiply(m_MoveInputs, Constants.CharacterMoveSpeed * DeltaTime);
      m_Location = m_Location + m_MoveInputs;

      ServerUpdatePositionUpdate();
      ServerUpdateBodyRotationUpdate();
    }

    // Helper
    public static GV3 V3ToGV3(V3 VInput)
    {
      return new GV3 { X = VInput.X, Y = VInput.Y, Z = VInput.Z };
    }

    public static V3 GV3ToV3(GV3 VInput)
    {
      return new V3 { X = VInput.X, Y = VInput.Y, Z = VInput.Z };
    }
  }
}
