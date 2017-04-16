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

            p.Print(c => c.Group.Goal.ToString() + getBehaviourChar(c.Group.Behaviour));

            PuzzleSolve.Solve(p);

            p.Print(c => c.Value.ToString());
            p.Print(c => c.Group.Goal.ToString() + getBehaviourChar(c.Group.Behaviour));


            Console.WriteLine();

            Console.ReadKey();
        }



        static char getBehaviourChar(Behavior b)
        {
            switch (b)
            {
                case Behavior.Multiply:
                    return 'x';
                case Behavior.Divide:
                    return '/';
                case Behavior.Add:
                    return '+';
                case Behavior.Subtract:
                    return '-';
                case Behavior.Constant:
                    return 'c';
            }
            return ' ';
        }
    }

}
