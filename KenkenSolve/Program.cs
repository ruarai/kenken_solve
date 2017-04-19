using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            Puzzle p = new Puzzle("game5.txt");


            p.Print(c => c.Group.Goal.ToString() + c.Group.CharCode);

            Stopwatch s = new Stopwatch();
            s.Start();

            PuzzleSolve.Solve(p);

            s.Stop();
            Console.WriteLine(s.ElapsedMilliseconds);
            
            p.Print(c => c.Value.ToString());


            Console.WriteLine();

            Console.ReadKey();
        }
    }

}
