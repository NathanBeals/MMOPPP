using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Google.Protobuf;
using Google.Protobuf.Examples.AddressBook;
using System.IO;

public class MessageTest : MonoBehaviour
{
    void Awake()
    {
        Person john = new Person
        {
            Id = 1234,
            Name = "John Doe",
            Email = "jdoe@example.com",
            Phones = { new Person.Types.PhoneNumber { Number = "555-4321", Type = Person.Types.PhoneType.Home } }
        };

        using (var output = File.Create("john.dat"))
        {
            john.WriteTo(output);
        }

        Person dave;

        using (var input = File.OpenRead("john.dat"))
        {
            dave = Person.Parser.ParseFrom(input);
        }

        if (dave.Name == "John Doe")
            Debug.Log("Success!!!");

        File.Delete("john.dat");
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
