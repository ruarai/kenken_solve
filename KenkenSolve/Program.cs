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
            Puzzle p = new Puzzle("game4.txt");

            p.Print(c => c.Group.Goal.ToString() + c.Group.CharCode);

            PuzzleSolve.Solve(p);
            
            p.Print(c => c.Value.ToString());
            p.Print(c => c.Group.Goal.ToString() + c.Group.CharCode);


            Console.WriteLine();

            Console.ReadKey();
        }
    }

}
