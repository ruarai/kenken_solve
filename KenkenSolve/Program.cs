using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KenkenSolve
{
    class Program
    {
        static void Main(string[] args)
        {
            Puzzle p = new Puzzle("game.txt");

            p.Print();

            Console.ReadKey();
        }
    }

}
