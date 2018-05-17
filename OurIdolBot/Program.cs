using OurIdolBot.Core;
using System;

namespace OurIdolBot
{
    class Program
    {
        static void Main(string[] args)
        {
            new Bot().Run();
            while (Console.ReadLine() != "quit") ;
        }
    }
}
