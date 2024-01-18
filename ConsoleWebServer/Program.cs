using System;
using Bare.WebServer;
using Bare.Extensions;
using System.ComponentModel.DataAnnotations;

namespace ConsoleWebServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");
            Server.Start();
            Console.ReadLine();
            
        }
    }
}