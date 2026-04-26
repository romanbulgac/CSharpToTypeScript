using System;
using System.Text.Json;

class Program
{
    static void Main()
    {
        Console.WriteLine(JsonNamingPolicy.CamelCase.ConvertName("IPAddress"));
        Console.WriteLine(JsonNamingPolicy.CamelCase.ConvertName("HTTPResponse"));
        Console.WriteLine(JsonNamingPolicy.CamelCase.ConvertName("MyProperty"));
        Console.WriteLine(JsonNamingPolicy.CamelCase.ConvertName("JSON Value"));
    }
}
