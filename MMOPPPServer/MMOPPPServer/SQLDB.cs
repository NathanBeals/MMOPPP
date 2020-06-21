using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Data.Sqlite;

namespace MMOPPPServer
{
    class SQLDB
    {
        SqliteConnection connection = new SqliteConnection("Data Source=Characters.db");

        public SQLDB()
        {
            connection.Open();
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

        public void SaveCharacterData(string Name)
        {
            if (!GetCharacterExists(Name))
            { 
            
            }


        }

        ~SQLDB()
        {
            connection.Close();
        }

        void TryCreateCharacterTable()
        {
            // Create the character table if it doesn't exist already
            try
            {
                var command = connection.CreateCommand();
                command.CommandText =
                    @"
                        CREATE TABLE CharacterData(
                        Name  TEXT NOT NULL UNIQUE,
                        LocationX REAL,
                        LocationY REAL,
                        LocationZ REAL,
                        RotationX REAL,
                        RotationY REAL,
                        RotationZ REAL,
                        PRIMARY KEY(Name)
                    );";
                command.ExecuteReader();
            }
            catch (SqliteException e)
            {
                if (!e.Message.Contains("already exists")) // Ignore table already exists errors
                    Console.WriteLine($"{e}");
            }
        }

        void test()
        {
            var command = connection.CreateCommand();

            command.CommandText =
            @"
                SELECT LocationX
                FROM CharacterData
                WHERE Name = $Name
                ";
            command.Parameters.AddWithValue("$Name", "Test");

            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var name = reader.GetFloat(0);
                    Console.WriteLine($"Hello, {name}!");
                }
            }
        }
    }

}
