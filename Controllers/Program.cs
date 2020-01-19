using System;

namespace Controllers
{
    class Program
    {

        static void Main(string[] args)
        {

           // var CCRC = new Controllers("CCRC1.txt");
            args = Environment.GetCommandLineArgs();
            var CCRC = new Controllers(args[1]);
            Console.ReadLine();
        }
    }
}
