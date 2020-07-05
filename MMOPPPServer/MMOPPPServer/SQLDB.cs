using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Google.Protobuf.MMOPPP.Messages;
using Microsoft.Data.Sqlite;

namespace MMOPPPServer
{
    using V3 = Google.Protobuf.MMOPPP.Messages.Vector3;

    class SQLDB
    {
        SqliteConnection connection = new SqliteConnection("Data Source=Characters.db");

        public SQLDB()
        {
            connection.Open();
        }

        ~SQLDB()
        {
            connection.Close();
        }

        public void Initialize()
        {
            TryCreateCharacterTable();
        }

        public bool GetCharacterExists(string Name)
        {
            var command = connection.CreateCommand();

            command.CommandText =
            @"
                SELECT 1
                FROM CharacterData
                WHERE Name=$Name;
                ";
            command.Parameters.AddWithValue("$Name", Name);

            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    return reader.GetInt16(0) == 1.0f;
                }
            }

            return false;
        }

        public void SaveCharacterData(string Name, V3 Location, V3 Rotation)
        {
            if (!GetCharacterExists(Name))
            {
                var command = connection.CreateCommand();
                command.CommandText = @"INSERT INTO CharacterData(Name) VALUES($Name);";
                command.Parameters.AddWithValue("$Name", Name);
                command.ExecuteNonQuery();
            }

            {
                var command = connection.CreateCommand();
                command.CommandText = @$"
                UPDATE CharacterData SET 
                LocationX = {Location.X}, 
                LocationY = {Location.Y},
                LocationZ = {Location.Z},
                RotationX = {Rotation.X}, 
                RotationY = {Rotation.Y},
                RotationZ = {Rotation.Z}
                WHERE Name = $Name";
                command.Parameters.AddWithValue("$Name", Name);
                command.ExecuteNonQuery();
            }
        }

        public void LoadCharacterData(string Name, out V3 Location, out V3 Rotation)
        {
            Location = new V3 { };
            Rotation = new V3 { };

            if (!GetCharacterExists(Name))
                return;

            var command = connection.CreateCommand();
            command.CommandText = @$"
            SELECT * FROM CharacterData WHERE Name = $Name";
            command.Parameters.AddWithValue("$Name", Name);

            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    Location.X = reader.GetFloat(1);
                    Location.Y = reader.GetFloat(2);
                    Location.Z = reader.GetFloat(3);

                    Rotation.X = reader.GetFloat(4);
                    Rotation.Y = reader.GetFloat(5);
                    Rotation.Z = reader.GetFloat(6);
                }
            }
        }

        void TryCreateCharacterTable()
        {
            var command = connection.CreateCommand();
            command.CommandText =
                @"
                CREATE TABLE if not exists CharacterData(
                Name  TEXT NOT NULL UNIQUE,
                LocationX REAL,
                LocationY REAL,
                LocationZ REAL,
                RotationX REAL,
                RotationY REAL,
                RotationZ REAL,
                PRIMARY KEY(Name)
                );";
            command.ExecuteNonQuery();
        }
    }

}
