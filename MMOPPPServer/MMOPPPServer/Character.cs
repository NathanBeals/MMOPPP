using Google.Protobuf.MMOPPP.Messages;
using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace MMOPPPServer
{
    using V3 = System.Numerics.Vector3;
    using GV3 = Google.Protobuf.MMOPPP.Messages.Vector3;

    class Character
    {
        string m_Name;
        string m_Tags;
        V3 m_Location;
        V3 m_MoveInputs;
        V3 m_Rotation;
        bool m_strafe = false;
        bool m_sprint = false;

        EntityUpdate MakeEnityUpdate()
        {
            EntityUpdate update = new EntityUpdate();
            update.Id = new Identifier { Name = m_Name, Tags = m_Tags };
            update.Position = Vector3ToGVector3(m_MoveInputs);
            update.PredictiveInputs = new EntityInput
            {
                Strafe = false,
                Sprint = false,
                EulerRotation = Vector3ToGVector3(m_Rotation),
                DirectionInputs = Vector3ToGVector3(m_MoveInputs)
            };

            return update;
        }

        // Helper
        public static GV3 Vector3ToGVector3(V3 VInput)
        {
            return new GV3 { X = VInput.X, Y = VInput.Y, Z = VInput.Z };
        }
    }
}
