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
            string[] lines = File.ReadAllLines("game.txt");

            int size = int.Parse(lines[0]);
            char[,] grid = new char[size, size];

            for (int y = 0; y < size; y++)
            {
                string row = lines[1 + y];//Take a row from the game file
                for (int x = 0; x < size; x++)
                    grid[x, y] = row[x];
            }
            
            Puzzle puzzle = new Puzzle();
            puzzle.Size = size;

            for (int i = size+1; i < lines.Length; i++)
            {
                Span span = new Span();

                var pieces = lines[i].Split(';');

                span.Character = pieces[0][0];
                span.N = int.Parse(pieces[1]);
                span.Behaviour = GetBehaviour(pieces[2][0]);

                puzzle.Groups.Add(span);
            }


            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    char c = grid[x, y];
                    Cell cell = new Cell();

                    cell.X = x;
                    cell.Y = y;

                    //build a bidirectional relationship between groups and cells
                    cell.Group = puzzle.Groups.FirstOrDefault(g => g.Character == c);
                    cell.Group.Cells.Add(cell);

                    puzzle.All.Cells.Add(cell);
                }
            }

            for (int i = 0; i < size; i++)
            {
                Span column = new Span {Cells = puzzle.All.Cells.Where(c => c.X == i).ToList()};
                Span row = new Span { Cells = puzzle.All.Cells.Where(c => c.Y == i).ToList() };

                puzzle.Columns.Add(column);
                puzzle.Rows.Add(row);
            }
            PrintPuzzle(puzzle);


            Console.ReadKey();
        }
        public static Behavior GetBehaviour(char c)
        {
            switch (c)
            {
                case 'x':
                    return Behavior.Multiply;
                case '/':
                    return Behavior.Divide;
                case '+':
                    return Behavior.Add;
                case '-':
                    return Behavior.Subtract;
                case 'c':
                    return Behavior.Constant;
            }
            return Behavior.Unique;//Shouldn't happen anyway
        }

        public static void PrintPuzzle(Puzzle puzzle)
        {
            foreach (var puzzleRow in puzzle.Rows)
            {
                foreach (var cell in puzzleRow.Cells)
                {
                    Console.Write(cell.Group.N.ToString().PadLeft(3, '0'));
                    Console.Write("|");
                }
                Console.WriteLine();
                Console.WriteLine("".PadLeft(4 * (puzzle.Size), '-'));
            }
        }
    }

    class Puzzle
    {
        public Span All = new Span();

        public List<Span> Groups = new List<Span>();

        public List<Span> Columns = new List<Span>();
        public List<Span> Rows = new List<Span>();

        public int Size;
    }

    class Span//Collection of cells, may be a row, column or a group
    {
        public List<Cell> Cells = new List<Cell>();
        public Behavior Behaviour;

        public int N;
        public char Character;
    }

    class Cell
    {
        public Span Column;
        public Span Row;
        public Span Group;

        public int X;
        public int Y;
    }

    enum Behavior
    {
        Multiply,//Combination of cells multiply to n
        Divide,//Permutation of cells divide to n
        Add,//Combination of cells add to n
        Subtract,//Permutation of cells subtract to n
        Constant,//All cells are always some n
        Unique//All cells form a distinct set of a sequence 1, 2, ..., n
    }

}
