using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace MMOPPPLibrary
{
  using SNV3 = System.Numerics.Vector3;
  using GV3 = Google.Protobuf.MMOPPP.Messages.Vector3;
  using ClientInput = Google.Protobuf.MMOPPP.Messages.ClientInput;

  public class CharacterController
  {
    public delegate void OnPositionCalculated(SNV3 Position);

    public static void ApplyInputs(List<ClientInput> Inputs, SNV3 CurrentPosition, OnPositionCalculated ReconciliationFunction)
    {
      if (Inputs.Count == 0)
        return;

      float timeOfLastUpdate = Inputs[0].Input.SentTime.Nanos;
      var sumOfMovements = new SNV3();

      foreach (var input in Inputs)
      {
        float deltaTime = ((input.Input.SentTime.Nanos - timeOfLastUpdate) / 1000000) / 1000; //HACK: math issue, the first input will be ignored and all deltatimes will be offset by 1
        timeOfLastUpdate = input.Input.SentTime.Nanos;

        if (deltaTime <= 0.0f)
          continue;

        var moveInputs = GV3ToSNV3(input.Input.PlayerMoveInputs);
        var bodyRotation = GV3ToSNV3(input.Input.EulerBodyRotation);
        var cameraRotation = GV3ToSNV3(input.Input.EulerCameraRotation);

        //forward * rotation * move input
        var forward = moveInputs.Z;
        var right = moveInputs.X;

        // Hack: I can't seem to use MathF here for some reason? I'm using it in MMOPPPServer.
        moveInputs.Z = (float)Math.Cos(cameraRotation.Y * (float)Math.PI / 180.0f) * forward;
        moveInputs.X = (float)Math.Sin(cameraRotation.Y * (float)Math.PI / 180.0f) * forward;
        moveInputs.Z += (float)Math.Cos((cameraRotation.Y + 90) * (float)Math.PI / 180.0f) * right;
        moveInputs.X += (float)Math.Sin((cameraRotation.Y + 90) * (float)Math.PI / 180.0f) * right;

        moveInputs = moveInputs * MMOPPPLibrary.Constants.CharacterMoveSpeed * deltaTime * 1000;
        sumOfMovements += moveInputs;
      }

      CurrentPosition = CurrentPosition + sumOfMovements;
      ReconciliationFunction(CurrentPosition);
    }

    // Helper
    public static GV3 V3ToGV3(SNV3 VInput)
    {
      return new GV3 { X = VInput.X, Y = VInput.Y, Z = VInput.Z };
    }

    public static SNV3 GV3ToSNV3(GV3 VInput)
    {
      return new SNV3 { X = VInput.X, Y = VInput.Y, Z = VInput.Z };
    }
  }
}
