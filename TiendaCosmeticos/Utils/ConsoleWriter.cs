using TiendaCosmeticos.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TiendaCosmeticos.Util
{
    public class ConsoleWriter : IConsoleWriter
    {
        public void DisplayInitialMessage(string name)
        {
            int totalWidth = 35;
            char filler = '*';
            char subLine = '-';

            string centeredMessage = name
                .PadLeft(((totalWidth - name.Length) / 2) + name.Length, filler)
                .PadRight(totalWidth, filler);

            string bottomLine = new string(subLine, totalWidth);

            Console.WriteLine(centeredMessage);
            Console.WriteLine(bottomLine);
        }

        public void DisplayMenu(string headMessage, params string[] options)
        {
            int i = 1;
            Console.WriteLine(headMessage);
            foreach (var item in options)
            {
                Console.WriteLine($"{i}. {item}");
                i++;
            }
            Console.WriteLine(new string('-', 35));
        }

        public void ShowHeader(string title)
        {
            Console.WriteLine("\n=======================================================================================================");
            int totalWidth = 103;
            int padding = (totalWidth - title.Length) / 2;
            string centered = title.PadLeft(padding + title.Length).PadRight(totalWidth);
            Console.WriteLine(centered);
            Console.WriteLine("=======================================================================================================");
        }

        public void ShowMessage(string message)
        {
            Console.WriteLine(message);
        }

        public void ShowError(string error)
        {
            Console.WriteLine($"\nError: {error}");
        }

        public void WaitForKey(string prompt = "Presione cualquier tecla para continuar...")
        {
            Console.WriteLine($"\n{prompt}");
            Console.ReadKey();
        }

        public void Clear()
        {
                Console.Clear();    
        }
    }
}
