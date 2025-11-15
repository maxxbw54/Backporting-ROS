using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace JsonViz.Actors
{
    public static class Vizualizer
    {
        public static void VizProgram()
        {
            while (true)
            {
                Console.WriteLine("------------INPUT----------------");
                string input = Console.ReadLine();
                Console.WriteLine("------------END INPUT----------------");
                Console.WriteLine("------------OUTPUT-----------------");
                try
                {
                    Console.WriteLine(JsonSerializer.Deserialize<string>(input));
                }
                catch (Exception e)
                {
                    Console.WriteLine("Failed");
                }


                Console.WriteLine("------------END OUTPUT-----------------");
            }
        }

        public static void ClearScreen()
        {
            Console.WriteLine("\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n");
        }


        public static void PrintChanges(this string changes)
        {
            string[] lines = changes.Split("\n");
            for (int i = 0; i < lines.Length; i++)
            {
                Console.ForegroundColor = lines[i].FirstOrDefault() switch
                {
                    '+' => ConsoleColor.Green,
                    '-' => ConsoleColor.Red,
                    '@' => ConsoleColor.Blue,
                    _ => ConsoleColor.White
                };
                Console.BackgroundColor = lines[i].FirstOrDefault() switch
                {
                    _ => ConsoleColor.Black
                };
                Console.WriteLine(lines[i]);
            }
            Console.ResetColor();
        }
    }
}
