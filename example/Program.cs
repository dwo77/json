using System;
using System.IO;
using dwo.Json;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            string json_string = File.ReadAllText("example.json");

            JsonObject jo = new JsonObject();
            jo.Parse(json_string);

            JsonObject glossary = jo.GetValue("glossary").ObjectVal;
            Console.WriteLine(glossary.GetValue("title").StrVal);

        }
    }
}
