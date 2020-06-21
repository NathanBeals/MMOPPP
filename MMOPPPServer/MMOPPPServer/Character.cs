using Google.Protobuf.MMOPPP.Messages;
using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Dynamic;
using MMOPPPLibrary;

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
        public V3 m_Rotation = V3.Zero;
        public bool m_strafe = false;
        public bool m_sprint = false;

        public float m_TimeOfLastUpdate = -1; // Miliseconds since UTC Epoc
        public float m_TimeSinceLastUpdate = 0; // Miliseconds

        public Character(string Name, V3 Location = new V3(), V3 MoveInputs = new V3(), V3 Rotation = new V3(), bool Strafing = false, bool Sprinting = false)
        {
            m_Name = Name;
            m_Tags = "";
            m_Location = Location;
            m_Rotation = Rotation;
            m_MoveInputs = MoveInputs;
            m_strafe = Strafing;
            m_sprint = Sprinting;
        }

        public EntityUpdate ToEntityUpdate()
        {
            EntityUpdate update = new EntityUpdate();
            update.Id = new Identifier { Name = m_Name, Tags = m_Tags };
            update.Position = V3ToGV3(m_Location);
            update.PredictiveInputs = new EntityInput
            {
                Strafe = false,
                Sprint = false,
                EulerRotation = V3ToGV3(m_Rotation),
                DirectionInputs = V3ToGV3(m_MoveInputs)
            };

            return update;
        }

        public void Update(PlayerInput Input, float DeltaTime)
        {
            if (DeltaTime < 0) //HACK: this should never be possible
                return;

            var moveInput = GV3ToV3(Input.MoveInput.DirectionInputs);
            var rotationInput = GV3ToV3(Input.MoveInput.DirectionInputs);

            moveInput = V3.Multiply(moveInput, Constants.CharacterMoveSpeed * DeltaTime);
            m_Location = m_Location + moveInput;

            Console.WriteLine($"{Input.Id.Name} is now at {m_Location}");
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
